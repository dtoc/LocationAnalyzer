using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using Parser;

namespace LocationAnalyzer.Parser
{
    class Program
    {
        public static string logfile = "C:\\projects\\practice" + DateTimeOffset.Now.Ticks.ToString() + ".txt";
        public static Stopwatch stopwatch = new Stopwatch();
        public static bool RunAsParallel = false;

        static void Main(string[] args)
        {
            // Create a timestamped file for logging results
            Console.WriteLine("Creating timestamped file for logging results.");
            stopwatch.Start();
            using (var fc = File.Create(logfile))
            {
                fc.Close();
            }
            stopwatch.Stop();
            Console.WriteLine("Time to create timestamped file: " + stopwatch.Elapsed.Seconds);

            try
            {
                
                List<State> states = SeedStates();
                if (RunAsParallel)
                    AddLocationDataAsParallel(states);
                else
                    AddLocationData(states);

                LogStatePlaces(states);

                if (RunAsParallel)
                    CheckForDuplicatesInEachStateAsParallel(states);
                else
                    CheckForDuplicatesInEachState(states);

                //CheckForDuplicatesAcrossEachState(states);
                CheckForDuplicatesAcrossEachStateV2(states);
                Console.ReadKey();
    
                if (RunAsParallel)
                    CountOccurrencesOfEachPlaceAsParallel(states);
                else
                    CountOccurrencesOfEachPlace(states);

                Console.WriteLine("Time to finish: " + stopwatch.Elapsed.TotalSeconds);
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
            Console.WriteLine("Seeding state objects with preliminary data.");
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

                // Sorting the states in alphabetical order
                states = SortStates(states);
                Console.WriteLine("Time to seed state objects with preliminary data: " + stopwatch.Elapsed.Seconds);

                return states;
            }
        }

        public static void AddLocationData(List<State> states)
        {
            Console.WriteLine("Seeding state objects with location data.");
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
                                    if (!state.Places.Any(p => p.Equals(place)))
                                    {
                                        state.Places.Add(place);
                                    }
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
                                        if (!state.Places.Any(p => p.Equals(place)))
                                        {
                                            state.Places.Add(place);
                                        }
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

            Console.WriteLine("Time to seed states with location data: " + stopwatch.Elapsed.Seconds);
        }

        public static void AddLocationDataAsParallel(List<State> states)
        {
            Console.WriteLine("Seeding state objects with location data.");
            string rootUrl = "https://en.wikipedia.org";
            Parallel.ForEach(states, state =>
            {
                List<string> placesToAdd = new List<string>();

                Parallel.ForEach(state.Links, link =>
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
                                    if (!state.Places.Contains(place) && !placesToAdd.Contains(place))
                                    {
                                        placesToAdd.Add(place);
                                    }
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
                                        if (!state.Places.Contains(place) && !placesToAdd.Contains(place))
                                        {
                                            placesToAdd.Add(place);
                                        }
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
                });

                state.Places.AddRange(placesToAdd);
            });
            Console.WriteLine("Time to seed states with location data: " + stopwatch.Elapsed.Seconds);
        }

        // Method for sorting our states. Added it as a method in case we later want to sort 
        // using a custom comparator 
        public static List<State> SortStates(List<State> list)
        {
            return list.OrderBy(s => s.Name).ToList();
        }

        public static void LogStatePlaces(List<State> list)
        {
            Console.WriteLine("Logging state places.");

            using (var sw = File.AppendText(logfile))
            {
                foreach (var state in list)
                {
                    sw.WriteLine(state.Name);
                    state.Places.ForEach(p => sw.WriteLine("\t" + p));
                }
            }

            Console.WriteLine("Time to log state places: " + stopwatch.Elapsed.Seconds);
        }

        public static void CheckForDuplicatesInEachState(List<State> list)
        {
            Console.WriteLine("Checking for potential duplicates within each state.");
            foreach (var state in list)
            {
                foreach (var place in state.Places)
                {
                    var count = state.Places.Where(p => p.Equals(place)).Count();
                    if (count > 1)
                    {
                        Console.WriteLine("Duplicate of: " + place + " found in " + state.Name);
                    }
                }
            }
            Console.WriteLine("Time to check for potential duplicates within each state: " + stopwatch.Elapsed.Seconds);
        }

        public static void CheckForDuplicatesInEachStateAsParallel(List<State> list)
        {
            Console.WriteLine("Checking for potential duplicates within each state.");
            foreach (var state in list)
            {
                Parallel.ForEach(state.Places, place =>
                {
                    var count = state.Places.Where(l => place.Equals(place)).Count();
                    if (count > 1)
                    {
                        Console.WriteLine("Duplicate of: " + place + " found in " + state.Name);
                    }
                });
            }
            Console.WriteLine("Time to check for potential duplicates within each state: " + stopwatch.Elapsed.Seconds);
        }

        public static void CheckForDuplicatesAcrossEachState(List<State> list)
        {
            Console.WriteLine("Checking for potential duplicates across each state.");
            foreach (var state in list)
            {
                var places = state.Places;

                foreach (var anotherState in list)
                {
                    foreach (var place in anotherState.Places)
                    {
                        var count = places.Where(p => p.Contains(place)).Count();
                        if (count > 1 && !state.Name.Equals(anotherState.Name))
                        {
                            Console.WriteLine("Duplicate of: " + place + " found in " + state.Name + " and " + anotherState.Name);
                        }
                    }
                }
            }
            Console.WriteLine("Time to check for potential duplicates across each state: " + stopwatch.Elapsed.Seconds);
        }

        public static void CheckForDuplicatesAcrossEachStateV2(List<State> states)
        {
            // Generate a list of PlaceNodes
            List<PlaceNode> placeNodes = new List<PlaceNode>();
            states.ForEach(s => s.Places.ForEach(p => placeNodes.Add(new PlaceNode(p))));
            
            foreach (var place in placeNodes)
            {
                var currentPlace = place.Place;
                var statesThatHaveThisPlace = states.Where(s => s.Places.Contains(currentPlace)).ToList();
                foreach (var stateThatHasThisPlace in statesThatHaveThisPlace)
                {
                    if (!place.StatesThatHaveThisPlace.Contains(stateThatHasThisPlace.Name))
                    {
                        place.StatesThatHaveThisPlace.Add(stateThatHasThisPlace.Name);
                    }
                }
            }

            foreach (var state in states)
            {
                foreach (var place in state.Places)
                {
                    var targetNode = placeNodes.Where(pn => pn.StatesThatHaveThisPlace.Equals(place)).FirstOrDefault();
                    
                }
            }

            var duplicatePlaces = placeNodes.Where(pn => pn.StatesThatHaveThisPlace.Count > 1);
            Console.WriteLine("Number of duplicate places: " + duplicatePlaces.Count());

            foreach (var duplicatePlace in duplicatePlaces)
            {
                Console.WriteLine("Found duplicates of: " + duplicatePlace.Place + " in: ");
                Console.WriteLine();
                foreach (var state in duplicatePlace.StatesThatHaveThisPlace)
                {
                    Console.WriteLine("\t" + state);
                }
            }
        }

        public static void CountOccurrencesOfEachPlace(List<State> list)
        {
            Console.WriteLine("Checking for number of occurrences of each place.");
            int numberOfDuplicates = 0;
        
            foreach (var state in list)
            {
                foreach (var place in state.Places)
                {
                    var currentPlace = place;
                    var placeCount = 1;
   
                    foreach (var anotherState in list)
                    {
                        if (!anotherState.Name.Equals(state.Name))
                        {
                            var newCount = anotherState.Places.Where(p => p.Equals(currentPlace)).Count();
                            placeCount += newCount;
                            numberOfDuplicates += newCount;
                        }
                    }

                    Console.WriteLine("Number of times we've seen " + currentPlace + ": " + placeCount);
                }
            }

            Console.WriteLine("Number of duplicates found: " + numberOfDuplicates);
            Console.WriteLine("Time to check for number of occurrences of each place: " + stopwatch.Elapsed.Seconds);
        }

        public static void CountOccurrencesOfEachPlaceAsParallel(List<State> list)
        {
            Console.WriteLine("Checking for number of occurrences of each place.");
            int numberOfDuplicates = 0;
            int numberOfNonUniquePlaces = 0;

            foreach (var state in list)
            {
                Parallel.ForEach(state.Places, place =>
                {
                    var currentPlace = place;
                    var placeCount = 1;

                    foreach (var anotherState in list)
                    {
                        if (!anotherState.Name.Equals(state.Name))
                        {
                            var newCount = anotherState.Places.Where(p => p.Equals(currentPlace)).Count();
                            placeCount += newCount;
                            numberOfDuplicates += newCount;
                        }
                    }

                    Console.WriteLine("Number of times we've seen " + currentPlace + ": " + placeCount);
                    if (placeCount > 1)
                    {
                        numberOfNonUniquePlaces++;
                    }
                });
            }

            Console.WriteLine("Number of duplicates found: " + numberOfDuplicates);
            Console.WriteLine("Time to check for number of occurrences of each place: " + stopwatch.Elapsed.Seconds);
            Console.WriteLine("Number of non-unique places:" + numberOfNonUniquePlaces);
        }
    }
}
