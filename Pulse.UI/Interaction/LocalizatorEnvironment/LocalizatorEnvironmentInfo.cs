using System;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class LocalizatorEnvironmentInfo
    {
        public Version PatherVersion;
        public string[] PatherUrls;
        public string[] TranslationUrls;

        public bool IsFullScreen = true;
        public bool IsFullHd = true;
        public bool IsNihonVoice = true;
        public bool? SwitchButtons;
        public int AntiAliasing = 1;
        public int ShadowResolution = 1;

        public bool PlayMusic = true;
        public bool ExitAfterRunGame = true;

        public LocalizatorEnvironmentInfo(Version patherVersion, string[] patherUrls, string[] translationUrls)
        {
            PatherVersion = patherVersion;
            PatherUrls = patherUrls;
            TranslationUrls = translationUrls;
        }

        public void Validate()
        {
            Exceptions.CheckArgumentNull(PatherVersion, "PatherVersion");
            Exceptions.CheckArgumentNullOrEmprty(PatherUrls, "PatherUrls");
            Exceptions.CheckArgumentNullOrEmprty(TranslationUrls, "TranslationUrls");
        }

        public void UpdateUrls(LocalizatorEnvironmentInfo info)
        {
            PatherVersion = info.PatherVersion;
            PatherUrls = info.PatherUrls;
            TranslationUrls = info.TranslationUrls;
        }

        public void ToXml(XmlElement xmlElement)
        {
            xmlElement.SetString("PatherVersion", PatherVersion.ToString());
            xmlElement.SetBoolean("IsFullScreen", IsFullScreen);
            xmlElement.SetBoolean("IsFullHd", IsFullHd);
            xmlElement.SetBoolean("IsNihonVoice", IsNihonVoice);
            if (SwitchButtons != null)
                xmlElement.SetBoolean("SwitchButtons", SwitchButtons.Value);
            xmlElement.SetInt32("AntiAliasing", AntiAliasing);
            xmlElement.SetInt32("ShadowResolution", ShadowResolution);
            xmlElement.SetBoolean("PlayMusic", PlayMusic);
            xmlElement.SetBoolean("ExitAfterRunGame", ExitAfterRunGame);

            XmlElement urls = xmlElement.CreateChildElement("PatherUrls");
            foreach (string url in PatherUrls)
                urls.CreateChildElement("Url").SetString("Value", url);

            urls = xmlElement.CreateChildElement("TranslationUrls");
            foreach (string url in TranslationUrls)
                urls.CreateChildElement("Url").SetString("Value", url);
        }

        public static LocalizatorEnvironmentInfo FromXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
                return null;

            Version patherVersion = Version.Parse(xmlElement.GetChildAttribute("PatherVersion").Value);
            string[] patherUrls = null;
            string[] archiveUrls = null;

            XmlElement urls = xmlElement["PatherUrls"];
            if (urls != null)
            {
                patherUrls = new string[urls.ChildNodes.Count];
                for (int i = 0; i < patherUrls.Length; i++)
                    patherUrls[i] = urls.GetChildElement(i).GetString("Value");
            }

            urls = xmlElement["TranslationUrls"];
            if (urls != null)
            {
                archiveUrls = new string[urls.ChildNodes.Count];
                for (int i = 0; i < archiveUrls.Length; i++)
                    archiveUrls[i] = urls.GetChildElement(i).GetString("Value");
            }

            LocalizatorEnvironmentInfo result = new LocalizatorEnvironmentInfo(patherVersion, patherUrls, archiveUrls)
            {
                IsFullScreen = xmlElement.FindBoolean("IsFullScreen") ?? true,
                IsFullHd = xmlElement.FindBoolean("IsFullHd") ?? true,
                IsNihonVoice = xmlElement.FindBoolean("IsNihonVoice") ?? true,
                SwitchButtons = xmlElement.FindBoolean("SwitchButtons"),
                AntiAliasing = xmlElement.FindInt32("AntiAliasing") ?? 1,
                ShadowResolution = xmlElement.FindInt32("ShadowResolution") ?? 1,
                PlayMusic = xmlElement.FindBoolean("PlayMusic") ?? true,
                ExitAfterRunGame = xmlElement.FindBoolean("ExitAfterRunGame") ?? true
            };

            return result;
        }

        public bool IsIncompatible(Version version)
        {
            return version.Major < PatherVersion.Major;
        }
    }
}