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