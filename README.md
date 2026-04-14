-- Phoenix Warren
-- CSCI 1260-002: Object-Oriented Programming
-- Spring 2026

This project is a simple, console-based implementation of the classic Minesweeper game, built using C# and .NET in Visual Studio. The player reveals tiles on a grid while avoiding hidden mines, using logic and deduction to clear the board safely.

The project follows a layered architecture where all game rules and logic are contained within the Core library, while the Console project is responsible only for user input and display output.

Enjoy :))



---------------------

***CONTROLS:***
Enter coordinates to reveal a tile
Enter a command to flag/unflag a tile

(Example: r 3 4 = reveal row 3, column 4)
(Example: f 3 4 = flag row 3, column 4)


***DISPLAY SYMBOLS:***
# = Hidden Tile
. = Revealed Empty Tile
1–8 = Number of adjacent mines
F = Flagged Tile
* = Mine
@ = Player cursor (if applicable)


***GAME RULES:***
The board is filled with hidden mines placed randomly
Numbers indicate how many mines are adjacent to a tile
Revealing a tile with a mine ends the game
Revealing all non-mine tiles wins the game
Flags can be placed to mark suspected mines


***GAME MECHANICS:***
If a tile has 0 adjacent mines, nearby tiles are revealed automatically
Flagging is optional but helps track mine locations
Incorrect flags do not end the game but may affect strategy


***WIN / LOSS CONDITIONS:***
✅ Win: All safe tiles are revealed
❌ Loss: A mine is revealed