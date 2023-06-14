//////////////////////////////////////////////
///FileName: Wall.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/16/2020
///Description: Object representing Wall within game world 
/////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankGameWorld
{
    /// <summary>
    /// Class represents a game Wall
    /// Has JSON serialization property names for server communcation 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        private int ID; //unique wall ID

        [JsonProperty(PropertyName = "p1")]
        private Vector2D p1; //end point of wall 

        [JsonProperty(PropertyName = "p2")]
        private Vector2D p2; //end point of wall 

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Wall()
        {

        }
        /// <summary>
        /// Returns Wall ID
        /// </summary>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        /// Sets Wall ID
        /// </summary>
        /// <param name="id">Wall ID</param>
        public void SetID(int id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Returns Vector2D of the first wall endpoint
        /// </summary>
        public Vector2D GetLocationP1()
        {
            return p1;
        }

        /// <summary>
        /// Returns Vector2D of the second wall endpoint
        /// </summary>
        public Vector2D GetLocationP2()
        {
            return p2;
        }

        /// <summary>
        /// Sets the P1 Vector
        /// </summary>
        public void SetLocationP1(Vector2D vector)
        {
            this.p1 = vector;
        }

        /// <summary>
        /// Sets the P2 Vector
        /// </summary>
        public void SetLocationP2(Vector2D vector)
        {
            this.p2 = vector;
        }
    }
}
