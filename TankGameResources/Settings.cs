//////////////////////////////////////////////
///FileName: Settings.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/30/2020
///Description: Represents the different settings for the server.
/////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Settings
    {
        // Holds the maximum HP of a tank
        public static int maxHP;

        // Holds how fast a projectile travels per frame
        public static int projSpeed;

        // Holds how fast a tank travels per frame
        public static double tankSpeed;

        // Holds the size of a tank
        public static int tankSize;

        // Holds the size of a wall
        public static int wallSize;

        // Holds the number of maximum powerups
        public static int maxPowerups;

        // Holds the maximum amount of time before a new powerup can spawn
        public static int maxPowerupDelay;

        // Holds the size of the world/universe
        public static int universeSize;

        // Holds the amount of time the world is updated per frame
        public static int timePerFrame;

        // Holds the time between a tank can fire
        public static int projFireDelay;

        // Holds the time before a tank respawns
        public static int respawnDelay;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Settings()
        {

        }

        /// <summary>
        /// Gets the maximum HP of a tank.
        /// </summary>
        /// <returns>Max HP</returns>
        public static int GetMaxHP()
        {
            return maxHP;
        }

        /// <summary>
        /// Sets the maximum HP of a tank.
        /// </summary>
        /// <param name="max">Max HP</param>
        public void SetMaxHP(int max)
        {
            maxHP = max;
        }

        /// <summary>
        /// Gets the speed at which projectiles travel.
        /// </summary>
        /// <returns>Speed/units per frame</returns>
        public int GetProjSpeed()
        {
            return projSpeed;
        }

        /// <summary>
        /// Sets the speed at which projectiles travel.
        /// </summary>
        /// <param name="speed">Speed/units per frame</param>
        public void SetProjSpeed(int speed)
        {
           projSpeed = speed;
        }

        /// <summary>
        /// Gets the speed at which a tank travels.
        /// </summary>
        /// <returns>Speed/units per frame</returns>
        public double GetTankSpeed()
        {
            return tankSpeed;
        }

        /// <summary>
        /// Sets the speed at which a tank travels.
        /// </summary>
        /// <param name="speed">Speed/units per frame</param>
        public void SetTankSpeed(int speed)
        {
            tankSpeed = speed;
        }

        /// <summary>
        /// Gets the size of a tank.
        /// </summary>
        /// <returns>Size of tank</returns>
        public double GetTankSize()
        {
            return tankSize;
        }

        /// <summary>
        /// Sets the size of a tank.
        /// </summary>
        /// <param name="size">Size of a tank</param>
        public void SetTankSize(int size)
        {
            tankSize = size;
        }

        /// <summary>
        /// Gets the size of a wall.
        /// </summary>
        /// <returns>Size of a wall</returns>
        public double GetWallSize()
        {
            return wallSize;
        }

        /// <summary>
        /// Sets the size of a wall.
        /// </summary>
        /// <param name="size">Size of a wall</param>
        public void SetWallSize(int size)
        {
            wallSize = size;
        }

        /// <summary>
        /// Gets the maximum amount of powerups spawned at any one time.
        /// </summary>
        /// <returns>Maximum number of powerups</returns>
        public static double GetMaxPowerups()
        {
            return maxPowerups;
        }

        /// <summary>
        /// Sets the maximum amount of powerups spawned at any one time.
        /// </summary>
        /// <param name="max">Maximum number of powerups </param>
        public void SetMaxPowerups(int max)
        {
            maxPowerups = max;
        }

        /// <summary>
        /// Gets the maximum amount of time before a new powerup spawns.
        /// </summary>
        /// <returns>Maximum amount of time in frames</returns>
        public double GetMaxPowerupsDelay()
        {
            return maxPowerupDelay;
        }

        /// <summary>
        /// Sets the maximum amount of time before a new powerup spawns.
        /// </summary>
        /// <param name="delay">Maximum amount of time in frames</param>
        public void SetMaxPowerupDelay(int delay)
        {
            maxPowerupDelay = delay;
        }

        /// <summary>
        /// Gets the universe/world size.
        /// </summary>
        /// <returns>Size in units</returns>
        public double GetUniverseSize()
        {
            return universeSize;
        }

        /// <summary>
        /// Sets the universe/world size.
        /// </summary>
        /// <param name="size">Size in units</param>
        public void SetUniverseSize(int size)
        {
            universeSize = size;
        }

        /// <summary>
        /// Gets the number of times the server updates the world per frame.
        /// </summary>
        /// <returns>Number of updates per frame</returns>
        public double GetTimePerFrame()
        {
            return timePerFrame;
        }

        /// <summary>
        /// Sets the number of times the serve rupdates the world per frame.
        /// </summary>
        /// <param name="time">Number of updates per frame</param>
        public void SetTimePerFrame(int time)
        {
            timePerFrame = time;
        }

        /// <summary>
        /// Gets the delay between when a tank fires, and can fire again.
        /// </summary>
        /// <returns>Delay in frames</returns>
        public double GetProjFireDelay()
        {
            return projFireDelay;
        }

        /// <summary>
        /// Sets the delay between when a tank fires, and can fire again.
        /// </summary>
        /// <param name="delay">Delay in frames</param>
        public void SetProjFireDelay(int delay)
        {
            projFireDelay = delay;
        }

        /// <summary>
        /// Gets the time between when a tank dies, and will respawn.
        /// </summary>
        /// <returns>Time in frames</returns>
        public static double GetRespawnDelay()
        {
            return respawnDelay;
        }
        /// <summary>
        /// Sets the time between when a tank dies, and will respawn.
        /// </summary>
        /// <param name="delay">Time in frames</param>
        public void SetRespawnDelay(int delay)
        {
            respawnDelay = delay;
        }

    }
}
