using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    /// <summary>
    /// This JSON only stores login & pword
    /// </summary>
    public class JSONlogin : EPF_JSONbase
    {
        public string login { get; set; }
        public string pword { get; set; }
    }
}
