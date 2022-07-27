using System;
using System.Configuration;
using System.IO;

namespace LightenceClient.Services
{
    class ConfigurationFileManager
    {
        static void IntializeConfigurationFile()
        {
            // Create a set of unique key/value pairs to store in
            // the appSettings section of an auxiliary configuration
            // file.
            bool mute = true;
            string[] buffer = {"<appSettings>",
            "<add key='MuteMicroWhileJoining' value='" + mute + "'/>",
            "<add key='CameraOffWhileJoining' value='true'/>",
            "</appSettings>"};

            // Create an auxiliary configuration file and store the
            // appSettings defined before.
            // Note creating a file at run-time is just for demo 
            // purposes to run this example.
            File.WriteAllLines("appSettings.config", buffer);

            // Get the current configuration associated
            // with the application.
            System.Configuration.Configuration config =
               ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Associate the auxiliary with the default
            // configuration file. 
            System.Configuration.AppSettingsSection appSettings = config.AppSettings;
            appSettings.File = "appSettings.config";

            // Save the configuration file.
            config.Save(ConfigurationSaveMode.Modified);

            // Force a reload in memory of the 
            // changed section.
            ConfigurationManager.RefreshSection("appSettings");
        }

        // This function shows how to write a key/value
        // pair to the appSettings section.
        static void WriteAppSettings()
        {
            try
            {
                // Get the application configuration file.
                System.Configuration.Configuration config =
                   ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Create a unique key/value pair to add to 
                // the appSettings section.
                string keyName = "AppStg" + config.AppSettings.Settings.Count;
                string value = string.Concat(DateTime.Now.ToLongDateString(),
                               " ", DateTime.Now.ToLongTimeString());

                // Add the key/value pair to the appSettings 
                // section.
                // config.AppSettings.Settings.Add(keyName, value);
                System.Configuration.AppSettingsSection appSettings = config.AppSettings;
                appSettings.Settings.Add(keyName, value);

                // Save the configuration file.
                config.Save(ConfigurationSaveMode.Modified);

                // Force a reload in memory of the changed section.
                // This to read the section with the
                // updated values.
                ConfigurationManager.RefreshSection("appSettings");

                Console.WriteLine(
                    "Added the following Key: {0} Value: {1} .", keyName, value);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception raised: {0}",
                    e.Message);
            }
        }

        // This function shows how to read the key/value
        // pairs (settings collection)contained in the 
        // appSettings section.
        static void ReadAppSettings()
        {
            try
            {

                // Get the configuration file.
                System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Get the appSettings section.
                System.Configuration.AppSettingsSection appSettings =
                    (System.Configuration.AppSettingsSection)config.GetSection("appSettings");

                // Get the auxiliary file name.
                Console.WriteLine("Auxiliary file: {0}", config.AppSettings.File);

                // Get the settings collection (key/value pairs).
                if (appSettings.Settings.Count != 0)
                {
                    foreach (string key in appSettings.Settings.AllKeys)
                    {
                        string value = appSettings.Settings[key].Value;
                        Console.WriteLine("Key: {0} Value: {1}", key, value);
                    }
                }
                else
                {
                    Console.WriteLine("The appSettings section is empty. Write first.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception raised: {0}",
                    e.Message);
            }
        }
    }
}
