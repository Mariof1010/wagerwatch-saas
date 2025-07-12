Build Run Cmd
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

App Req
Wager object

Game object
League  /  Teams / Time / Etc about game
Starting Odds
some sort of periodic score capture .. (e.g.  every qtr, innning , period) , proably need to not get to granular in beginning.
This can be used to check how a users wager is performing.
-If possible capture real time odds at those points in time to help assess win probability of wager

Wager object
Users entered/imported wager  or they can choose to just track (no money just wathcing , maybe sunglasses instead of $ amt)
pointer to game to get all game properties
Users wager line (likely diff than opening odds)
Track wager status at game update periods (e.g.  Start of game neutral 50% win prob (calculated),  end first qtr green if winning 75% win prob (calculated))
UI can creat timeline of wager and status at diff periods throughout the game

HOW TO SHARE WITH CLAUDE or AI agents
Ask for RAW URL strings for all files in project
Make Git repo public
Share urls
Make Git repo private