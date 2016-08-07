using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationAnalyzer
{
    class State
    {
        public List<string> Places = new List<String>();
        public string Name { get; set; }
        public string Link { get; set; }
        public State (string vName, string vLink)
        {
            Name = vName;
            Link = vLink;
        }
        public State()
        {

        }
    }
}
