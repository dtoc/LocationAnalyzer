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

                // Our starting URL used for seeding the rest of the app with the data and links we'll need later on.
                Stream stream = client.OpenRead("https://en.wikipedia.org/wiki/Lists_of_populated_places_in_the_United_States");

                // If we already have a file of results, delete it because it's outdated.
                File.Delete("C:\\projects\\practice.txt");

                // Recreate the file that we deleted so we can use it for logging results.
                using (var fc = File.Create("C:\\projects\\practice.txt"))
                {
                    fc.Close();
                }

                StreamReader sr = new StreamReader(stream);
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    State state = new State();
                    
                    // We only care about webpage lines that 1) reference an American state and 2) contain data on the list of places in that state
                    if (statesContainer.GetStateName(currentLine) != null && currentLine.Contains("List_of_places_in"))
                    {
                        // Since each line contains multiple bits of useful information, let's tokenize the line.
                        var tokens = currentLine.Split(' ');
                        foreach (var token in tokens)
                        {
                            var stateName = statesContainer.GetStateName(token);

                            // If the current token contains an href, we want to parse out the link.
                            if (token.Contains("href"))
                            {
                                // We want the substring that no longer has the href in it.
                                var strippedLink = token.Substring(5);
                                // We want to remove the first character, since it's a quotation mark.
                                strippedLink = strippedLink.Remove(0, 1);
                                // We want to remove the last character, since it's a quotation mark.
                                strippedLink = strippedLink.Remove(strippedLink.Length - 1, 1);
                                state.Link = strippedLink;
                                
                            }
                            else if (stateName != null)
                            {
                                state.Name = stateName;
                            }
                        }

                        Console.WriteLine(currentLine);

                        // Logging the results to the log file
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
