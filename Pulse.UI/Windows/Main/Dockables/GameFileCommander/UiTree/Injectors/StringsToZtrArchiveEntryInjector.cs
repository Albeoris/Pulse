using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class StringsToZtrArchiveEntryInjector : IArchiveEntryInjector
    {
        public static bool ReplaceAnimatedText = false;

        public string SourceExtension => ".strings";

        public bool TryInject(IUiInjectionSource source, string sourceFullPath, ArchiveEntryInjectionData data, ArchiveEntry entry)
        {
            Dictionary<string, string> sourceEntries;
            using (Stream input = source.TryOpen(sourceFullPath))
            {
                if (input != null)
                {
                    string entryName;
                    ZtrTextReader reader = new ZtrTextReader(input, StringsZtrFormatter.Instance);
                    sourceEntries = reader.Read(out entryName).ToDictionary(e => e.Key, e => e.Value);
                    using (Stream output = data.OuputStreamFactory(entry))
                        Inject(data.Listing, entry, sourceEntries, output);

                    return true;
                }
            }

            sourceEntries = source.TryProvideStrings();
            if (sourceEntries == null)
                return false;

            using (Stream output = data.OuputStreamFactory(entry))
                Inject(data.Listing, entry, sourceEntries, output);

            return true;
        }

        private void Inject(ArchiveListing listing, ArchiveEntry entry, Dictionary<String, String> sourceEntries, Stream output)
        {
            ZtrFileType type = ZtrFileType.LittleEndianUncompressedDictionary;
            ZtrFileEntry[] targetEntries;
            using (Stream original = listing.Accessor.ExtractBinary(entry))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(original, InteractionService.TextEncoding.Provide().Encoding);
                targetEntries = unpacker.Unpack();

                if (InteractionService.GamePart == FFXIIIGamePart.Part2)
                {
                    if (entry.Name.StartsWith("txtres/resident/system/txtres_", StringComparison.InvariantCultureIgnoreCase))
                        type = ZtrFileType.BigEndianCompressedDictionary;
                }
            }

            MergeEntries(sourceEntries, targetEntries);

            ZtrFilePacker packer = new ZtrFilePacker(output, InteractionService.TextEncoding.Provide().Encoding, type);
            packer.Pack(targetEntries);
        }

        private static void MergeEntries(Dictionary<string, string> newEntries, ZtrFileEntry[] targetEntries)
        {
            StringBuilder sb = new StringBuilder(1024);
            foreach (ZtrFileEntry entry in targetEntries)
            {
                string oldText = entry.Value;
                string newText;
                if (!newEntries.TryGetValue(entry.Key, out newText))
                {
                    Log.Warning("[ArchiveEntryInjectorStringsToZtr] Unknown entry was skipped {0}={1}.", entry.Key, entry.Value);
                    continue;
                }

                entry.IsAnimatedText = TryReplaceAnimatedText(entry.Key, ref newText);

                GetEndingTags(oldText, sb);
                string oldEnding = sb.ToString();
                sb.Clear();

                int endingLength = GetEndingTags(newText, sb);
                int newLength = newText.Length - endingLength;
                sb.Clear();

                // Restore an old tails and new line tags
                bool cr = false;
                for (int i = 0; i < newLength; i++)
                {
                    char ch = newText[i];
                    switch (ch)
                    {
                        case '\n':
                        {
                            cr = false;
                            sb.Append(NewLineTag);
                            break;
                        }
                        case '\r':
                        {
                            cr = true;
                            break;
                        }
                        default:
                        {
                            if (cr)
                            {
                                sb.Append(NewLineTag);
                                cr = false;
                            }

                            sb.Append(ch);
                            break;
                        }
                    }
                }

                sb.Append(oldEnding);

                entry.Value = sb.ToString();

                // Many, article
                //int nameLength = entry.Value.IndexOf('{');
                //string name = nameLength < 0 ? entry.Value : entry.Value.Substring(0, nameLength);
                //if (oldText.Contains("{End}{Many}") && !entry.Value.Contains("{End}{Many}"))
                //    entry.Value += ("{End}{Many}" + name);
                //if (oldText.Contains("{End}{Article}") && !entry.Value.Contains("{End}{Article}"))
                //    entry.Value += ("{End}{Article}");

                sb.Clear();
            }
        }

        private static bool TryReplaceAnimatedText(string key, ref string newText)
        {
            if (!ReplaceAnimatedText)
                return false;

            string value;
            if (AnimatedText.TryGetValue(key, out value))
            {
                newText = value;
                return true;
            }
            return false;
        }

        private static int GetEndingTags(string text, StringBuilder sb)
        {
            int index = text.Length - 1;
            if (index < 0)
                return 0;

            while (text[index] == '}')
            {
                index = text.LastIndexOf('{', index - 1) - 1;
                if (index < 0)
                    break;
            }

            if (index == text.Length - 1)
                return 0;

            char[] chars = index < 0 ? text.ToCharArray() : text.ToCharArray(index + 1, text.Length - index - 1);

            int offset = 0;
            int left = chars.Length;
            int result = left;
            FFXIIITextTag tag;

            while (left > 0 && (tag = FFXIIITextTag.TryRead(chars, ref offset, ref left)) != null)
            {
                switch (tag.Code)
                {
                    case FFXIIITextTagCode.End:
                    case FFXIIITextTagCode.Escape:
                    case FFXIIITextTagCode.Text:
                        sb.Append(tag);
                        break;
                    default:
                        result = left;
                        sb.Clear();
                        break;
                }
            }

            if (left != 0)
                Log.Warning("[ArchiveEntryInjectorStringsToZtr.GetEndingTags] Unexpected escape sequence: {0}", text);

            return result;
        }

        private static readonly FFXIIITextTag NewLineTag = new FFXIIITextTag(FFXIIITextTagCode.Text, FFXIIITextTagText.NewLine);

        private static readonly Dictionary<string, string> AnimatedText = new Dictionary<string, string>
        {
            {"$btl_guard", "BLOCK"},
            {"$btl_help", "HELP"},
            {"$btl_keep", "SAFE"},
            {"$btl_limit", "TIME"},
            {"$btl_miss", "MISS"},

            {"$btl_atb", "ATB COST"},
            {"$btl_bonus", "BONUS"},
            {"$btl_break", "STAGGER"},
            {"$btl_chain", "CHAIN"},
            {"$btl_drive", "GESTALT"},
            {"$btl_driving_00", "GESTALT MODE"},

            {"$btl_mtxt_00", "COMMANDS"},
            {"$btl_mtxt_01", "ABILITIES"},
            {"$btl_mtxt_02", "TECHNIQUES"},
            {"$btl_mtxt_03", "ITEMS"},
            {"$btl_mtxt_04", "PARADIGM"},
            {"$btl_mtxt_05", "TARGET"},
            {"$btl_mtxt_06", "TARGET"},
            {"$btl_mtxt_07", "TARGET"},
            {"$btl_mtxt_08", "PRIMED"},
            {"$btl_mtxt_09", "SUMMON"},
            {"$btl_mtxt_10", "PARADIGM SHIFT"},

            {"$target", "TARGET"},
            {"$pre_mtxt_01" ,"PREEMPTIVE STRIKE!"},

            {"$f_abi_mtxt_00", "ABILITIES"},
            {"$f_abi_mtxt_01", "ABILITY"},
            {"$f_abi_mtxt_02", "ATB Cost"},
            {"$f_abi_mtxt_04", "HP"},
            {"$f_abi_mtxt_05", "TP LEVEL"},
            {"$f_abi_mtxt_06", "ROLE"},
            {"$f_abi_mtxt_07", "ROLE BONUS"},
            {"$f_abi_mtxt_08", "SP"},
            {"$f_abi_mtxt_09", "TP COST"},

            {"$f_equ_mtxt_02", "WEAPON"},
            {"$f_equ_mtxt_03", "ACCESSORY{End}{Many}ACCESSORIES"},
            {"$f_equ_mtxt_05", "WEAPONS"},
            {"$f_equ_mtxt_07", "ACCESSORIES"},
            {"$f_equ_mtxt_09", "EMPHASIS"},

            {"$libra_00", "LEVEL"},
            {"$pause_00", "PAUSED"},
            {"$tutorial_00", "TUTORIAL"},

            {"$m_atcl_chpt00", "CHAPTER"},
            {"$m_atcl_head00", "LOG ENTRY"},
            {"$m_atcl_mong00", "TYPE"},
            {"$m_atcl_mong01", "SUBTYPE"},
            {"$m_atcl_mong02", "LOG ENTRY"},
            {"$m_atcl_page00", "PAGE"},

            {"$m_res_fl_wmw03", "MISSION"},
            {"$m_res_fl_wmw04", "MARK"},
            {"$m_res_fl_wmw05", "CLASS"},
            {"$m_res_fl_wmw06", "RATING"},
            {"$m_res_fl_wmw11", "SLAIN!"},

            {"$m_res_fl_wmm03", "MISSION TITLE"},
            {"$m_res_fl_wmm05", "STONE LOCATION"},
            {"$m_res_fl_wmm07", "CLASS"},
            {"$m_res_fl_wmm09", "RATING"},

            {"$m_res_fl_x", "EXAMINE"},
            {"$m_res_mchg_in", "IN"},
            {"$m_res_mchg_out", "OUT"},
            {"$m_res_mchg_ttl", "BATTLE TEAM CHANGED"},

            {"$m_res_fl_mss", "EXAMINE"},
            {"$m_res_fl_mss2", "MARK"},
            {"$m_res_fl_op2", "EXAMINE"},
            {"$m_res_fl_save", "ACCESS"},
            {"$m_res_fl_tlk", "TALK"},
            {"$m_res_fl_tre", "OPEN"},
            {"$m_res_fl_trel", "GUARDED"},

            {"$m_res_cho_mes0", "MORALE"},
            {"$m_res_ms_start", "MISSION COMMENCED!"},
            {"$m_res_vb_mes0", "POWER"},

            {"$res_mtxt_00", "SHROUDS"},
            {"$resms_txt00", "MISSION COMPLETE!"},
            {"$resms_txt01", "MISSION RESULTS"},

            {"$role_change", "PARADIGM SHIFT"},
            {"$role_lv", "LEVEL"},

            {"$growth_00", "CRYSTARIUM EXPANDED!"},
            {"$growth_01", "ROLE DEVELOPMENT UNLOCKED!"},

            {"$f_con_mtxt_01", "OPTION"},
            {"$f_con_mtxt_02", "CURRENT SETTING"},

            {"$f_cus_def_02", "LEVEL IMPROVED"},
            {"$f_cus_def_03", "TYPE MODIFIED"},

            {"$f_cus_mtxt_00", "INVENTORY"},
            {"$f_cus_mtxt_01", "POWER"},
            {"$f_cus_mtxt_02", "CUSTOMIZATION"},
            {"$f_cus_mtxt_03", "BONUS:"},
            {"$f_cus_mtxt_04", "EXP:"},
            {"$f_cus_mtxt_06", "METHOD"},
            {"$f_cus_mtxt_07", "WEAPONS"},
            {"$f_cus_mtxt_08", "ACCESSORIES"},
            {"$f_cus_mtxt_09", "COMPONENTS"},

            {"$f_cus_mtxt_13", "EXP BONUS:"},
            {"$f_cus_mtxt_14", "NEXT BONUS!"},

            {"$f_cry_inf_01", "CRYSTARIUM"},
            {"$f_cry_inf_02", "CRYSTAL"},
            {"$f_cry_inf_03", "CP COST"},
            {"$f_cry_inf_04", "TOTAL COST"},

            {"$f_ite_mtxt_03", "ITEM"},
            {"$f_ite_mtxt_04", "QUANTITY"},
            {"$f_ite_mtxt_06", "METHOD"},
            {"$f_ite_mtxt_07", "EQUIPPED"},

            {"$f_mtxt_02", "PARADIGM"},
            {"$f_mtxt_03", "CRYSTARIUM"},
            {"$f_mtxt_04", "TECHNICAL POINTS"},

            {"$f_opt_mtxt_00", "PARADIGMS"},
            {"$f_opt_mtxt_01", "ROLE LEVEL"},
            {"$f_opt_mtxt_02", "ROLE BONUS"},
            {"$f_opt_mtxt_03", "LV."},
            {"$f_opt_mtxt_04", "LEADER"},
            {"$f_opt_mtxt_05", "MEMBER"},

            {"$result_easy", "EASY MODE"},
            {"$result_title00", "BATTLE RESULTS"},
            {"$result_title01", "STATS"},
            {"$result_title02", "SCORE"},
            {"$result_title04", "RATING"},
            {"$result_title05", "CRYSTOGEN POINTS"},
            {"$result_title06", "TECHNICAL POINTS"},
            {"$result_title07", "SPOILS"},

            {"$role_00", "COMMANDO"},
            {"$role_01", "RAVAGER"},
            {"$role_02", "SENTINEL"},
            {"$role_03", "SABOTEUR"},
            {"$role_04", "SYNERGIST"},
            {"$role_05", "MEDIC"},
            {"$role_06", "COM"},
            {"$role_07", "RAV"},
            {"$role_08", "SEN"},
            {"$role_09", "SAB"},
            {"$role_10", "SYN"},
            {"$role_11", "MED"},

            {"$f_sho_def_03", "LOADING..."},

            {"$f_sho_mtxt_00", "ITEM"},
            {"$f_sho_mtxt_01", "PRICE"},
            {"$f_sho_mtxt_02", "OWNED"},
            {"$f_sho_mtxt_03", "TOTAL"},

            {"$f_sta_00", "HP"},
            {"$f_sta_01", "ATB LEVEL"},

            {"$nowloading", "NOW LOADING..."},
            {"$atb_01", "ATB LEVEL UP!"},

            //z002                                                      
            {"$f_cry_tut_00", "CRYSTARIUM"},
            {"$m_z002fl_051a", "EXAMINE"},
            {"$m_z002fl_131a", "ENTER"},
            {"$m_z002fl_132a", "ENTER"},
            {"$t_hgcr01_p01_01", "PARADIGM SYSTEM"},
            {"$t_hgcr02_p01_01", "BATTLE TECHNIQUE"},
            {"$t_hgcr04_p01_01", "EIDOLON BATTLE"},
            {"$t_hgcr05_p01_01", "COMMAND EXECUTION"},
            {"$t_hgcr06_p01_01", "PARADIGM SYSTEM"},
            {"$t_hgcr_cap_l2", "CRYSTARIUM EXPANDED!"},

            //z003                                                      
            {"$m_z003_panel", "ACTIVATE"},
            {"$m_z003_panel2", "ACTIVATE"},
            {"$m_z003_panel3", "ACTIVATE"},
            {"$m_z003_talk", "TALK"},

            //z004                                                      
            {"$m_z004fl_l_pn", "ACTIVATE"},
            {"$m_z004fl_p_pn", "ACTIVATE"},
            {"$m_z004fl_talk", "TALK"},
            {"$t_vpek01_p00_01", "GESTALT MODE"},

            //z006                                                      
            {"$m_z006fl_START", "READY, SET, GO!"},
            {"$m_z006fl_check", "BOARD"},
            {"$m_z006fl_seek", "EXAMINE"},

            //z008                                                      
            {"$m_z008fl_esc", "EXAMINE"},
            {"$m_z008fl_hchk1", "EXAMINE"},
            {"$m_z008fl_hchk2", "EXAMINE"},
            {"$m_z008fl_hchk3", "EXAMINE"},
            {"$m_z008fl_hchk4", "EXAMINE"},
            {"$m_z008fl_sdn", "ACTIVATE"},
            {"$m_z008fl_tv", "ACTIVATE"},

            //z010                                                      
            {"$m_z010fl_check", "TOUCH"},

            //z015                                                      
            {"$m_z015fl_blift", "ACTIVATE"},
            {"$m_z015fl_nlift", "ACTIVATE"},

            //z016                                                      
            {"$m_z016bt_t00_01", "ACTIVE TIME BATTLE"},
            {"$m_z016bt_t01_01", "ATTACK CHAIN"},
            {"$m_z016bt_t02_01", "ITEM USAGE"},
            {"$m_z016fl_rt000", "ACTIVATE"},
            {"$m_z016fl_rt001", "EXAMINE"},
            {"$m_z016fl_t7_00", "CAMERA CONTROL"},

            //z017                                                      
            {"$m_z017ts_check", "ACTIVATE"},

            //z018                                                      
            {"$m_z018fl_gate", "OPEN"},
            {"$m_z018fl_lift", "ACTIVATE"},
            {"$m_z018fl_talk", "TALK"},

            //z019                                                      
            {"$m_z019fl_p_c", "OPEN"},
            {"$m_z019fl_p_r", "ACTIVATE"},
            {"$m_z019fl_p_w", "WARP"},

            //z021                                                      
            {"$m_z021_to_lasd", "WARP"},

            //z022                                                      
            {"$m_z022fl_yu001", "BOARD"},
            {"$m_z022fl_yu005", "EXAMINE"},

            //z023                                                      
            {"$m_z023fl_a_che", "EXAMINE"},
            {"$m_z023fl_a_cho", "MOUNT"},
            {"$m_z023fl_a_tre", "DIG"},

            //z024                                                      
            {"$m_z024ac_yrkg", "BOARD"},
            {"$m_z024kzr_txt", "EXAMINE"},

            //z026                                                      
            {"$m_z026fl_ac000", "BOARD"},
            {"$m_z026fl_tg000", "ACTIVATE"},
            {"$m_z026fl_tg001", "EXAMINE"},

            //z027                                                      
            {"$m_z027fl_bk_ev", "EXAMINE"},
            {"$m_z027fl_bk_t0", "EXAMINE"},
            {"$m_z027fl_bk_t2", "EXAMINE"},
            {"$m_z027fl_bk_t3", "EXAMINE"},
            {"$m_z027fl_bk_t4", "EXAMINE"},
            {"$m_z027fl_ev050", "EXAMINE"},
            {"$m_z027fl_tg00", "EXAMINE"},
            {"$m_z027fl_tg000", "ACTIVATE"},
            {"$m_z027fl_tg001", "BOARD"},
            {"$m_z027fl_tg01", "EXAMINE"},
            {"$m_z027fl_tg02", "EXAMINE"},
            {"$m_z027fl_tg03", "EXAMINE"},
            {"$m_z027fl_tg04", "EXAMINE"},
            {"$m_z027fl_tg05", "EXAMINE"},
            {"$m_z027fl_tg06", "EXAMINE"},
            {"$m_z027fl_tg07", "EXAMINE"},
            {"$m_z027fl_tg08", "EXAMINE"},
            {"$m_z027fl_tg09", "EXAMINE"},
            {"$m_z027fl_tg10", "EXAMINE"},
            {"$m_z027fl_tg11", "EXAMINE"},
            {"$m_z027fl_tg12", "EXAMINE"},
            {"$m_z027fl_tg13", "EXAMINE"},
            {"$m_z027fl_tg14", "EXAMINE"},
            {"$m_z027fl_tg15", "EXAMINE"},
            {"$m_z027fl_tg16", "EXAMINE"},
            {"$m_z027fl_tg17", "EXAMINE"},
            {"$m_z027fl_tg18", "EXAMINE"},
            {"$m_z027fl_tg19", "EXAMINE"},
            {"$m_z027fl_tg20", "EXAMINE"},
            {"$m_z027fl_tg21", "EXAMINE"},
            {"$m_z027fl_tg22", "EXAMINE"},
            {"$m_z027fl_tg23", "EXAMINE"},
            {"$m_z027fl_tg24", "EXAMINE"},

            //z029
            {"$m_z029fl_fal", "EXAMINE"},
            {"$m_z029fl_gate", "WARP"},
            {"$m_z029fl_p_c", "ACTIVATE"},

            //z030                                                      
            {"$m_z030fl_wp001", "EXAMINE"}
        };
    }
}