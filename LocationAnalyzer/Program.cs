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
                StatesContainer statesContainer = new StatesContainer();
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
                    if (statesContainer.GetState(currentLine) != null && currentLine.Contains("List_of_places_in"))
                    {
                        var tokens = currentLine.Split(' ');
                        foreach (var token in tokens)
                        {
                            var state = statesContainer.GetState(token);
                            if (token.Contains("href"))
                            {
                                var strippedLink = token.Substring(5);
                                strippedLink = strippedLink.Remove(0, 1);
                                strippedLink = strippedLink.Remove(strippedLink.Length - 1, 1);
                                Console.WriteLine("Link: " + strippedLink);
                            }
                            else if (state != null)
                            {
                                Console.WriteLine(state);
                            }
                        }

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
