//////////////////////////////////////////////
///FileName: Powerup.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/16/2020
///Description: Object representing Powerup within game world 
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
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        private int ID; //unique powerup ID

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location; //location of powerup within world 

        [JsonProperty(PropertyName = "died")]
        private bool died; //holds whether powerpup is still "alive" or "dead"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Powerup()
        {

        }

        /// <summary>
        /// Returns true if powerup is dead, false if it is still active 
        /// </summary>
        public bool Died
        {
            get { return died; }
            set { died = value; }
        }

        /// <summary>
        /// Returns Vector2D location of powerup 
        /// </summary>
        public Vector2D GetLocation()
        {
            return this.location;
        }

        /// <summary>
        /// Returns unique powerup ID 
        /// </summary>
        public int GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Sets ID of powerup
        /// </summary>
        /// <param name="id"></param>
        public void SetID(int id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Sets location of powerup
        /// </summary>
        /// <param name="loc"></param>
        public void SetLocation(Vector2D loc)
        {
            this.location = loc;
        }
    }
}
