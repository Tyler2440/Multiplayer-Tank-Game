//////////////////////////////////////////////
///FileName: World.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Represents the World and everything the world holds.
/////////////////////////////////////////////
using Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankGameWorld
{
    public class World
    {
        // Holds all Tanks in the world
        public Dictionary<int, Tank> Tanks;

        // Holds all Walls in the world
        public Dictionary<int, Wall> Walls;

        // Holds all projectiles in the world
        public Dictionary<int, Projectile> Projectiles;

        // Holds all powerups in the world
        public Dictionary<int, Powerup> Powerups;

        // Holds all beams in the world
        public Dictionary<int, Beam> Beams;

        // Holds this client's tank ID
        public int tankID;

        // Holds the number of projectiles in the world to use as IDs
        private int projCounter = 0;

        // Holds the amount of time before next powerup spawns
        private int powerupTimer = 0;

        // Holds how many powerups are in the world
        private int powerupCounter = 0;

        // Holds the max amount of time until a new powerup spawns
        private int maxPowerupDelay = 0;

        /// <summary>
        /// Initializes each Dictionary
        /// </summary>
        public World()
        {
            Tanks = new Dictionary<int, Tank>();
            Walls = new Dictionary<int, Wall>();
            Projectiles = new Dictionary<int, Projectile>();
            Powerups = new Dictionary<int, Powerup>();
            Beams = new Dictionary<int, Beam>();
        }

        /// <summary>
        /// Sets the total projectile counter.
        /// </summary>
        /// <param name="count">Number of projectiles</param>
        public void SetProjCounter(int count)
        {
            projCounter = count;
        }

        /// <summary>
        /// Gets the total projectile counter.
        /// </summary>
        /// <returns>Number of projectiles</returns>
        public int GetProjCounter()
        {
            return projCounter;
        }

        /// <summary>
        /// Gets the amount of time until a powerup can spawn.
        /// </summary>
        /// <returns>Time until powerup spawn</returns>
        private int GetPowerupTimer()
        {
            return this.powerupTimer;
        }

        /// <summary>
        /// Sets the base amount of time before a new powerup spawns
        /// </summary>
        /// <param name="value">Time in frames</param>
        public void SetPowerupTimer(int value)
        {
            this.powerupTimer = value;
        }

        /// <summary>
        /// Gets amount of time before a new powerup spawns
        /// </summary>
        /// <returns></returns>
        public bool GetPowerupReady()
        {
            // If it is not time for a new powerup to spawn, decrease the amount of time until a new one will spawn
            if (GetPowerupTimer() > 0)
                SetPowerupTimer(GetPowerupTimer() - 1);

            // Otherwise, if a new powerup just spawned, find some random new time until another powerup spawns
            else
            {
                Random rand = new Random();
                SetPowerupTimer(rand.Next(0, maxPowerupDelay));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets powerup counter
        /// </summary>
        /// <param name="count"></param>
        public void SetPowerupCounter(int count)
        {
            this.powerupCounter = count;
        }

        /// <summary>
        /// Sets the max delay before a powerup spawns.
        /// </summary>
        /// <param name="max">Time until powerup spawns</param>
        public void SetMaxPowerupDelay(int max)
        {
            this.maxPowerupDelay = max;
        }

        /// <summary>
        /// returns powerup counter (powerup ID)
        /// </summary>
        /// <returns></returns>
        public int GetPowerupCounter()
        {
            return this.powerupCounter;
        }
    }
}
