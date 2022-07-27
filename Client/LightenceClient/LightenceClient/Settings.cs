using System;
using System.Threading.Tasks;
using System.Xml;

namespace LightenceClient
{
    public static class Settings
    {
     
        #region settings variables
        // general settings
        public static bool MicroMutedStartMeeting = true;
        public static bool AutostartEnabled = false;
        public static bool AutoCopyID = true;
        public static string DownloadedFilesPath = "C:\\Users\\Public\\Downloads\\";
        #endregion

        public static void ResetToDefault()
        {
            MicroMutedStartMeeting = true;
            AutostartEnabled = false;
            AutoCopyID = true;
            // TODO this something
            DownloadedFilesPath = "C:\\Users\\Public\\Downloads\\";
        }

        public static async Task SaveSettings()
        {
            await Task.Run(() =>
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true
                };
                XmlWriter document = XmlWriter.Create(Constants.configFileName, settings);

                document.WriteStartDocument();
                    document.WriteStartElement("settings");
                        document.WriteStartElement("page");
                            document.WriteAttributeString("ID", "General");
                            document.WriteStartElement("microMutedStartMeeting");
                                document.WriteString(MicroMutedStartMeeting.ToString());
                            document.WriteEndElement();
                            document.WriteStartElement("autostartEnabled");
                                document.WriteString(AutostartEnabled.ToString());
                            document.WriteEndElement();
                            document.WriteStartElement("autoCopyID");
                                document.WriteString(AutoCopyID.ToString());
                            document.WriteEndElement();
                            document.WriteStartElement("downloadedFilesPath");
                                document.WriteString(DownloadedFilesPath.ToString());
                            document.WriteEndElement();
                        document.WriteEndElement();
                    document.WriteEndElement();
                document.WriteEndDocument();

                document.Close();
            });
        }

        public static async Task LoadSettings()
        {
            await Task.Run(() =>
            {
                XmlDocument document = new XmlDocument();
                document.Load(Constants.configFileName);

                XmlElement root = document.DocumentElement;

                foreach (XmlNode page in root.ChildNodes)
                {
                    if (page.NodeType == XmlNodeType.Element)
                    {
                        foreach (XmlNode setting in page.ChildNodes)
                        {
                            switch (setting.Name)
                            {
                                case "microMutedStartMeeting":
                                    {
                                        MicroMutedStartMeeting = Convert.ToBoolean(setting.FirstChild.Value);
                                    }
                                    break;
                                case "autostartEnabled":
                                    {
                                        AutostartEnabled = Convert.ToBoolean(setting.FirstChild.Value);
                                    }
                                    break;
                                case "autoCopyID":
                                    {
                                        AutoCopyID = Convert.ToBoolean(setting.FirstChild.Value);
                                    }
                                    break;
                                case "downloadedFilesPath":
                                    {
                                        DownloadedFilesPath = setting.FirstChild.Value;
                                    }
                                    break;
                            }
                        }
                    }
                }
            });
            
        }
    }
}
