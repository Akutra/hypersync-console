using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace hypersync
{
    public class syncconfig
    {
        public bool LogAction { get; set; }
        public int LogLevel { get; set; }
        public bool Migratory { get; set; }
        public bool KeepNewerDest { get; set; }
        public bool DamagedSource { get; set; }
        public bool nofolders { get; set; }
        public string LogFile { get; set; }

        public FolderPath[] SyncPaths;

        public syncconfig()
        {
            // default to an empty array
            this.SyncPaths = new FolderPath[0];
            // default to verbose
            this.LogLevel = 3;
            // default log file
            this.LogFile = "synclog.txt";
            this.nofolders = false;
        }
    }

    public class configLoader
    {
        public readonly int ERROR_BAD_ARGUMENTS = 0xA0;

        public syncconfig ConfigData { get; set; }

        private string _configfile = "";

        private DisplayManager ScreenPrinter;

        public configLoader(DisplayManager displayManager = null, string configPath = "syncconfig.xml")
        {
            this._configfile = configPath;

            if (object.ReferenceEquals(null, displayManager))
            {
                this.ScreenPrinter = new DisplayManager();
            }
            else
            {
                this.ScreenPrinter = displayManager;
            }

            this.reLoad();
        }

        public void reLoad()
        {
            // Initialize to empty
            this.ConfigData = new syncconfig();

            if (File.Exists(this._configfile))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(syncconfig));
                    FileStream fs = new FileStream(this._configfile, FileMode.Open);

                    syncconfig _syncconfig = (syncconfig)serializer.Deserialize(fs);
                    this.ConfigData = _syncconfig;
                }
                catch (Exception ex)
                {
                    // for now simply don't read config on error
                    this.ScreenPrinter.WriteError(string.Format("Unable to load the config file  with exception {0}",ex.Message));
                    System.Environment.Exit(ERROR_BAD_ARGUMENTS);
                }
            }
        }

        public bool WriteConfig()
        {
            if (File.Exists(this._configfile))
            {
                //File.Delete(this._configfile);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(syncconfig));
                TextWriter writer = new StreamWriter(this._configfile);

                serializer.Serialize(writer, this.ConfigData);
                writer.Close();
                return true;
            }
            catch (Exception ex)
            {
                // for now simply don't read config on error
                this.ScreenPrinter.WriteError(string.Format("Write config file failed with exception {0}",ex.Message));
                System.Environment.Exit(ERROR_BAD_ARGUMENTS);
                return false;
            }
        }
    }
}
