//////////////////////////////////////////////
///FileName: Projectile.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Represents a projectile and each of its characteristics.
/////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankGameWorld
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        // Holds the projectile's ID
        [JsonProperty(PropertyName = "proj")]
        private int ID;

        // Holds the projectile's current location
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Holds which direction the projectile is going
        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        // Holds whether the projectile "died"
        [JsonProperty(PropertyName = "died")]
        private bool died;

        // Holds the owner/which tank shot this projectile
        [JsonProperty(PropertyName = "owner")]
        private int owner;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Projectile()
        {

        }

        /// <summary>
        /// Property for whether the projectile has "died"
        /// </summary>
        public bool Died
        {
            get { return died; }
            set { died = value;  }
        }

        /// <summary>
        /// Gets the projectile's ID.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Sets the projectile's ID.
        /// </summary>
        /// <param name="ID"></param>
        public void SetID(int ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// Gets the projectile's location.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return this.location;
        }

        /// <summary>
        /// Sets the projectile's location.
        /// </summary>
        /// <param name="vector">Location</param>
        public void SetLocation(Vector2D vector)
        {
            this.location = vector;
        }

        /// <summary>
        /// Gets the direction the projectile is going.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetDirection()
        {
            return this.direction;
        }

        /// <summary>
        /// Sets the direction the projectile is going.
        /// </summary>
        /// <param name="vector">Direction</param>
        public void SetDirection(Vector2D vector)
        {
            this.direction = vector;
        }

        /// <summary>
        /// Gets the owner of this projectile.
        /// </summary>
        /// <returns></returns>
        public int GetOwner()
        {
            return owner;
        }

        /// <summary>
        /// Sets the owner of this projectile.
        /// </summary>
        /// <param name="ID">Owner ID</param>
        public void SetOwner(int ID)
        {
            this.owner = ID;
        }

    }
}
