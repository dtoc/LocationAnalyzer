using DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    public class ParserAgent
    {
        public bool RunAsParallel = true;
        public List<State> states;
        public List<PlaceNode> placeNodes;
        public IEnumerable<PlaceNode> duplicates;
        
        public ParserAgent()
        {

        }

        public void Parse()
        {
            try
            {
                states = SeedStates();

                if (RunAsParallel)
                    AddLocationDataAsParallel(states);
                else
                    AddLocationData(states);

                placeNodes = SeedPlaceNodes(states);

                if (RunAsParallel)
                    CheckForDuplicatesInEachStateAsParallel(states);
                else
                    CheckForDuplicatesInEachState(states);

                duplicates = CheckForDuplicatesAcrossEachState(states, placeNodes);

                CountOccurrencesOfEachPlace(states, placeNodes);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return;
            }
        }

        public List<State> SeedStates()
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
                        if (currentLine.Contains("List_of_places_in")
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
                return states;
            }
        }

        public List<PlaceNode> SeedPlaceNodes(List<State> states)
        {
            // Generate a list of PlaceNodes
            List<PlaceNode> placeNodes = new List<PlaceNode>();
            states.ForEach(s => s.Places.ForEach(p => placeNodes.Add(new PlaceNode(p))));
            return placeNodes;
        }

        public void AddLocationData(List<State> states)
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
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        public void AddLocationDataAsParallel(List<State> states)
        {
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
                            Console.WriteLine(ex.ToString());
                        }
                    }
                });

                state.Places.AddRange(placesToAdd);
            });
        }

        // Method for sorting our states. Added it as a method in case we later want to sort 
        // using a custom comparator 
        public List<State> SortStates(List<State> list)
        {
            return list.OrderBy(s => s.Name).ToList();
        }

        public void CheckForDuplicatesInEachState(List<State> list)
        {
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
        }

        public void CheckForDuplicatesInEachStateAsParallel(List<State> states)
        {
            List<string> results = new List<string>();

            Parallel.ForEach(states, state =>
            {
                Parallel.ForEach(state?.Places, place =>
                {
                    var count = state.Places.Where(p => p.Equals(place)).Count();
                    if (count > 1)
                    {
                        results.Add("Duplicate of: " + place + " found in " + state.Name);
                    }
                });
            });

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }

        public IEnumerable<PlaceNode> CheckForDuplicatesAcrossEachState(List<State> states, List<PlaceNode> placeNodes)
        {
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

            return duplicatePlaces;
        }

        public void CountOccurrencesOfEachPlace(List<State> list, List<PlaceNode> places)
        {
            foreach (var place in places)
            {
                Console.WriteLine("Number of times we've seen: " + place.Place + ": " + place.StatesThatHaveThisPlace.Count());
            }
        }
    }
}
