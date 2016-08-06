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

                File.Delete("C:\\projects\\practice.txt");
                using (var fc = File.Create("C:\\projects\\practice.txt"))
                {
                    fc.Close();
                }

                StreamReader sr = new StreamReader(stream);
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    if (states.isAState(currentLine) && currentLine.Contains("List_of_places_in"))
                    {
                        Console.WriteLine(currentLine);
                        using (var sw = File.AppendText("C:\\projects\\practice.txt"))
                        {
                            sw.WriteLine(currentLine);
                            sw.Dispose();
                        }
                    }
                }
                Console.ReadKey();
            }
        }
    }
}
