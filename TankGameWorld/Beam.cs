//////////////////////////////////////////////
///FileName: Beam.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/16/2020
///Description: Object representing Beam within game world 
/////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankGameWorld
{
    /// <summary>
    /// Class represents a game Powerup
    /// Has JSON serialization property names for server communcation 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        private int ID; //Unique beam ID given by server 

        [JsonProperty(PropertyName = "org")]
        private Vector2D origin; //Origin of beam shot 

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction; //direction of beam shot 

        [JsonProperty(PropertyName = "owner")]
        private int owner; //Tank ID of the tank who shot the beam 

        private int drawTimer = 40; // Timer to be decremented when beam is drawn, when timer hits 0 the beam dissapears 

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Beam()
        {

        }

        /// <summary>
        /// Gets the beam's ID.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Sets the beam's ID.
        /// </summary>
        /// <param name="ID"></param>
        public void SetID(int ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// Gets the origin of the beam.
        /// </summary>
        /// <returns>Origin</returns>
        public Vector2D GetOrigin()
        {
            return this.origin;
        }

        /// <summary>
        /// Sets the origin of the beam.
        /// </summary>
        /// <param name="vector">Origin</param>
        public void SetOrigin(Vector2D vector)
        {
            this.origin = vector;
        }

        /// <summary>
        /// Gets the direction the beam is going.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetDirection()
        {
            return this.direction;
        }

        /// <summary>
        /// Sets the direction the beam is going.
        /// </summary>
        /// <param name="vector">Direction</param>
        public void SetDirection(Vector2D vector)
        {
            this.direction = vector;
        }

        /// <summary>
        /// Gets the owner of this beam.
        /// </summary>
        /// <returns></returns>
        public int GetOwner()
        {
            return owner;
        }

        /// <summary>
        /// Sets the owner of this beam.
        /// </summary>
        /// <param name="ID">Owner ID</param>
        public void SetOwner(int ID)
        {
            this.owner = ID;
        }

        /// <summary>
        /// Returns int value within beam drawing timer 
        /// Decrements value each time this method is called, so it should be called once a frame 
        /// </summary>
        public int GetTimer()
        {
            return --drawTimer;
        }
    }
}
