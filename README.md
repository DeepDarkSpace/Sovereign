# Sovereign a Battleships SDK AI-Project

# BATTLESHIPS SDK NOT included contact me if interested

# Battleship

The Battleship project is a platform that allows different AIs to compete against each other in a tournament of the Age old Classic "Battleships". Everyone plays against everyone else. There are always 1000 matches played to determine the winner between two AIs. This is to avoid possible coincidences and really find out the better winner.
It is advisable to keep your own source code secret from the other participants during the development period so that they cannot prepare their own tactics.

## Structure

The project consists of 3 parts. The GameManager, the BattleSDK and the AI project.
The GameManager is the heart of the platform. It is started as an .exe, loads the AIs, lets them compete against each other and determines the winner.
The BattleSDK is a .dll that enables the participant to write his/her/their own AI that can be loaded by the system. 
The AI project is created by the participant. It is a class library (.dll). The project must reference the BattleSDK in order to access its functionality.
When the solution is built, the required files are located under battleship/GameManager/bin/Debug:

* /BattleSDK.dll
* /GameManager.exe
* /Players/TestKI.dll
* /Players/Overkill.dll

The AIs that are not needed can be removed from the Players folder, otherwise they will take part in the tournament.

## Battle SDK

The BattleSDK consists of 3 simple classes that allow integration into the Battleship system. BattleshipAI, Direction and Ship

## Direction

Direction is an enum that determines the orientation of a ship. A ship can be placed horizontally or vertically on the field.

## Ship

A ship is placed on the playing field.
It has the coordinates X and Y. Depending on the orientation of the ship, these determine either the topmost or the leftmost field of the ship.
It also has a size Size. Size is the number of squares that this ship occupies. 
A ship also has a Direction Dir.
Lastly, each ship also has a list of Hits. Each hit represents a hit from the enemy AI. The integer in the list describes the index of the field from the origin. So to get from a hit to the absolute position of the field, both the position of the ship and its orientation are necessary.

## BattleshipAI

This abstract class is used by the participant. The participant creates exactly one class that inherits from BattleshipAI.

`public BattleshipAI(int size);`

The constructor of the class. Size is the size of the playing field. Size = 10 means that the game is played on a 10x10 field.
Size can also be -1. In that case, no game is played, but the system needs an instance of the class to call GetName().

`public abstract void SetShips(List<Ship> ships);`

Before the game starts, each AI must place its ships on the board. To do this, this method is called and a list 

## Sovereign AI

It's simple baseline of AI targeted for early learners of C# it divides the Enemy Ocean in a Checkered Board to maximize hit potential
It is supossed to sparc interested and kickstart ideas to improve an AI in the tournament.

Try to read and understand it first before you do the funni.

Possible advances:
* Use a Method to determine the likeliness of orientation
* Write any hits to an external file and use those coordinates first
* After each round determine "Save Spots" for ships and place own ships accordingly
* Be Creative!