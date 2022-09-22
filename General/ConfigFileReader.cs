using System;
using System.IO;
using System.Xml;

namespace RPS
{
    internal class ConfigFileReader : IDisposable
    {
        private XmlReader Reader_;
        public ConfigFile ConfigContent { get; set; } = new ConfigFile();

        public ConfigFileReader(Stream stream)
        {
            Reader_ = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = true });

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "RPS_ConfigFile":
                                ReadConfig();
                                break;
                            default:
                                SkipElement();
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "RPS_ConfigFile") throw new FormatException(Reader_.Name);
                        return;
                }
            }
        }

        public void Dispose()
        {
            Reader_.Close();
        }

        private void ReadConfig()
        {
            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Api-Key":
                                ReadApiKey();
                                break;
                            case "Directory":
                                ReadDir();
                                break;
                            case "StartPoint":
                                ReadStart();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "RPS_ConfigFile") throw new FormatException(Reader_.Name);
                        return;
                }
            }
        }

        private void ReadApiKey()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Landscape":
                        ConfigContent.ApiKey_Landscape = Reader_.Value;
                        break;
                    case "Mail-Key":
                        ConfigContent.ApiKey_MailKey = Reader_.Value;
                        break;
                    case "Mail-User":
                        ConfigContent.ApiKey_MailUser = Reader_.Value;
                        break;
                }
            }
        }
        private void ReadDir()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "RoutDir":
                        ConfigContent.RouterDir = Reader_.Value;
                        break;
                    case "SRTMDir":
                        ConfigContent.SRTMDir = Reader_.Value;
                        break;
                    case "TracksDir":
                        ConfigContent.TracksDir = Reader_.Value;
                        break;
                }
            }
        }
        private void ReadStart()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Lon":
                        ConfigContent.StartPoint_Lon = Reader_.Value;
                        break;
                    case "Lat":
                        ConfigContent.StartPoint_Lat = Reader_.Value;
                        break;
                    case "Zoom":
                        ConfigContent.StartPoint_Zoom = Reader_.Value;
                        break;
                    case "RouterDB":
                        ConfigContent.StarRouter_DB = Reader_.Value;
                        break;
                }
            }
        }

        private void SkipElement()
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;
            int depth = Reader_.Depth;

            while (Reader_.Read())
            {
                if (Reader_.NodeType == XmlNodeType.EndElement)
                {
                    if (Reader_.Depth == depth && Reader_.Name == elementName) return;
                }
            }

            throw new FormatException(elementName);
        }
    }

    public class ConfigFile
    {
        public string ApiKey_Landscape { get; set; }
        public string ApiKey_MailKey { get; set; }
        public string ApiKey_MailUser { get; set; }
        public string RouterDir { get; set; }
        public string SRTMDir { get; set; }
        public string TracksDir { get; set; }
        public string StartPoint_Lon { get; set; }
        public string StartPoint_Lat { get; set; }
        public string StartPoint_Zoom { get; set; }
        public string StarRouter_DB { get; set; }
    }
}
