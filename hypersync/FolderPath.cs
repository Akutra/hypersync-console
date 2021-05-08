using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hypersync
{
    public enum ptype { normal = 0 };
    public class FolderPath
    {
        public string src_folder;
        public string dest_folder;
        public ptype thisType;
    }

    public class reportdata
    {
        public long total_items = 0;
        public long total_updated = 0;
        public long total_older = 0;
        public long total_newer = 0;
        public long total_missingdest = 0;
        public long total_missingsrc = 0;
        public long total_invalid = 0;
        public long total_removed = 0;
        public long total_zerosize = 0;

        public void reset()
        {
            total_items = 0; total_invalid = 0; total_missingdest = 0; total_missingsrc = 0; total_newer = 0; total_older = 0; total_updated = 0; total_removed = 0; total_zerosize = 0;
        }
    }
}

