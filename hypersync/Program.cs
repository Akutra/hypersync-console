using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace hypersync
{
    static class Program
    {
        static DisplayManager OutputManager = new DisplayManager();
        static configLoader ConfigManager = new configLoader(OutputManager);
        static bool SaveAndExit = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //process switch, set property value
            if (args.Length > 0)
            {
                int argpos = 1;
                foreach(string arg in args)
                {
                    if (arg.Length > 0) { ProcessArgument(arg, argpos); }
                    argpos++;
                }
                if(SaveAndExit)
                {
                    ConfigManager.WriteConfig();
                    OutputManager.AppendToConsole("Config Stored.", ConsoleColor.White);
                    System.Environment.Exit(0);
                }
            }
            if (ConfigManager.ConfigData.SyncPaths.Length > 0)
            {
                if (string.IsNullOrEmpty(ConfigManager.ConfigData.SyncPaths[0].dest_folder))
                {
                    OutputManager.WriteError("Destination path is required.");
                    System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
                }
                if (string.IsNullOrEmpty(ConfigManager.ConfigData.SyncPaths[0].src_folder))
                {
                    OutputManager.WriteError("Source path is required.");
                    System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
                }
            } else
            {
                OutputManager.WriteError("Both source and destination paths are required.");
                System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
            }
            if (ConfigManager.ConfigData.SyncPaths[0].dest_folder.Contains(ConfigManager.ConfigData.SyncPaths[0].src_folder))
            {
                OutputManager.WriteError("Destination cannot be inside source due to recursion.");
                System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
            }

            // Begin the File Sync.
            SyncCopy Synchronizer = new SyncCopy(ConfigManager, OutputManager);
            Synchronizer.SourcePath = ConfigManager.ConfigData.SyncPaths[0].src_folder;
            Synchronizer.DestPath = ConfigManager.ConfigData.SyncPaths[0].dest_folder;
            Synchronizer.CopyFolder();

            string final_notice = string.Format("> Synchronization Completed at {0}\n\r", DateTime.Now);
            OutputManager.Clear(3); OutputManager.Clear(2); OutputManager.Clear(1);
            OutputManager.hWriteToConsole(final_notice, 0, ConsoleColor.Green);
            Synchronizer.LogOutput(final_notice,1);

            OutputManager.AppendToConsole("               Total processed: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_items.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("                 Total updated: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_updated.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("       Total older than source: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_older.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("       Total newer than source: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_newer.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("Total missing from destination: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_missingdest.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("     Total missing from source: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_missingsrc.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("  Total invalid on destination: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_removed.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("       Total Zero Size Skipped: "); OutputManager.AppendLineToConsole(Synchronizer.result_data.total_zerosize.ToString().Trim(), ConsoleColor.White);
            OutputManager.AppendToConsole("");
        }

        static string[] SplitParm(string argument)
        {
            string[] rt = new[] { argument, "" };

            if (argument.Contains("="))
            {
                try
                {
                    rt[0] = argument.Split('=')[0];
                    rt[1] = argument.Substring(rt[0].Length + 1);
                } catch(Exception ex)
                {
                    OutputManager.WriteError(string.Format("Argument parsing error with exception: {0}", ex.Message));
                }
            }
            return rt;
        }

        static void ProcessArgument(string argument, int apos)
        {
            string[] parts = SplitParm(argument);
            
            string field = parts[0].Trim(new[] { '-', '/' }).ToLower();
            string parm = parts[1];
            int LogLevel;

            switch(field)
            {
                case "log":
                    ConfigManager.ConfigData.LogAction = true;
                    if (!string.IsNullOrEmpty(parm)) { ConfigManager.ConfigData.LogFile = parm; }
                    break;
                case "loglevel":
                    ConfigManager.ConfigData.LogAction = true;
                    if (int.TryParse(parm, out LogLevel)) { ConfigManager.ConfigData.LogLevel = LogLevel; }
                    break;
                case "keepnewerdest":
                    ConfigManager.ConfigData.KeepNewerDest = true;
                    break;
                case "damaged":
                    ConfigManager.ConfigData.DamagedSource = true;
                    break;
                case "src":
                case "s":
                    AddSrc(parm);
                    break;
                case "d":
                    AddTo(parm);
                    break;
                case "type":
                    AddType(parm);
                    break;
                case "v":
                case "version":
                    OutputManager.AppendLineToConsole(string.Format("Hyper-Synchronizer [{0}] version {1}-console", Constants.CompilationTimestampUtc.ToLocalTime().ToLongDateString(), Application.ProductVersion), ConsoleColor.White);
                    System.Environment.Exit(0);
                    break;
                case "h":
                case "help":
                    OutputManager.AppendLineToConsole("Command Line Syntax:\n\r");
                    OutputManager.AppendToConsole("\thypersync [s=]"); OutputManager.AppendToConsole("<source folder>",ConsoleColor.White);
                    OutputManager.AppendToConsole(" [d=]"); OutputManager.AppendToConsole("<destination folder>", ConsoleColor.White);
                    OutputManager.AppendToConsole(" [type="); OutputManager.AppendToConsole("<source folder type>", ConsoleColor.White); OutputManager.AppendToConsole("]");
                    OutputManager.AppendToConsole(" [log="); OutputManager.AppendToConsole("<optional file name>", ConsoleColor.White); OutputManager.AppendToConsole("]");
                    OutputManager.AppendToConsole(" [loglevel="); OutputManager.AppendToConsole("<level = 1, 2, or 3>", ConsoleColor.White); OutputManager.AppendToConsole("]");
                    OutputManager.AppendLineToConsole(" [KeepNewerDest] [damaged] [v or version] [h or help] [w]");
                    System.Environment.Exit(0);
                    break;
                case "w":
                    SaveAndExit = true;
                    break;
                default:
                    // No field name specified
                    if(apos==1) { AddSrc(argument); }
                    if(apos==2) { AddTo(argument); }
                    break;
            }
        }

        static void AddTo(string destination)
        {
            if( ConfigManager.ConfigData.SyncPaths.Length<1)
            {
                ConfigManager.ConfigData.SyncPaths = new[] { new FolderPath() };
            }
            ConfigManager.ConfigData.SyncPaths[0].dest_folder = destination;
        }

        static void AddSrc(string source)
        {
            if (ConfigManager.ConfigData.SyncPaths.Length < 1)
            {
                ConfigManager.ConfigData.SyncPaths = new[] { new FolderPath() };
            }
            ConfigManager.ConfigData.SyncPaths[0].src_folder = source;
        }

        static void AddType(string type)
        {
            List<string> types = new List<string>(Enum.GetNames(typeof(ptype)).Cast<string>().Select(x => x.ToString()));

            if (ConfigManager.ConfigData.SyncPaths.Length < 1)
            {
                ConfigManager.ConfigData.SyncPaths = new[] { new FolderPath() };
            }
            try
            {
                if (types.Contains(type))
                {
                    ConfigManager.ConfigData.SyncPaths[0].thisType = (ptype)Enum.Parse(typeof(ptype), type);
                } else
                {
                    string available = string.Join(", ", types.ToArray());
                    OutputManager.WriteError(string.Format("Invalid path type specified '{0}'. Available type(s): {1}.", type, available));
                    System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
                }
            }
            catch(Exception ex) {
                OutputManager.WriteError(string.Format("Parsing parameter Type failed with exception {0}", ex.Message));
                System.Environment.Exit(ConfigManager.ERROR_BAD_ARGUMENTS);
            }
        }
    }
}
