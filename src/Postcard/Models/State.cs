using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class State
{
    public List<string> Places = new List<string>();
    public List<string> Links = new List<string>();
    public string Name { get; set; }
    public State(string vName)
    {
        Name = vName;
    }
    public State()
    {

    }
}