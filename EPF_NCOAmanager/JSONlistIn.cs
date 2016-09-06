using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    public class JSONlistIn : EPF_JSONbase
    {
        public string productcode { get; set; }
        public string productid { get; set; }
        public string status { get; set; }
        /* Have to remove this because if it's null the JSON has { fulfilled:null }
         * resulting in no files returned. 
         */
        //public string fulfilled { get; set; }
    }
}
