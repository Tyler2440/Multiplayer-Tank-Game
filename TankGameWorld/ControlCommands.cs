//////////////////////////////////////////////
///FileName: ControlCommands.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/16/2020
///Description: Object representing user input data to be sent to the server
/////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankGameWorld
{
    /// <summary>
    /// Class represents the user input controls
    /// Has JSON serialization property names for server communcation 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        [JsonProperty(PropertyName = "moving")]
        private string moving; //string holding movement command 

        [JsonProperty(PropertyName = "fire")]
        private string fire; //string holding firing command 

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D orientation = new Vector2D(0,0); // aiming direction of client tank 

        public ControlCommands()
        {

        }

        /// <summary>
        /// Sets the moving command string to one of 5 commands : 
        /// "up", "left", "down", "right", "none"
        /// </summary>
        public void SetMoving(string moving)
        {
            this.moving = moving;
        }

        /// <summary>
        /// Sets firing command string to one of 3 commands : 
        /// "main", "alt", "none"
        /// </summary>
        public void SetFire(string fire)
        {
            this.fire = fire;
        }

        /// <summary>
        /// Sets Vector2D aiming direction of turret 
        /// </summary>
        public void SetOrientation(Vector2D tdir)
        {
            this.orientation = tdir;
        }

        public string GetMoving()
        {
            return this.moving;
        }

        public string GetFire()
        {
            return this.fire;
        }

        public Vector2D GetOrientation()
        {
            return this.orientation;
        }
    }
}
