using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using LocationAnalyzer.Timer;
using System.Text.RegularExpressions;

namespace LocationAnalyzer.Parser
{
    class Program
    {
        public static string logfile = "C:\\projects\\practice" + DateTimeOffset.Now.Ticks.ToString() + ".txt";
        public static TimerBase timer = new TimerBase();

        static void Main(string[] args)
        {
            // Create a timestamped file for logging results
            Console.WriteLine("Creating timestamped file for logging results.");
            timer.Start();
            using (var fc = File.Create(logfile))
            {
                fc.Close();
            }
            Console.WriteLine("Time to create timestamped file: " + timer.DurationMs());

            try
            {
                Console.WriteLine("Seeding state objects with preliminary data.");
                timer.Start();
                List<State> states = SeedStates();
                Console.WriteLine("Time to seed state objects with preliminary data: " + timer.DurationMs());

                Console.WriteLine("Seeding state objects with location data.");
                timer.Start();
                AddLocationData(states);
                Console.WriteLine("Time to seed states with location data: " + timer.DurationS());

                Console.WriteLine("Logging state places.");
                timer.Start();
                LogStatePlaces(states);
                Console.WriteLine("Time to log state places: " + timer.DurationMs());

                Console.WriteLine("Execution complete!");
                Console.ReadKey();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
                return;
            }
        }

        public static List<State> SeedStates()
        {
            using (var client = new WebClient())
            {
                StatesContainer statesContainer = new StatesContainer();

                // Our starting URL used for seeding the rest of the app with the data and links we'll need later on.
                Stream stream = client.OpenRead("https://en.wikipedia.org/wiki/Lists_of_populated_places_in_the_United_States");
                List<State> states = new List<State>();

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
                                if (!states.Any(s => s.Links.Contains(strippedLink)) && !strippedLink.Contains("redlink"))
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
                                }
                            }                            
                        }
                    }
                }

                sr.Close();

                // Sorting the states in alphabetical order
                states = SortStates(states);

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

                                if (currentLine.Contains("td scope"))
                                {
                                    var match = Regex.Match(currentLine, "(\\b(title=\")\\b).+?(?=,)");
                                    var place = match.ToString().Substring(7);
                                    state.Places.Add(place);
                                }
                                else if (currentLine.Contains("title") && currentLine.Contains(state.Name) 
                                    && !currentLine.Contains("span") && !currentLine.Contains("ul") 
                                    && !currentLine.Contains("abbr") && !currentLine.Contains("List of")
                                    && !currentLine.Contains("census") && !currentLine.Contains("places")
                                    && !currentLine.Contains("Cities"))
                                {
                                    var pattern = Regex.Match(currentLine, ".*(?=\")");
                                    var chunk = pattern.ToString();
                                    var match = currentLine.Substring(chunk.Length);

                                    pattern = Regex.Match(match, ".*(?=\\<)");
                                    chunk = pattern.ToString();
                                    match = match.Substring(0, chunk.Length);

                                    pattern = Regex.Match(match, ".*(?=<)");
                                    chunk = pattern.ToString();
                                    match = match.Substring(0, chunk.Length);

                                    if (match.Length > 5)
                                    {
                                        var place = match.Substring(2);
                                        state.Places.Add(place);
                                    }
                                }
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

        public static void LogStatePlaces(List<State> list)
        {
            using (var sw = File.AppendText(logfile))
            {
                foreach (var state in list)
                {
                    sw.WriteLine(state.Name);
                    state.Places.ForEach(p => sw.WriteLine("\t" + p));
                }

                sw.Close();
            }
        }
    }
}
