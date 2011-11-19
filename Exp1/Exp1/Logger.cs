using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public class Logger
    {
        public List<string> entries = new List<string>();
        public void Add(string entry, int level = 0)
        {
            string logentry = String.Empty;
            while (level>0)
            {
                logentry += "   ";
                level--;
            }
            logentry += entry;
            entries.Add(logentry);
        }
    }
}
