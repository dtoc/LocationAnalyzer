# LocationAnalyzer
I drove through the United States and noticed some duplicate town and city names. I wonder...how many duplicates are there?
How many street names occur more than once despite not being shared on some contiguous road? How many cities have the same 
name despite having no relation? 

Why not scrape Wikipedia or some other data store...and assesmble a list of every town and city in every state...
and then get to work with figuring out how many dupes there are and which name is most common?! Sure sounds like
a fun little project. ;D

First I will try to grab the data I need from Wikipedia's pages directly. It's possible to download Wikipedia pages as 
XML files, so I will also try to incorporate local data at a later date. 

To see a sample of the output, take a look at the practice.txt file I've attached. 

8/21/2016
When I'm grabbing links for each state object, I currently look for a few different types of links. For example, 
"List_of_places_in" and "List_of_cities_in" links might contain different data. Therefore I want to process
those two pages as separate sources of data. Right now I have eight pages that I might want to process individually
when getting each state's places. They are:

"List_of_places_in"
"List_of_cities_in"
"List_of_towns_in"
"List_of_cities_and_towns_in"
"List_of_municipalities_in"
"List_of_populated_places"
"List_of_census-designated-places_in"
"List_of_unincorporated"

I want to make sure that each state's list of "places" is as complete as possible. To that effect, I want to grab 
a state's places, towns, cities, municipalities, etc. A city might also be called a town or a village, but I found
some cases where a list of towns will have places that a list of cities will not, and vice versa. To get as complete a 
picture as possible into a state's places, I will write slightly different parsing code so that I can get data from
each of these lists. The list of cities might have its html structured differently than the list of towns, so different
parsing strategies will be necessary for each page.

8/29/2016

Apologies for the delay - I was doing some traveling and then was slammed by work and life in general. Today
I worked on making my parsing strategy a little more inclusive. The last time I worked on this code, my logfile
was being filled with around 13KB of useful data. But I noticed that some states had a noticeable lack of places!
So I worked on the parsing strategy some more to grab locations even from HTML lines that don't have a "td scope"
as an easy parsing hook. This skyrocketed my usable data from 13KB to over 500KB! The list of states and their places
is so much more massive because now I'm getting a much bigger picture of what places are in each state.

I still need to work on the parsing some more, though. The code itself isn't very clean. The next thing I want to add
is a constraint check that will make the parsing cleaner. More details on that in the form of an explanation once the
code is implemented and pushed. Basically, a constraint check is a useful technique for declaring what your search agent
should never violate during its search. For example, my search agent should never consider a line of HTML as relevant
if it contains "List of" anywhere, as my visual inspection of the HTML has already proven to me that lines with valuable
place data aren't going to have "List of" anywhere in them. So I can safely discard a line if it has "List of" in it when
I'm searching for a state's places. A constraint function would be used to periodically check whether or not the agent is
violating any such rules. It's used as a guiding principle while the agent is working so that it can take a step back
if it ever sees that it's about to violate a constraint. 

-- DERP. The logfile was over 500KB because there were duplicates already present since I processed similar links more than 
once. :D Needed to correct that error asap!

I also added some methods to check for duplicates in each state and across each state. When I say in each state, I mean
that I want to know if there's any duplicate towns/cities/etc inside of a state. When I say across each state, I mean
that I want to know if there's any town/city/etc that appears in multiple states. The results are very preliminary. I still
need to clean up the parsing and clean up the code before I dive more into getting results to the question of how many duplicates
there are. But I was excited to get a small glimpse ahead of time, which is why I added those new methods now. :)

-- Since some states' list of places is sorted alphabetically, I should be able to implement a nice searching algorithm
for when I want to search for duplicates or for the number of times something occurs. Right now, if I take place X and want to
see how many times that place occurs in every state, I check every place in every state's list of places. That's not very smart
because it's doing a lot of unnecesssary work - after all, if you're searching an alphabetized list and you've gotten far enough
into the list to know that none of the rest of the items could possibly match, you can stop searching that list and move on to
the next state's list of places. I'm really excited to see how much faster the application will be with that change! For the
states that have a list that isn't sorted, I bet I can sort the list before doing the search and then do a smarter search...
and have it perform faster than not sorting and just searching everything.

Will be an interesting experiment!

9/10/2016

Keeping a detailed log of everything I worked on is hard! I intend to write a more detailed entry on everything I've done
and what I've experimented with. But for now, suffice it to say that a slightly less preliminary look at the data shows that
the real number of duplicates is potentially in the thousands. Meaning there might be several thousand names that are shared 
for cities, towns, villages, etc. That number will become more clear as I work on my code some more. Today I've experimented a bit
with profiling strategies and with parallelism in my code for faster performance. Tomorrow I intend to play around with the actual
problem at hand so that I don't get too sidetracked with parallelism or other fun things. :D

9/11/2016

Did some code clean up today! Added a new version of the method that checks for duplicates. It's much faster and cleaner 
than the garbage I had before. :D I also pulled out individual references to StreamWriter and created just a single global
StreamWriter object that anything in the main class can access. And cleaned up odds and ends. Here's a sample of what the 
output looks like right now:

---
Found duplicates of: Vienna in: 

	Georgia
	Illinois
	Maine
	Missouri
	New York
	South Dakota
	Virginia
	Wisconsin
---

Nice, eh? :D Next up I need to work on:

1) Add cleaner code for constraining the content of each page as my agent crawls Wikipedia. Some of my results consider counties 
to be cities. Counties are not cities - they often encompass cities. So a smarter agent with better constraint checking can help me
eliminate bad results. (So far the results are pretty awesome!)

2) Cache results from the web crawling. That way my agent can check for cached results first and avoid crawling the web entirely if
it isn't needed. This way if I want to experiment with faster methods for crunching the data I can do so without having to waste time
crawling the web for the same exact data each time I execute the app.

3) Format the data into JSON or some other format and add a nice visualization. It'd be cool to slap this on the web for anyone to 
visualize immediately without needing to download and execute a console app that spits out text results. :D

And there's much more to work on! Very fun little sided project so far. Learning a ton about parallelism and crawling the web
and just writing code in general. Tons of tun. :D 
