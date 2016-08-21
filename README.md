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