﻿using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace LBCService
{
    internal class XMLConfigMethods
    {
        private static readonly string XMLPath = AppDomain.CurrentDomain.BaseDirectory + "LCBServiceConfig.xml";

        /// <summary>
        ///    Our config data structure/class we use to easily pass things along
        /// </summary>
        public class ConfigData
        {
            public string Keyboard_Core_Path { get; set; }
            public int Light_Level { get; set; }
            public int Timeout_Preference { get; set; }
        }

        /// <summary>
        ///    Create new Config XML with default values if not present 
        /// </summary>
        public static bool SaveConfigXML(string KBCorePath, int LightLevel, int TimeoutPreference)
        {
            try
            {
                var configXML = new XDocument(
                    new XElement("configuration",
                        new XElement("Keyboard_Core_Path", KBCorePath),
                        new XElement("Light_Level", LightLevel),
                        new XElement("Timeout_Preference", TimeoutPreference)
                    )
                );
                configXML.Save(XMLPath);
                return true;
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("LenovoBacklightControl", $"Error creating config XML: {e.Message}",EventLogEntryType.Error, 50915);
                return false;
            }

        }

        /// <summary>
        ///    Read Config XML data
        /// </summary>
        public static ConfigData ReadConfigXML()
        {
            //
            // set default config data values
            //
            var configData = new ConfigData
            {
                Keyboard_Core_Path =
                    @"C:\ProgramData\Lenovo\ImController\Plugins\ThinkKeyboardPlugin\x86\Keyboard_Core.dll",
                Light_Level = 2
            };

            //
            // If config file is not found, create one
            //
            if (!System.IO.File.Exists(XMLPath))
            {
                if (!SaveConfigXML(@"C:\ProgramData\Lenovo\ImController\Plugins\ThinkKeyboardPlugin\x86\Keyboard_Core.dll",2,300))
                {
                    // if creation fails, return default values
                    return configData;
                }
            }

            var xmlConfigDocument = new XmlDocument();
            var timeoutPreferenceFound = false;
            try
            {
                xmlConfigDocument.Load(XMLPath);
                foreach (XmlElement xmlElement in xmlConfigDocument.DocumentElement)
                {
                    switch (xmlElement.Name)
                    {
                        case "Keyboard_Core_Path":
                            configData.Keyboard_Core_Path = xmlElement.InnerText;
                            break;
                        case "Light_Level":
                            configData.Light_Level = int.Parse(xmlElement.InnerText);
                            break;
                        case "Timeout_Preference":
                            configData.Timeout_Preference = int.Parse(xmlElement.InnerText);
                            timeoutPreferenceFound = true;
                            break;
                    }
                }
                if (!timeoutPreferenceFound)
                {
                    SaveConfigXML(configData.Keyboard_Core_Path, configData.Light_Level, 300);
                }
                return configData;
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("LenovoBacklightControl", $"Error reading config XML: {e.Message}", EventLogEntryType.Error, 50915);
                return configData;
            }
        }
    }
}
