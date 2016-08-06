using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LocationAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new WebClient())
            {
                States states = new States();
                Stream stream = client.OpenRead("https://en.wikipedia.org/wiki/Lists_of_populated_places_in_the_United_States");
                StreamReader sr = new StreamReader(stream);
                int count = 0;
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    if (states.isAState(currentLine))
                    {
                        Console.WriteLine(currentLine);
                        count++;
                    }
                }
                Console.WriteLine("count: " + count);
                Console.ReadKey();
            }
        }
    }
}
