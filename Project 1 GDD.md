# CSC 443 Project 1: VIRUS MAYHEM
- **Theme:** Turn based battle between up to 4 teams each team has up to 6 players.
- **Core Gameplay:**
  - Player chooses a faction from the team selection and gives a name to the team if no name is given the default name is the faction name (Player cannot choose the name of different selected faction).
  - Each player has 30 seconds to finish their turn.
  - Players can swap team member using the numpad. 
  - Third person shooter.
  - Player chooses weapons from their inventory.
  - Players can pick up crates to get new weapons. 
  - Players can pick up med kits to heal themselves.
  - Players can open an overlay to check the team’s ranking.
  - Players can open the map to see the position of each teams
- **Scripts:**
  - Game Manager: Manages the team’s turn and game duration and switch teams at the end of each turn and ends the game when the time runs out. 
  - UI Manager: Manages all transitions between UIs 
  - Team Manager: Manages the team’s health and activates each team member’s movement. 
  - Player Controller: Manages the player’s movement, shooting and manages the player’s health. 
  - Supply Drop: Base class that starts the turn once it collides with something 
  - Projectile: Base class for all weapons contains the explosion function. 
  - Camera Manager: Manages the camera’s target.
  - Sound Manager: Controls all sounds in the game
- **How to Play:**
  - Move around with W A S D or the keyboard arrows.
  - Right Click to aim. 
  - Left Click to charge projectile release to shoot. 
  - Hold Q to open the inventory release to hide. 
  - Hold Tab to open teams overlay release to hide. 
  - Hold M to open map overlay release to hide. 
  - Change players with the numpad. 
