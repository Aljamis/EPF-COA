using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    public class JSONfiles : EPF_JSONbase
    {
        public string fileid { get; set; }
        public string status { get; set; }
        public string filepath { get; set; }
        public string fulfilled { get; set; }
    }
}
