using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPF_NCOAmanager
{
    /// <summary>
    /// Stores name-value pairs for EPF Product Code - Product IDs
    /// </summary>
    public class EPFProductCodeIDs
    {
        public string Code { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }

        public EPFProductCodeIDs(string nm, string cd, string id)
        {
            Name = nm;
            Code = cd;
            ID = id;
        }
    }
}
