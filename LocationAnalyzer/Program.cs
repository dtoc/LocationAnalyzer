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
            List<State> states = SeedStates();
            AddLocationData(states);
            foreach (var state in states)
            {
                foreach (var place in state.Places)
                {
                    Console.WriteLine(place);
                }
            }

            Console.ReadKey();
        }

        public static List<State> SeedStates()
        {
            using (var client = new WebClient())
            {
                StatesContainer statesContainer = new StatesContainer();

                // Our starting URL used for seeding the rest of the app with the data and links we'll need later on.
                Stream stream = client.OpenRead("https://en.wikipedia.org/wiki/Lists_of_populated_places_in_the_United_States");
                List<State> states = new List<State>();

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
                    var stateName = statesContainer.GetStateName(currentLine);

                    // Only proceed if we're dealing with a state that we care about
                    if (!String.IsNullOrEmpty(stateName))
                    {
                        // We only care about webpage lines that 1) reference an American state and 2) contain data on the list of places in that state
                        if ( currentLine.Contains("List_of_places_in")  
                            || currentLine.Contains("List_of_cities_in")
                            || currentLine.Contains("List_of_towns_in")
                            || currentLine.Contains("List_of_cities_and_towns_in")
                            || currentLine.Contains("List_of_municipalities_in")
                            || currentLine.Contains("List_of_populated_places_")
                            || currentLine.Contains("List_of_census-designated-places_in")
                            || currentLine.Contains("List_of_unincorporated"))
                        {
                            // Since each line contains multiple bits of useful information, let's tokenize the line.
                            var tokens = currentLine.Split(' ');
                            var linkToken = tokens.Where(t => t.Contains("href")).FirstOrDefault();

                            if (linkToken != null)
                            {
                                // We want the substring that no longer has the href in it.
                                var strippedLink = linkToken.Substring(5);
                                // We want to remove the first character, since it's a quotation mark.
                                strippedLink = strippedLink.Remove(0, 1);
                                // We want to remove the last character, since it's a quotation mark.
                                strippedLink = strippedLink.Remove(strippedLink.Length - 1, 1);

                                // Continue only if we didn't already handle this specific link
                                if (!states.Any(s => s.Links.Contains(strippedLink)))
                                {
                                    state.Links.Add(strippedLink);

                                    state.Name = stateName;
                                    // If there isn't already an entry for this state, add it to our list of states.
                                    if (!states.Where(s => s.Name == stateName).Any())
                                    {
                                        states.Add(state);
                                    }
                                    // Else since we already have an entry for this state, add just the link to that state's object
                                    else
                                    {
                                        if (state.Links.Count > 0)
                                        {
                                            states.Where(s => s.Name == stateName).First().Links.Add(state.Links.First());
                                        }
                                    }

                                    Console.WriteLine(currentLine);
                                }
                            }                            
                        }
                    }
                }

                sr.Close();
                Console.WriteLine("COUNT: " + states.Count);

                // Sorting the states in alphabetical order
                states = SortStates(states);

                using (var sw = File.AppendText("C:\\projects\\practice.txt"))
                {
                    foreach (var state in states)
                    {
                        sw.WriteLine(state.Name);
                        foreach (var link in state.Links)
                        {
                            sw.WriteLine(link);
                        }
                        sw.WriteLine();
                    }
                }

                return states;
            }
        }

        public static void AddLocationData(List<State> states)
        {
            string rootUrl = "https://en.wikipedia.org";
            foreach (var state in states)
            {
                foreach (var link in state.Links)
                {
                    string targetUrl = rootUrl + link;
                    targetUrl.Replace("\"", "");
                    using (var client = new WebClient())
                    {
                        try
                        {
                            Stream stream = client.OpenRead(targetUrl);
                            StreamReader sr = new StreamReader(stream);
                            while (!sr.EndOfStream)
                            {
                                var currentLine = sr.ReadLine();
                                state.Places.Add(currentLine);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Console.WriteLine("Invalid link");
                        }
                    }
                }
            }
        }

        // Method for sorting our states. Added it as a method in case we later want to sort 
        // using a custom comparator 
        public static List<State> SortStates(List<State> list)
        {
            return list.OrderBy(s => s.Name).ToList();
        }
    }
}
