dotnet watch run --urls "http://localhost:5555"



// App purpose
-Bet tracking app hosted on web with option to run as local app.   
-Ideally users could import bets from a betting site such as FanDuel , DraftKings etc but since those
APIs may not be avail then entering manually should be option.  Perhaps importing from screen captures could ease the import method.
-The primary feature should be visualiztion of bet(s) peformance  an "aura".  The "how am I doing" will vary
on scope of bets (active, day, past week, etc). The visual representation should have 2 parts to it: current state (green:good yellow:neutral  red:bad),
but should also have a volitility component (early in game winning / losing etc is very volatile  versus late in the game)
-The web based appp is prob a good start but ideally the user could have an icon on their mobile or some indicator of aura.
Not sure that is possible so perhaps push notifications when aura changes for a particular bet, or the day status or whatever scope.