using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace hypersync
{
    public class SyncCopy
    {
        public reportdata result_data = new reportdata();
        private DisplayManager ScreenPrinter;
        private configLoader configHandler { get; set; }

        string[] exclusionList = { "$RECYCLE.BIN", "System Volume Information", "WindowsImageBackup", "RECYCLER" };
        string[] exclusionFileList = { "container.dat", "Thumbs.db", "Desktop.ini", "~$" };
        private bool StopCopy = false, skipFile = false, AnalyzeOnly = false;
        public string DestPath = "";
        public string SourcePath = "";
        public char folder_delimiter = '\\';

        public SyncCopy(configLoader settings, DisplayManager displayManager = null)
        {
            this.configHandler = settings;
            if (object.ReferenceEquals(null, displayManager))
            {
                this.ScreenPrinter = new DisplayManager();
            } else
            {
                this.ScreenPrinter = displayManager;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                folder_delimiter = '/';
            }
        }

        private string SanitizeSource(string raw_src)
        {
            string tsource = raw_src;

            //Remove starting drive specific chars from source path and file
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (tsource.Contains(":")) { tsource = tsource.Substring(tsource.IndexOf(":") + 1); }
            }
            if (tsource[0] == folder_delimiter) { tsource = tsource.Substring(1); }
            return tsource;
        }

        private string SanitizeDestination(string raw_dest, string src_root)
        {
            string tdest = raw_dest;

            //Remove starting drive specific chars from source path and file
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (tdest.Contains(":")) { tdest = tdest.Substring(tdest.IndexOf(":") + 1); }
            }
            if (tdest[0] == folder_delimiter) { tdest = tdest.Substring(1); }

            //Remove root folder from path
            if (tdest.StartsWith(src_root)) { tdest = tdest.Substring(src_root.Length); }
            if (tdest[0] == folder_delimiter) { tdest = tdest.Substring(1); }

            return tdest;
        }

        private bool NoCopyAttributes(FileAttributes fAttrib)
        {
            return ((fAttrib & FileAttributes.Hidden) == FileAttributes.Hidden) || ((fAttrib & FileAttributes.System) == FileAttributes.System);
        }

        private bool NeedToCopyValidator(string cFile, string locOnDest)
        {
            FileInfo dest_finfo = new FileInfo(locOnDest);
            FileInfo src_finfo = new FileInfo(cFile);

            FileAttributes fattributes = File.GetAttributes(cFile);
            if (NoCopyAttributes(fattributes))
            {
                return false;
            }

            // Statistics
            if (File.GetLastWriteTime(cFile) > File.GetLastWriteTime(locOnDest))
            {
                result_data.total_older++;
            } else
            {
                result_data.total_newer++;
            }
            if (dest_finfo.Length < 1) { result_data.total_invalid++; }

            // Validation
            if (File.GetLastWriteTime(cFile).Equals(File.GetLastWriteTime(locOnDest)) 
                && (dest_finfo.Length == src_finfo.Length)) { return false; }
            if (File.GetLastWriteTime(cFile) < File.GetLastWriteTime(locOnDest) 
                && this.configHandler.ConfigData.KeepNewerDest) { return false; }
            //if (dest_finfo.Length != src_finfo.Length) { return true; }
            return true;
        }

        public void CopyFolder(string SourceFolder = "")
        {
            string tsrc = "", tdst= "", locOnDest, tsource;
            string[] SourceFiles;
            DateTime lastWriteTime;
            bool copyCurFile;

            if(SourceFolder == string.Empty) { SourceFolder = SourcePath; }

            try
            {
                SourceFiles = Directory.GetFiles(SourceFolder);
                ScreenPrinter.hWriteToConsole("Scanning...",0);
                foreach (string cFile in SourceFiles)
                {
                    copyCurFile = true;
                    lastWriteTime = File.GetLastWriteTime(cFile);
                    tsrc = cFile;
                    if (cFile.Length > 0 && !StopCopy && !exclusionCheck(cFile, true))
                    {
                        result_data.total_items++;
                        //Get destination path
                        locOnDest = DestPath;
                        tsource = SanitizeSource(SourcePath); 

                        //append destination
                        if (locOnDest[locOnDest .Length - 1] != folder_delimiter) { locOnDest += folder_delimiter; }
                        tdst = SanitizeDestination(cFile, tsource);
                        if( object.ReferenceEquals(null, tdst))
                        {
                            ScreenPrinter.WriteError(string.Format("Destination path issue: {0}", tdst));
                            System.Environment.Exit(0);
                        }
                        locOnDest += tdst;

                        if(cFile.Length>120)
                        {
                            Console.Write("");
                        }
                        //update display
                        ScreenPrinter.hWriteToConsole(cFile,1, ConsoleColor.White);

                        //check for destination
                        copyCurFile = true;
                        if (File.Exists(locOnDest))
                        {
                            copyCurFile = NeedToCopyValidator(cFile, locOnDest);
                        }
                        else
                        {
                            result_data.total_missingdest++;
                        }

                        if (copyCurFile)
                        {
                            if (!AnalyzeOnly)
                            {
                                LogOutput("Starting Copy " + cFile + " to " + locOnDest + " @ " + DateTime.Now);
                                SafeCopy(cFile, locOnDest);
                            }
                            else if (AnalyzeOnly && this.configHandler.ConfigData.LogAction)
                            {
                                LogOutput("Synchronization would update:", 1);
                                LogFileData(cFile, locOnDest);
                            }
                        }

                    }

                    if (StopCopy) { break; }
                }
            }
            catch (Exception _e)
            {
                if (tsrc.Length > 0)
                {
                    ScreenPrinter.WriteError(string.Format("Path/File issue with exception {0}",_e.Message));
                }
            }

            // Copy the subfolders
            if (!StopCopy)
            {
                ScanFolders(SourceFolder);
            }
        }

        private void ScanFolders(string SourceFolder)
        {
            string[] SourceFolders;
            string tsrc = "";
            try
            {
                SourceFolders = Directory.GetDirectories(SourceFolder);
                foreach (string cFolder in SourceFolders)
                {
                    if (!exclusionCheck(cFolder))
                    {

                        tsrc = cFolder;
                        if (cFolder.Length > 0)
                        {
                            if( !NoCopyAttributes(File.GetAttributes(cFolder))) { 
                                CopyFolder(cFolder); }
                        }
                    }
                }
            }
            catch (Exception _e)
            {
                if (tsrc.Length > 0)
                {
                    ScreenPrinter.WriteError(_e.Message);
                    LogOutput("Error : " + _e.Message + " @ " + DateTime.Now, 1);
                }
            }
        }

        public void LogOutput(string outstr, int loglevel = 3)
        {
            if (loglevel <= this.configHandler.ConfigData.LogLevel) { return; }
            if (this.configHandler.ConfigData.LogAction)
            {
                StreamWriter outfle;

                if (File.Exists(this.configHandler.ConfigData.LogFile))
                {
                    outfle = File.AppendText(this.configHandler.ConfigData.LogFile);
                }
                else
                {
                    outfle = File.CreateText(this.configHandler.ConfigData.LogFile);
                }
                outfle.WriteLine(outstr);

                outfle.Close();
            }
        }

        void LogFileData(string src_file, string dest_file)
        {
            FileAttributes fattributes = File.GetAttributes(src_file);
            FileInfo dest_finfo = null, src_finfo = null;
            string tlog = "";

            if (File.Exists(src_file))
            {
                src_finfo = new FileInfo(src_file);
                tlog = "Source File: " + src_file + " " + src_finfo.Length + " " + File.GetLastWriteTime(src_file);
            }
            else
            {
                tlog = "Source File: " + src_file + " not located.";
            }

            tlog += " ";

            if (File.Exists(dest_file))
            {
                dest_finfo = new FileInfo(dest_file);
                tlog += "Destination File: " + dest_file + " " + dest_finfo.Length + " " + File.GetLastWriteTime(dest_file) + " ";
            }
            else
            {
                tlog += "Destination File: " + dest_file + " not located.";
            }

            tlog += "@ " + DateTime.Now;
            LogOutput(tlog, 1);
        }

        bool SafeCopy(string src_file, string dest_file)
        {
            bool rt = false;
            FileInfo fInfo;

            do
            {
                try
                {
                    //Skip some work files, no need to replicate office work files.
                    fInfo = new FileInfo(src_file);
                    if( fInfo.Length<1 )
                    {
                        LogOutput("processing Zero Size file skipped : " + fInfo.Name, 2);
                        ScreenPrinter.hWriteToConsole("Skipping zero size file...", 0);
                        result_data.total_zerosize++;
                        rt = false;
                        break;
                    }
                    if (fInfo.Name.StartsWith("~$"))
                    {
                        LogOutput("processing WorkFile skipped : " + fInfo.Name, 2);
                        ScreenPrinter.hWriteToConsole("Skipping work file..." + fInfo.Name,0);
                        rt = false;
                        break;
                    }

                    ScreenPrinter.hWriteToConsole(dest_file,2, ConsoleColor.Yellow);
                    ScreenPrinter.hWriteToConsole("in progress...",0);

                    //Make sure directory exists
                    fInfo = new FileInfo(dest_file);
                    if (!fInfo.Directory.Exists) { fInfo.Directory.Create(); }

                    //File.Delete(dest_file);
                    File.Copy(src_file, dest_file, true);
                    result_data.total_updated++;

                    ScreenPrinter.WriteToConsole("success...", 0, ConsoleColor.Green);
                    //ScreenPrinter.Clear(2);
                    LogOutput("Successfully Copied File : " + src_file + " to " + dest_file + " @ " + DateTime.Now, 2);
                    rt = true;
                    break;
                }
                catch (IOException _e)
                {
                    ScreenPrinter.WriteError(_e.Message);
                    LogOutput(_e.ToString() + " : " + _e.Message + " @ " + DateTime.Now, 1);
                    if (!this.configHandler.ConfigData.DamagedSource)
                    {
                        rt = false;
                        break;
                    }
                    if (this.configHandler.ConfigData.DamagedSource || StopCopy || skipFile) { ScreenPrinter.hWriteToConsole("in progress...Skipping file...",0); break; }
                }
                catch (UnauthorizedAccessException _e)
                {
                    //Read or write error, skip file.
                    ScreenPrinter.WriteError(_e.Message);
                    ScreenPrinter.hWriteToConsole("in progress...Skipping file...",0);
                    LogOutput(_e.ToString() + " : " + _e.Message + " @ " + DateTime.Now, 1);
                    break;
                }
                catch (Exception _e)
                {
                    ScreenPrinter.WriteError(_e.Message);
                    ScreenPrinter.hWriteToConsole("in progress...Skipping file...",0);
                    LogOutput(_e.ToString() + " : " + _e.Message + " @ " + DateTime.Now, 1);
                    //Thread.Sleep(5000);
                    break;
                }
                if (skipFile) { break; }
            } while (true);
            skipFile = false;
            return rt;
        }

        bool exclusionCheck(string file)
        {
            bool rt = false;

            for (int x=0; x < exclusionList.Length; x++) // Fastest loop mechanizm for this repetative task
            {
                if (file.Contains(exclusionList[x]))
                {
                    rt = true;
                    break;
                }
            }

            return rt;
        }

        bool exclusionCheck(string file, bool file_chk)
        {
            bool rt = false;

            for (int x = 0; x < exclusionFileList.Length; x++) // Fastest loop mechanizm for this repetative task
            {
                if (file.Contains(exclusionFileList[x]))
                {
                    rt = true;
                    break;
                }
            }

            return rt;
        }

    }
}
