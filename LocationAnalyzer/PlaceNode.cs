using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class PlaceNode
    {
        public string Place;
        public List<string> StatesThatHaveThisPlace = new List<string>();

        public PlaceNode(string place)
        {
            Place = place;
        }
    }
}
