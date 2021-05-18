using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hypersync
{
    public static partial class Constants
    {
        public static string[] exclusionList = { "$RECYCLE.BIN", "System Volume Information", "WindowsImageBackup", "RECYCLER" };
        public static string[] exclusionFileList = { "container.dat", "Thumbs.db", "Desktop.ini", "~$" };
    }
}
