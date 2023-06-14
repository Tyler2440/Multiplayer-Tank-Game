//////////////////////////////////////////////
///FileName: Constants.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Holds the Constant values used throughout TankGame
/////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

namespace TankGameWorld
{
    public class Constants
    {
        // Holds the maximum HP of a tank
        public static readonly int MaxHP = 3;

        // Used in the WorldSize property to hold the world size
        private static int size = 2000;

        // Holds the view size of the drawingPanel
        public static readonly int ViewSize = 900;

        // Holds the size of a tank
        public static readonly int TankSize = 60;

        // Holds the size of a wall
        public static readonly int WallSize = 50;

        // Holds the size of a powerup
        public static readonly int PowerupSize = 30;

        // Holds the size of a turret
        public static readonly int TurretSize = 50;

        // Holds the size of a projectile
        public static readonly int ProjectileSize = 30;

        /// <summary>
        /// Property for WorldSize
        /// </summary>
        public static int WorldSize
        {
            get { return size; }
            
            // Set size to the given value
            set { size = value; }
        }
    }
}
