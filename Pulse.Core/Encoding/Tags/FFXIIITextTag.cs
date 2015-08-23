using System;
using System.Globalization;
using System.Text;

namespace Pulse.Core
{
    public sealed class FFXIIITextTag
    {
        public const int MaxTagLength = 32;

        public readonly FFXIIITextTagCode Code;
        public readonly Enum Param;

        public FFXIIITextTag(FFXIIITextTagCode code, Enum param = null)
        {
            Code = code;
            Param = param;
        }

        public int Write(byte[] bytes, ref int offset)
        {
            bytes[offset++] = (byte)Code;
            if (Param == null)
                return 1;

            bytes[offset++] = (byte)(FFXIIITextTagParam)Param;
            return 2;
        }

        public int Write(char[] chars, ref int offset)
        {
            StringBuilder sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            if (EnumCache<FFXIIITextTagCode>.IsDefined(Code))
                sb.Append(Code);
            else
                sb.Append("Var").AppendFormat(((byte)Code).ToString("X2"));
            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');

            if (sb.Length > MaxTagLength)
                throw Exceptions.CreateException(Lang.Error.Text.TooLongTagNameFormat, sb.ToString());

            for (int i = 0; i < sb.Length; i++)
                chars[offset++] = sb[i];

            return sb.Length;
        }

        public static FFXIIITextTag TryRead(byte[] bytes, ref int offset, ref int left)
        {
            FFXIIITextTagCode code = (FFXIIITextTagCode)bytes[offset++];
            left -= 2;
            switch (code)
            {
                case FFXIIITextTagCode.End:
                case FFXIIITextTagCode.Escape:
                case FFXIIITextTagCode.Italic:
                case FFXIIITextTagCode.Many:
                case FFXIIITextTagCode.Article:
                case FFXIIITextTagCode.ArticleMany:
                    left++;
                    return new FFXIIITextTag(code);
                case FFXIIITextTagCode.Icon:
                    return new FFXIIITextTag(code, (FFXIIITextTagIcon)bytes[offset++]);
                case FFXIIITextTagCode.VarF4:
                case FFXIIITextTagCode.VarF6:
                case FFXIIITextTagCode.VarF7:
                    return new FFXIIITextTag(code, (FFXIIITextTagParam)bytes[offset++]);
                case FFXIIITextTagCode.Text:
                    return new FFXIIITextTag(code, (FFXIIITextTagText)bytes[offset++]);
                case FFXIIITextTagCode.Key:
                    return new FFXIIITextTag(code, (FFXIIITextTagKey)bytes[offset++]);
                case FFXIIITextTagCode.Color:
                    return new FFXIIITextTag(code, (FFXIIITextTagColor)bytes[offset++]);
                default:
                    int value = (int)code;
                    switch (value)
                    {
                        case 0x81:
                        case 0x85:
                            left += 2;
                            offset--;
                            return null;
                    }

                    if (value >= 0x80)
                        return new FFXIIITextTag(code, (FFXIIITextTagColor)bytes[offset++]);

                    left += 2;
                    offset--;
                    return null;
            }
        }

        public static FFXIIITextTag TryRead(char[] chars, ref int offset, ref int left)
        {
            int oldOffset = offset;
            int oldleft = left;

            string tag, par;
            if (chars[offset++] != '{' || !TryGetTag(chars, ref offset, ref left, out tag, out par))
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            FFXIIITextTagCode? code = EnumCache<FFXIIITextTagCode>.TryParse(tag);
            if (code == null)
            {
                byte varCode, numArg;
                if (tag.Length == 5 &&
                    byte.TryParse(tag.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out varCode) &&
                    byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                    return new FFXIIITextTag((FFXIIITextTagCode)varCode, (FFXIIITextTagParam)numArg);
            }

            if (code == null)
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            switch (code.Value)
            {
                case FFXIIITextTagCode.End:
                case FFXIIITextTagCode.Escape:
                case FFXIIITextTagCode.Italic:
                case FFXIIITextTagCode.Many:
                case FFXIIITextTagCode.Article:
                case FFXIIITextTagCode.ArticleMany:
                    return new FFXIIITextTag(code.Value);
                case FFXIIITextTagCode.VarF4:
                case FFXIIITextTagCode.VarF6:
                case FFXIIITextTagCode.VarF7:
                {
                    byte numArg;
                    if (byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        return new FFXIIITextTag(code.Value, (FFXIIITextTagParam)numArg);
                    break;
                }
                case FFXIIITextTagCode.Icon:
                {
                    byte numArg;
                    FFXIIITextTagIcon? arg = EnumCache<FFXIIITextTagIcon>.TryParse(par);
                    if (arg == null && byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        arg = (FFXIIITextTagIcon)numArg;
                    if (arg != null)
                        return new FFXIIITextTag(code.Value, arg.Value);
                    break;
                }
                case FFXIIITextTagCode.Text:
                {
                    byte numArg;
                    FFXIIITextTagText? arg = EnumCache<FFXIIITextTagText>.TryParse(par);
                    if (arg == null && byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        arg = (FFXIIITextTagText)numArg;
                    if (arg != null)
                        return new FFXIIITextTag(code.Value, arg.Value);
                    break;
                }
                case FFXIIITextTagCode.Key:
                {
                    byte numArg;
                    FFXIIITextTagKey? arg = EnumCache<FFXIIITextTagKey>.TryParse(par);
                    if (arg == null && byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        arg = (FFXIIITextTagKey)numArg;
                    if (arg != null)
                        return new FFXIIITextTag(code.Value, arg.Value);
                    break;
                }
                case FFXIIITextTagCode.Color:
                {
                    byte numArg;
                    FFXIIITextTagColor? arg = EnumCache<FFXIIITextTagColor>.TryParse(par);
                    if (arg == null && byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        arg = (FFXIIITextTagColor)numArg;
                    if (arg != null)
                        return new FFXIIITextTag(code.Value, arg.Value);
                    break;
                }
            }

            offset = oldOffset;
            left = oldleft;
            return null;
        }

        private static bool TryGetTag(char[] chars, ref int offset, ref int left, out string tag, out string par)
        {
            int lastIndex = Array.IndexOf(chars, '}', offset);
            int length = lastIndex - offset + 1;
            if (length < 2)
            {
                tag = null;
                par = null;
                return false;
            }

            left--;
            left -= length;

            int spaceIndex = Array.IndexOf(chars, ' ', offset + 1, length - 2);
            if (spaceIndex < 0)
            {
                tag = new string(chars, offset, length - 1);
                par = string.Empty;
            }
            else
            {
                tag = new string(chars, offset, spaceIndex - offset);
                par = new string(chars, spaceIndex + 1, lastIndex - spaceIndex - 1);
            }

            offset = lastIndex + 1;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            if (EnumCache<FFXIIITextTagCode>.IsDefined(Code))
                sb.Append(Code);
            else
                sb.Append("Var").AppendFormat(((byte)Code).ToString("X2"));

            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}