####################################################
Authors: Tyler Allen, Dallon Haley
Entry Dates(client): 
		 V.0.1 - 11/7/20
	     V.0.2 - 11/9/20
	     V.0.3 - 11/12/20
	     V.0.4 - 11/14/20
	     V.0.5 - 11/16/20
	     V.0.6 - 11/17/20
	     V.0.7 - 11/19/20
	     V.1 - 11/20/20

Entry Dates(server):
		 V.0.1 - 11/28/20
		 V.0.2 - 11/29/20
		 V.0.3 - 11/30/20
		 V.0.4 - 12/2/20
		 V.0.5 - 12/4/20
		 V.1 - 12/4/20
####################################################

TankGame Info: TankGame is a game where you can play with your friends where you play as a tank, with your objective to destroy enemy tanks to increase your score.

Basic Gameplay: TankGame allows you to move around (WASD) a world filled with walls, powerups, and other tanks. As you move around the map, you can shoot other tanks with your main attack(Left Click), or pick up powerups that spawn randomly to shoot an instant-kill beam attack(Right Click). Each tank has a total of 3 health, but after you are destroyed, you can spawn in again to get right back into the action.

Gameplay Elements: We chose to make several different design decisions that change/improve gameplay:
		1. Draw the world in a lower resolution to help with performance.
		2. Handle player movement in such a way where it feels seamless, smooth, and interactive.
		3. Draw the beam in a color that corresponds with the player's tank color.
		4. Draw a "Boom!" explosion image on the tank's destruction to add a little "oomph" when you are destroyed.
		5. Draw the health above each player, and the name/score below each player to simplify what you see, while keeping the
			information easily readable.

Server Elements:
		1. Reads all of the custom settings for the server from the settings.xml file, including world size, tank move speed, and any walls provided.
		2. Able to join a server that is constantly waiting for new clients, and sends out new world information every frame.
		3. Handles joining the server with a name, creating a new tank for the client, and connecting them to several other players in the same server.
		4. Handles disconnecting from the server gracefully, with no implications for other players.
		5. Handles player movement, projectile movement, and beam attacks. If a tank collides with a wall, the tank will be unable to move. If the tank collides with a projectile,
		   the tank's health decreases. If the tank were to die from a projectile or collide with another player's beam, the tank dies, and increments the score of the tanks destroyer.
		6. After the tank has died, the server handles finding a suitable respawn location.
		7. If the tank were to go outside the world borders, the tank will "wraparound" the world seamlessly, just like in games like Pac-man.

What Works:
		1. Joining a new server with the provided name
		2. Drawing the background/every element of the game seamlessly
		3. Moving/shooting your tank around the map
		4. Respawning after a tank is destroyed
		5. Picking up a powerup and using it to shoot a beam

External Resources: This README file. 