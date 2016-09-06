using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    public class JSONlistOut : EPF_JSONbase
    {
        public string reccount { get; set; }
        public List<JSONfiles> filelist { get; set; }
    }
}
