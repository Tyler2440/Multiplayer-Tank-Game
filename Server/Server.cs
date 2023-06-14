//////////////////////////////////////////////
///FileName: Server.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/28/2020
///Description: Server class handles updating world and returning updated world to all clients
/////////////////////////////////////////////
using NetworkUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using TankGameWorld;
using TankWars;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Server
{
    /// <summary>
    /// Class handles all Server networking and world updates 
    /// Server will handle handshake protocol, take in control commands from clients and send updated world each frame to all active clients 
    /// </summary>
    class Server
    {
        public static Dictionary<long, SocketState> clients; //collection of all clients currently connected to server
        public static Dictionary<int, ControlCommands> commands; //collection of all client's most recent control command request 
        private static World theWorld; //instance of game world 
        private static Settings settings; //instance of game settings 

        static void Main(string[] args)
        {
            Server server = new Server();

            //Read XML File for settings 
            ReadXML();

            //start server / tcp listener         
            Networking.StartServer(OnConnection, 11000);

            Stopwatch watch = new Stopwatch();

            //update clients within infinite loop 
            while (true)
            {
                watch.Start();

                //wait until total frame time has elapsed before updating the world 
                while (watch.ElapsedMilliseconds < settings.GetTimePerFrame())
                { /* do nothing */}

                watch.Restart();

                Update();
            }

        }

        /// <summary>
        /// Constructor sets up server instance variables 
        /// </summary>
        public Server()
        {
            clients = new Dictionary<long, SocketState>();
            theWorld = new World();
            settings = new Settings();
            commands = new Dictionary<int, ControlCommands>();

        }

        /// <summary>
        /// Helper method which updates all world objects and sends updated data to each connected client
        /// 
        /// Updates include moving the tank, checking for collisions, handling tank deaths and respawns, updating score, and spawining powerups 
        /// </summary>
        private static void Update()
        {
            //check to see if powerup needs spawned 
            if (theWorld.Powerups.Count < Settings.GetMaxPowerups())
            {
                //if powerup is ready to be spawned, available spawn location is found and powerup is added to the world 
                if (theWorld.GetPowerupReady())
                {
                    Vector2D spawn = FindSpawnLocation();
                    Powerup p = new Powerup();
                    p.SetID(theWorld.GetPowerupCounter());
                    p.SetLocation(spawn);
                    theWorld.Powerups.Add(theWorld.GetPowerupCounter(), p);
                    theWorld.SetPowerupCounter(theWorld.GetPowerupCounter() + 1);
                }
            }

            StringBuilder serialize = new StringBuilder();
            ControlCommands cmd;

            //handle tank updates 
            lock (theWorld.Tanks)
            {
                foreach (Tank tank in theWorld.Tanks.Values)
                {
                    if (tank.GetHitPoints() > 0) //update movement for all tanks currently alive 
                    {

                        if (tank.Disconnected()) //let other tanks know that the tank disconnected 
                        {
                            tank.SetHealth(0);
                            tank.SetDied(true);
                        }

                        if (commands.ContainsKey(tank.GetID()))
                        {
                            lock (commands)
                            {
                                cmd = commands[tank.GetID()];
                            }

                            HandleTankMovement(tank, cmd); //moves tank in in direction given by client by the tank speed in settings 

                            //Set the turret direction as given by the client control command
                            Vector2D tdir = cmd.GetOrientation();
                            Vector2D zeroZero = new Vector2D(0, 0);

                            if (!tdir.Equals(zeroZero))
                                tdir.Normalize();

                            tank.SetTurretDirection(tdir);

                            //check for tank firing commands 
                            string projType = cmd.GetFire();
                            if (projType == "main" && tank.GetFireDelay() == 0)
                            {
                                //create new projectile and add it to the world 
                                Projectile proj = new Projectile();
                                proj.SetDirection(tank.GetAiming());
                                proj.SetOwner(tank.GetID());
                                proj.SetLocation(tank.GetLocation());
                                proj.SetID(theWorld.GetProjCounter());

                                theWorld.Projectiles.Add(theWorld.GetProjCounter(), proj);
                                theWorld.SetProjCounter(theWorld.GetProjCounter() + 1);

                                //set fire delay so tank needs to wait to shoot again 
                                tank.SetFireDelay((int)settings.GetProjFireDelay());
                            }

                            else if (projType == "alt" && tank.HasPowerup())
                            {
                                //create beam and add it to the world 
                                Beam beam = new Beam();
                                beam.SetDirection(tank.GetAiming());
                                beam.SetOwner(tank.GetID());
                                beam.SetOrigin(tank.GetLocation());
                                beam.SetID(theWorld.Beams.Count);

                                theWorld.Beams.Add(theWorld.Beams.Count, beam);
                                //remove beam abilities from tank once it has been used
                                tank.SetsHasPowerup(false); 
                            }

                            if (tank.GetFireDelay() > 0)
                                tank.SetFireDelay(tank.GetFireDelay() - 1);

                        }
                    }

                    //serialize tank to be sent to each active client 
                    serialize.Append(JsonConvert.SerializeObject(tank) + "\n");

                    //after tank data has been serialized, remove disconnected tanks from the world model 
                    if (tank.Disconnected()) 
                    {
                        theWorld.Tanks.Remove(tank.GetID());
                    }
                    //after tank data has been serialized, respawn tank/set died to false
                    if (tank.GetHitPoints() == 0)
                    {
                        if (tank.GetRespawnDelay() <= 0)
                        {
                            tank.SetHealth(Settings.GetMaxHP());
                            tank.SetLocation(FindSpawnLocation());
                        }

                        else
                            tank.SetDied(false);
                    }
                }
            }

            //handle projectile updates 
            lock (theWorld.Projectiles)
            {
                foreach (Projectile proj in theWorld.Projectiles.Values)
                {
                    //remove previously dead projectiles 
                    if (proj.Died)
                        theWorld.Projectiles.Remove(proj.GetID());

                    Vector2D location = proj.GetLocation();
                    Vector2D direction = proj.GetDirection();
                    direction.Normalize();

                    //destroy projectile if it collides with wall 
                    if (DetectProjWallCollision(location))
                    {
                        proj.Died = true;
                        theWorld.Projectiles.Remove(proj.GetID());
                    }
                    //otherwise, move projectile in direction of tank aiming by the projectile speed in settings 
                    else
                    {
                        location += proj.GetDirection() * settings.GetProjSpeed();
                        if (DetectProjOutOfBounds(location))
                            proj.Died = true;

                        proj.SetLocation(location);
                    }

                    //check to see if projectile collides with any tanks in the frame
                    DetectTankProjectileCollisions(proj);

                    //serialize projectile to be sent to each client
                    serialize.Append(JsonConvert.SerializeObject(proj) + "\n");
                }
            }

            //handle powerups updates
            lock (theWorld.Powerups)
            {
                foreach (Powerup powerup in theWorld.Powerups.Values)
                {
                    //check to see if tank collides with powerup 
                    if (DetectTankPowerupCollisions(powerup.GetLocation()))
                    {
                        powerup.Died = true;
                    }
                    
                    //serialize powerup data to be sent to  each client
                    serialize.Append(JsonConvert.SerializeObject(powerup) + "\n");

                    //after powerup has been serialized to notify clients, remove powerup from world 
                    if (powerup.Died)
                    {
                       theWorld.Powerups.Remove(powerup.GetID());
                    }
                }
            }

            //handle beam updates
            lock (theWorld.Beams)
            {
                foreach (Beam beam in theWorld.Beams.Values)
                {
                    //check each tank to see if active beam collides with it 
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        //if beam collision is detected, set tank as dead and update score 
                        if (DetectBeamCollisions(beam.GetOrigin(), beam.GetDirection(), tank.GetLocation(), settings.GetTankSize() / 2))
                        {
                            tank.SetDied(true);
                            tank.SetHealth(0);

                            theWorld.Tanks[beam.GetOwner()].SetScore(int.Parse(theWorld.Tanks[beam.GetOwner()].GetScore()) + 1);
                        }
                    }

                    //serialize and then remove beam so that it updates clients only for this frame
                    serialize.Append(JsonConvert.SerializeObject(beam) + "\n");
                    theWorld.Beams.Remove(beam.GetID());
                }
            }

            //send seriealized data to each active client 
            lock (clients)
            {
                foreach (SocketState client in clients.Values)
                {
                    if (client.ErrorOccured)
                    {
                        clients.Remove(client.ID);
                    }

                    else
                        Networking.Send(client.TheSocket, serialize.ToString());
                }
            }

            // Reset the stringbuilder
            serialize = new StringBuilder();
        }

        /// <summary>
        /// check if projectile goes out of bounds 
        /// </summary>
        /// <param name="location"> location of projectile </param>
        /// <returns></returns>
        private static bool DetectProjOutOfBounds(Vector2D location)
        {
            double x = location.GetX();
            double y = location.GetY();

            if (x < -settings.GetUniverseSize() / 2)
                return true;

            else if (x > settings.GetUniverseSize() / 2)
                return true;

            if (y < -settings.GetUniverseSize() / 2)
                return true;

            else if (y > settings.GetUniverseSize() / 2)
                return true;

            return false;
        }

        /// <summary>
        /// Checks the control commands for given tank and moves tank as needed by the tank speed in settings 
        /// Method will not move tank if it collides with wall
        /// Tanks will also wrap around world if tank goes out of bounds
        /// </summary>
        /// <param name="tank"> tank being moved </param>
        /// <param name="cmd"> control command for tank </param>
        private static void HandleTankMovement(Tank tank, ControlCommands cmd)
        {
            Vector2D vector = tank.GetLocation();
            Vector2D velocity = new Vector2D(0, 0);
            double tankSpeed = settings.GetTankSpeed();

            //check control command movement direction and set orientation 
            string movement = cmd.GetMoving();
            if (movement == "up")
            {
                velocity = new Vector2D(0, -1);
                tank.SetOrientation(new Vector2D(0, -1));
            }

            else if (movement == "down")
            {
                velocity = new Vector2D(0, 1);
                tank.SetOrientation(new Vector2D(0, 1));
            }

            else if (movement == "right")
            {
                velocity = new Vector2D(1, 0);
                tank.SetOrientation(new Vector2D(1, 0));
            }

            else if (movement == "left")
            {
                velocity = new Vector2D(-1, 0);
                tank.SetOrientation(new Vector2D(-1, 0));
            }

            //multiply orientation by tank speed and add it to the old tank location 
            velocity *= tankSpeed;
            velocity += vector;

            //check for wall collisions and set new tank location 
            if (!DetectWallCollisions(velocity))
                tank.SetLocation(DetectWraparound(velocity));

        }

        /// <summary>
        /// Checks if tank goes out of bounds
        /// Returns current location if tank is still wtihin bounds, or returns the wrap around location 
        /// if tank goes out of bounds
        /// </summary>
        /// <param name="velocity"> location tank if trying to move to </param>
        /// <returns>updated tank location </returns>
        private static Vector2D DetectWraparound(Vector2D velocity)
        {
            double x = velocity.GetX();
            double y = velocity.GetY();

            if (x < -settings.GetUniverseSize() / 2)
                x = settings.GetUniverseSize() / 2 - (settings.GetTankSize() / 2 + settings.GetWallSize());

            else if (x > settings.GetUniverseSize() / 2)
                x = -settings.GetUniverseSize() / 2 + (settings.GetTankSize() / 2 + settings.GetWallSize());

            if (y < -settings.GetUniverseSize() / 2)
                y = settings.GetUniverseSize() / 2 - (settings.GetTankSize() / 2 + settings.GetWallSize());

            else if (y > settings.GetUniverseSize() / 2)
                y = -settings.GetUniverseSize() / 2 + (settings.GetTankSize() / 2 + settings.GetWallSize());


            return new Vector2D(x, y);
        }

        /// <summary>
        /// Checks if projectile collides with any walls and returns true if it does 
        /// </summary>
        /// <param name="vector"> location of projectile </param>
        /// <returns>whether projectile collides with wall </returns>
        private static bool DetectProjWallCollision(Vector2D vector)
        {
            foreach (Wall wall in theWorld.Walls.Values)
            {
                double p1X = wall.GetLocationP1().GetX();
                double p1Y = wall.GetLocationP1().GetY();
                double p2X = wall.GetLocationP2().GetX();
                double p2Y = wall.GetLocationP2().GetY();

                //Vertical wall 
                if (p1X == p2X)
                {
                    p1X += settings.GetWallSize() - 8;
                    p2X -= settings.GetWallSize() - 8;
                    
                    //return true if projectile is within the bounds of wall 
                    if (vector.GetX() < p1X && vector.GetX() > p2X)
                    {
                        if (p1Y < p2Y)
                        {
                            p1Y -= settings.GetWallSize() - 8;
                            p2Y += settings.GetWallSize() - 8;

                            if (vector.GetY() > p1Y && vector.GetY() < p2Y)
                                return true;
                        }

                        else if (p2Y < p1Y)
                        {
                            p2Y -= settings.GetWallSize() - 8;
                            p1Y += settings.GetWallSize() - 8;

                            if (vector.GetY() < p1Y && vector.GetY() > p2Y)
                                return true;
                        }
                    }
                }

                // Horizontal wall
                if (p1Y == p2Y)
                {
                    p1Y += settings.GetWallSize() - 8;
                    p2Y -= settings.GetWallSize() - 8;

                    //return true if projectile is within the bounds of wall 
                    if (vector.GetY() < p1Y && vector.GetY() > p2Y)
                    {
                        if (p1X < p2X)
                        {
                            p1X -= settings.GetWallSize() - 8;
                            p2X += settings.GetWallSize() - 8;

                            if (vector.GetX() > p1X && vector.GetX() < p2X)
                                return true;
                        }

                        else if (p2X < p1X)
                        {
                            p1X += settings.GetWallSize() - 8;
                            p2X -= settings.GetWallSize() - 8;

                            if (vector.GetX() < p1X && vector.GetX() > p2X)
                                return true;
                        }
                    }
                }
            }
            return false; //return false if projectile is not within bounds of any wall 
        }

        /// <summary>
        /// Checks if projectile collides with any active tanks in the world
        /// </summary>
        /// <param name="proj"> projectile in question </param>
        /// <returns> bool representing whether projectile collides with any tanks </returns>
        private static bool DetectTankProjectileCollisions(Projectile proj)
        {
            Vector2D projLoc = proj.GetLocation();
            foreach (Tank tank in theWorld.Tanks.Values)
            {
                if (proj.Died) //skip projectile if it is already dead
                    continue;

                int distance = (int)(projLoc - tank.GetLocation()).Length();

                //check if distance between projectile and tank is less than tank radius
                if (distance < (settings.GetTankSize() / 2) && (proj.GetOwner() != tank.GetID()))
                {
                    if (tank.GetHitPoints() <= 0) //skip collision detection if tank is dead already 
                        continue;

                    proj.Died = true; //destroy projectile 

                    tank.DeductHealth(); 

                    if (tank.GetHitPoints() <= 0) //check if tank is now dead
                        tank.SetDied(true);

                    //update world if tank is dead 
                    if (tank.GetDied())
                    {
                        theWorld.Tanks[proj.GetOwner()].SetScore(int.Parse(theWorld.Tanks[proj.GetOwner()].GetScore()) + 1);
                        tank.SetRespawnDelay((int)Settings.GetRespawnDelay());
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if powerup collides with any tanks in the world
        /// if tank picks up powerup, method updates tank object to have powerup 
        /// </summary>
        /// <param name="vector"> location of powerup </param>
        /// <returns> whether powerup was picked up by tank </returns>
        private static bool DetectTankPowerupCollisions(Vector2D vector)
        {
            foreach (Tank tank in theWorld.Tanks.Values)
            {
                int distance = (int)(vector - tank.GetLocation()).Length();

                //if powerup is within tanks radius, give powerup to tank 
                if (distance < (settings.GetTankSize() / 2))
                {
                    tank.SetsHasPowerup(true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if given tank collides with wall 
        /// </summary>
        /// <param name="vector"> tank location </param>
        /// <returns> whether tank collides with wall or not </returns>
        private static bool DetectWallCollisions(Vector2D vector)
        {
            foreach (Wall wall in theWorld.Walls.Values)
            {
                double p1X = wall.GetLocationP1().GetX();
                double p1Y = wall.GetLocationP1().GetY();
                double p2X = wall.GetLocationP2().GetX();
                double p2Y = wall.GetLocationP2().GetY();

                // Vertical wall
                if (p1X == p2X)
                {
                    p1X += settings.GetTankSize() - 5;
                    p2X -= settings.GetTankSize() - 5;

                    //if tank is within wall bounds, returns true
                    if (vector.GetX() < p1X && vector.GetX() > p2X)
                    {
                        if (p1Y < p2Y)
                        {
                            p1Y -= settings.GetTankSize() - 5;
                            p2Y += settings.GetTankSize() - 5;

                            if (vector.GetY() > p1Y && vector.GetY() < p2Y)
                                return true;
                        }

                        else if (p2Y < p1Y)
                        {
                            p2Y -= settings.GetTankSize() - 5;
                            p1Y += settings.GetTankSize() - 5;

                            if (vector.GetY() < p1Y && vector.GetY() > p2Y)
                                return true;
                        }
                    }
                }

                // Horizontal wall
                if (p1Y == p2Y)
                {
                    p1Y += settings.GetTankSize() - 5;
                    p2Y -= settings.GetTankSize() - 5;

                    //if tank is within wall bounds, returns true
                    if (vector.GetY() < p1Y && vector.GetY() > p2Y)
                    {
                        if (p1X < p2X)
                        {
                            p1X -= settings.GetTankSize() - 5;
                            p2X += settings.GetTankSize() - 5;

                            if (vector.GetX() > p1X && vector.GetX() < p2X)
                                return true;
                        }

                        else if (p2X < p1X)
                        {
                            p1X += settings.GetTankSize() - 5;
                            p2X -= settings.GetTankSize() - 5;

                            if (vector.GetX() < p1X && vector.GetX() > p2X)
                                return true;
                        }
                    }
                }
            }
            return false; //if tank is not colliding with wall, return false 
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool DetectBeamCollisions(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Finds spawn location on map that is within world bounds and is not instersecting a wall 
        /// </summary>
        /// <returns> Vector2D of avalable spawn location </returns>
        private static Vector2D FindSpawnLocation()
        {
            while (true)
            {
                //pick random spawn point within world and retun if is does not collide with wall 
                Random rand = new Random();
                int x = rand.Next(-(int)settings.GetUniverseSize() / 2, (int)settings.GetUniverseSize() / 2);
                int y = rand.Next(-(int)settings.GetUniverseSize() / 2, (int)settings.GetUniverseSize() / 2);
                Vector2D spawnLoc = new Vector2D(x, y);

                if (!DetectWallCollisions(spawnLoc))
                    return spawnLoc;
            }


        }

        /// <summary>
        /// Reads through XML file and sets server settings. EDIT COMMENTS
        /// </summary>
        private static void ReadXML()
        {
            // Took basic comments and skeleton from XMLDemo from Examples Repository.
            try
            {
                // Create an XmlReader inside this block, and automatically Dispose() it at the end.
                using (XmlReader reader = XmlReader.Create("..\\..\\..\\..\\TankGameResources\\settings.xml"))
                {
                    // Initializes a wall, idCounter, pX, and py variables that hold each wall's id, and editable p1/p2 coordinates
                    Wall wall = new Wall();
                    int idCounter = 0;
                    int pX = 0;
                    int pY = 0;

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                // In the case of reader.Name being "GameSettings", do nothing
                                case "GameSettings":
                                    break;

                                // In the case of reader.Name being "UniverseSize", edit the Setting's universe size to be sent to the user
                                case "UniverseSize":
                                    reader.Read();
                                    settings.SetUniverseSize(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "MSPerFrame", edit the Setting's TimePerFrame to know how often to update the game
                                case "MSPerFrame":
                                    reader.Read();
                                    settings.SetTimePerFrame(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "FramesPerShot", edit the Setting's ProjFireDelay to know how often a tank can shoot
                                case "FramesPerShot":
                                    reader.Read();
                                    settings.SetProjFireDelay(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "RespawnRate", edit the Setting's RespawnDelay to know how long to wait until a tank respawn
                                case "RespawnRate":
                                    reader.Read();
                                    settings.SetRespawnDelay(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "MaxHP", edit the Setting's MaxHP to know how tanks starting HP 
                                case "MaxHP":
                                    reader.Read();
                                    settings.SetMaxHP(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "ProjSpeed", edit the Setting's ProjSpeed to know how far to move projectile each frame 
                                case "ProjSpeed":
                                    reader.Read();
                                    settings.SetProjSpeed(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "TankSpeed", edit the Setting's TankSpeed to know how far to move tank each frame
                                case "TankSpeed":
                                    reader.Read();
                                    settings.SetTankSpeed(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "TankSize", edit the Setting's TankSize to know how large a tanks hit box is 
                                case "TankSize":
                                    reader.Read();
                                    settings.SetTankSize(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "WallSize", edit the Setting's WallSize to know how large a wall's hit box is 
                                case "WallSize":
                                    reader.Read();
                                    settings.SetWallSize(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "MaxPowerups", edit the Setting's MaxPowerups to know what the maximum amount of powerups on the map at any given time is 
                                case "MaxPowerups":
                                    reader.Read();
                                    settings.SetMaxPowerups(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "MaxPowerupDelay", edit the Setting's MaxPowerupDelay to know what the maximum frames for a powerup to respawn is 
                                case "MaxPowerupDelay":
                                    reader.Read();
                                    settings.SetMaxPowerupDelay(int.Parse(reader.Value));
                                    theWorld.SetMaxPowerupDelay(int.Parse(reader.Value));
                                    break;

                                // In the case of reader.Name being "Wall", do nothing
                                case "Wall":
                                    break;

                                // In the case of reader.Name being "P1", do nothing
                                case "P1":
                                    break;

                                // In the case of reader.Name being "P2", do nothing
                                case "P2":
                                    break;

                                // In the case of reader.Name being "x", change the pX variable to this x position
                                case "x":
                                    reader.Read();
                                    pX = int.Parse(reader.Value);
                                    break;

                                // In the case of reader.Name being "y", change the pY variable to this y position. We now know both the x and y for either P1 or P2,
                                // so we can update this wall's either P1 or P2 position. If we have updated both, set its ID, add it to the walls, and reset the 
                                // "wall" variable to be updated again.
                                case "y":
                                    reader.Read();
                                    pY = int.Parse(reader.Value);

                                    // If we have not updated P1 yet, create a new Vector2D and set the wall's P1 location.
                                    if (wall.GetLocationP1() == null)
                                    {
                                        Vector2D p1 = new Vector2D(pX, pY);
                                        wall.SetLocationP1(p1);
                                    }

                                    // If we have not updated P2 yet, create a new Vector2D and set the wall's P2 location.
                                    else if (ReferenceEquals(wall.GetLocationP2(), null))
                                    {
                                        Vector2D p2 = new Vector2D(pX, pY);
                                        wall.SetLocationP2(p2);
                                    }

                                    // If neither P1 or P2 are unknown, add the wall to theWorld.Walls and update the unique ID counter.
                                    if (!ReferenceEquals(wall.GetLocationP1(), null) && !ReferenceEquals(wall.GetLocationP2(), null))
                                    {
                                        lock (theWorld.Walls)
                                        {
                                            wall.SetID(idCounter);
                                            theWorld.Walls.Add(idCounter, wall);
                                        }
                                        idCounter++;
                                        wall = new Wall();
                                    }
                                    break;

                            }
                        }
                    }
                }
            }

            catch (XmlException e)
            {
                throw e;
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// On client connection, establish the connection and start receiving data.
        /// </summary>
        /// <param name="state">Client's SocketState</param>
        public static void OnConnection(SocketState state)
        {
            if (state.ErrorOccured)
                return;

            state.OnNetworkAction = ReceiveName;
            Networking.GetData(state);
        }

        /// <summary>
        /// On the client sending the player name, create a unique tank for the client and begin sending the client's
        /// ID, world size, and wall data.
        /// </summary>
        /// <param name="state"></param>
        public static void ReceiveName(SocketState state)
        {
            string temp = state.GetData();
            Tank tank = new Tank();
            tank.SetName(temp.Substring(0, temp.Length - 1));
            tank.SetID((int)state.ID);

            // Clear the socket state's StringBuilder
            state.RemoveData(0, temp.Length);

            //add tank to the world
            lock (theWorld.Tanks)
            {
                theWorld.Tanks.Add(tank.GetID(), tank);
            }

            SetTank(tank); //set up initial tank for client

            //send data to client
            SendHandshakeData(state, tank, 2000);
            SendWallData(state);

            //change on network action to receive normal control command requests from client
            state.OnNetworkAction = ReceiveCommand;


            lock (clients)
            {
                clients[state.ID] = state;
            }

            Networking.GetData(state); //continue receive loop 
        }

        /// <summary>
        /// Updates a tank's initial spawn and information.
        /// </summary>
        /// <param name="tank">tank to be set up </param>
        private static void SetTank(Tank tank)
        {
            lock (tank)
            {
                Vector2D vector = FindSpawnLocation();
                Vector2D vectorTurret = new Vector2D(0, -1);

                tank.SetLocation(vector);
                vectorTurret.Normalize();
                tank.SetTurretDirection(vectorTurret);
                tank.SetOrientation(vectorTurret);
            }
        }

        /// <summary>
        /// Sends the tank ID and world size to the client.
        /// </summary>
        /// <param name="state">Client's SocketState</param>
        /// <param name="tank">Client's Tank</param>
        /// <param name="worldSize">World Size</param>
        private static void SendHandshakeData(SocketState state, Tank tank, int worldSize)
        {
            string info = tank.GetID() + "\n" + worldSize + "\n";
            Networking.Send(state.TheSocket, info);
        }

        /// <summary>
        /// Sends the wall data to the client.
        /// </summary>
        /// <param name="state">Client's SocketState</param>
        private static void SendWallData(SocketState state)
        {
            StringBuilder builder = new StringBuilder();
            lock (theWorld.Walls)
            {
                foreach (Wall wall in theWorld.Walls.Values)
                {
                    builder.Append(JsonConvert.SerializeObject(wall) + "\n");
                }
            }
            Networking.Send(state.TheSocket, builder.ToString());
        }

        /// <summary>
        /// On a client sending movement information, create a ControlCommands to be used later to update each tank's information.
        /// </summary>
        /// <param name="state">Client's SocketState</param>
        private static void ReceiveCommand(SocketState state)
        {
            //if error occured, client is disconnected and their tank is set to dead 
            if (state.ErrorOccured)
            {
                lock (clients)
                {
                    theWorld.Tanks[(int)state.ID].SetDisconnected(true);
                    theWorld.Tanks[(int)state.ID].SetHealth(0);
                    theWorld.Tanks[(int)state.ID].SetDied(true);
                    clients.Remove(state.ID);
                }
                return;
            }

            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            state.RemoveData(0, totalData.Length);

            foreach (string part in parts)
            {
                if (part.Length == 0)
                {
                    continue;
                }

                if (part[part.Length - 1] != '\n')
                {
                    break;
                }

                JObject obj = JObject.Parse(part);

                JToken token = obj["moving"];
                if (token != null)
                {
                    //deserialize control command to be used to update world 
                    ControlCommands cmd = JsonConvert.DeserializeObject<ControlCommands>(part);

                    //add control command object to dictionary so the Update() method can use it 
                    int tankID = (int)state.ID;
                    if (commands.ContainsKey(tankID))
                    {
                        lock (commands)
                        {
                            commands[tankID] = cmd;
                        }
                    }
                    else
                    {
                        lock (commands)
                        {
                            commands.Add(tankID, cmd);
                        }
                    }
                }
            }

            Networking.GetData(state); //continue receive loop 
        }
    }
}