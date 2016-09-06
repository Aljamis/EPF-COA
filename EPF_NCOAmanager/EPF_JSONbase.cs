using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    /// <summary>
    /// Storing the common fields in all JSON communication with EPF
    /// </summary>
    public class EPF_JSONbase
    {
        //  Found in both request & response
        public string logonkey { get; set; }
        public string tokenkey { get; set; }

        // Only found in the RESPONSE
        public string response { get; set; }
        public string messages { get; set; }
    }
}
