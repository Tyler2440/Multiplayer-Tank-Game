//////////////////////////////////////////////
///FileName: Tank.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Represents a Tank and each of its characteristics.
/////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using TankWars;

namespace TankGameWorld
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        // Holds the tank's ID
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        // Holds the location of the tank
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Holds the orientation of the tank
        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        // Holds the orientation the turret is facing
        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming;

        // Holds the name of the tank
        [JsonProperty(PropertyName = "name")]
        private string name;

        // Holds the HP of the tank
        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = Constants.MaxHP;

        // Holds the score of the tank
        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        // Holds whether this tank has "died"
        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        // Holds whether the tank/player has disconnected
        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        // Holds whether the tank/player has just joined
        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        // Holds the time the explosion effect is active
        private int explosionTimer = 5;

        // Holds how long until tank can shoot
        private int firingDelay = 0;

        // Holds whether tank has an unused powerup
        private bool hasPowerup = false;

        // Holds whether the tank can respawn if dead
        private int respawnDelay = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Tank()
        {

        }

        /// <summary>
        /// Gets whether the tank has "died".
        /// </summary>
        /// <param name="died">Whether the tank has "died"</param>
        public bool GetDied()
        {
            return this.died;
        }

        /// <summary>
        /// Sets whether the tank has "died".
        /// </summary>
        /// <param name="died">Whether the tank has "died"</param>
        public void SetDied(bool died)
        {
            this.died = died;

            if(died)
            {
                this.respawnDelay = (int)Server.Settings.GetRespawnDelay();
            }
        }

        /// <summary>
        /// Gets if the tank has disconnected from server.
        /// </summary>
        /// <returns>True - tank has disconnected, False - tank has not disconnected</returns>
        public bool Disconnected()
        {
            return this.disconnected;
        }

        /// <summary>
        /// Sets if the tank has disconnected from the server.
        /// </summary>
        /// <param name="disconnected">True - tank has disconnected, False - tank has not disconnected</param>
        public void SetDisconnected(bool disconnected)
        {
            this.disconnected = disconnected;
        }

        /// <summary>
        /// Sets the fire delay.
        /// </summary>
        /// <param name="delay">Fire delay</param>
        public void SetFireDelay(int delay)
        {
            firingDelay = delay;
        }

        /// <summary>
        /// Gets the fire delay.
        /// </summary>
        public int GetFireDelay()
        {
            return this.firingDelay;
        }

        /// <summary>
        /// Gets whether tank has a powerup.
        /// </summary>
        /// <returns>True - has powerup, False - does not have powerup</returns>
        public bool HasPowerup()
        {
            return this.hasPowerup;
        }

        /// <summary>
        /// Sets whether the tank a powerup.
        /// </summary>
        public void SetsHasPowerup(bool hasPowerup)
        {
            this.hasPowerup = hasPowerup;
        }

        /// <summary>
        /// Gets the HP of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetHitPoints()
        {
            return hitPoints;
        }

        /// <summary>
        /// Deduct one health from the tank's hit points.
        /// </summary>
        public void DeductHealth()
        {
            this.hitPoints -= 1;
        }

        /// <summary>
        /// Sets the health of the tank.
        /// </summary>
        /// <param name="health">Amount of health</param>
        public void SetHealth(int health)
        {
            this.hitPoints = health;
        }

        /// <summary>
        /// Gets the location of the tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return this.location;
        }

        /// <summary>
        /// Sets the location of the tank.
        /// </summary>
        /// <param name="location">Location of tank</param>
        public void SetLocation(Vector2D location)
        {
            this.location = location;
        }


        /// <summary>
        /// Gets the orientation of the tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrientation()
        {
            return this.orientation;
        }

        /// <summary>
        /// Sets the orientation of the tank.
        /// </summary>
        public void SetOrientation(Vector2D vector)
        {
            this.orientation = vector;
        }

        /// <summary>
        /// Gets the ID of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Sets the ID of the tank.
        /// </summary>
        /// <param id="id">ID of tank</param>
        public void SetID(int id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Gets the orientation of the turret/where the player is aiming.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetAiming()
        {
            return this.aiming;
        }

        /// <summary>
        /// Sets the orientation/direction of the turret.
        /// </summary>
        /// <param name="direction">Direction to set the turret</param>
        public void SetTurretDirection(Vector2D direction)
        {
            this.aiming = direction;
        }

        /// <summary>
        /// Gets the name of the tank.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.name;
        }

        /// <summary>
        /// Sets the name of the tank.
        /// </summary>
        /// <param name="name">Name of tank</param>
        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the score of the tank.
        /// </summary>
        /// <returns>Score</returns>
        public string GetScore()
        {
            return score.ToString();
        }

        /// <summary>
        /// Sets the score of the tank.
        /// </summary>
        /// <param name="score">Score</param>
        public void SetScore(int score)
        {
            this.score = score;
        }

        /// <summary>
        /// Gets the time between when a tank dies, and will respawn.
        /// </summary>
        /// <returns>Time in frames</returns>
        public int GetRespawnDelay()
        {
            // If the respawn timer is up, simply return 0
            if (this.respawnDelay == 0)
                return this.respawnDelay;

            // If the timer is not over, decrement the delay, then return
            return --this.respawnDelay;
        }

        /// <summary>
        /// Sets the time between when a tank dies, and will respawn.
        /// </summary>
        /// <param name="delay">Time in frames</param>
        public void SetRespawnDelay(int delay)
        {
            this.respawnDelay = delay;
        }

        /// <summary>
        /// Gets the timer for how long the explosion effect is active for.
        /// </summary>
        /// <returns></returns>
        public int GetTimer()
        {
            // Decrements the timer, then returns
            return --explosionTimer;
        }

    }
}
