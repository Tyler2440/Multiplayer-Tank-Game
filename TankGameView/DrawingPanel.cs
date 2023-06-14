//////////////////////////////////////////////
///FileName: DrawingPanel.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Handles updating and drawing images/shapes for the TankGameView
/////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankGameWorld;
using System.IO;

namespace TankGameView
{
    
    public class DrawingPanel : Panel
    {
        private World theWorld;
        private BufferedGraphics bufferedGraphics;

        /// <summary>
        /// Constructor sets up World instance variable and Buffer settings 
        /// </summary>
        /// <param name="w"></param>
        public DrawingPanel(World w)
        {
            this.DoubleBuffered = true;
            theWorld = w;
            using (Graphics graphics = CreateGraphics())
            {
                bufferedGraphics = BufferedGraphicsManager.Current.Allocate(graphics, new Rectangle(0, 0, Constants.WorldSize, Constants.WorldSize));
            }
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for the player(Tank).
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PlayerDrawer(object o, PaintEventArgs e)
        {
            // Gets the tank from object o
            Tank t = o as Tank;

            // Create a string to hold the color of the tank
            string colorString = "";

            // Gets the color of the turret based on the ID of the tank
            GetColor(t.GetID(), out colorString);

            // Draw the tank from the specified path/size/offset
            DrawImage(colorString + "Tank", Constants.TankSize, Constants.TankSize, -30, e);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for the tank's turret.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            // Gets the tank from object o
            Tank t = o as Tank;

            // Create a string to hold the color of the turret
            string colorString = "";

            // Gets the color of the turret based on the ID of the tank
            GetColor(t.GetID(), out colorString);

            // Draw the turret from the specified path/size/offset
            DrawImage(colorString + "Turret", Constants.TurretSize, Constants.TurretSize, -25, e);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for the player's name.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PlayerNameDrawer(object o, PaintEventArgs e)
        {
            // Gets the tank from object o
            Tank t = o as Tank;
         
            // Draws the name and score of the player with the given font/PointF offset from the tank
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.White))
            {
                Font font = new Font("TimesRoman", 10);
                PointF point = new PointF(-30, 35);
                e.Graphics.DrawString(t.GetName() + ": " + t.GetScore(), font, brush, point);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for the player's health.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PlayerHealthDrawer(object o, PaintEventArgs e)
        {
            // Gets the tank from object o
            Tank t = o as Tank;

            // If the tank has 3 hitpoints left, draw a green bar
            if (t.GetHitPoints() == 3)
            {
                // Draws the green bar for the "full" health bar with the specified height/width/offsetX/offsetY
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Green))
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = 8;
                    rect.Width = 50;
                    rect.X = -25;
                    rect.Y = -45;
                    e.Graphics.FillRectangle(brush, rect);
                }
            }

            // If the tank has 2 hitpoints left, draw a yellow bar
            else if (t.GetHitPoints() == 2)
            {
                // Draws the yellow bar for the "medium" health bar with the specified height/width/offsetX/offsetY
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Yellow))
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = 8;
                    rect.Width = 33;
                    rect.X = -25;
                    rect.Y = -45;
                    e.Graphics.FillRectangle(brush, rect);
                }
            }

            // If the tank has 1 hitpoint left, draw a red bar
            else if(t.GetHitPoints() == 1)
            {
                // Draws the red bar for the "low" health bar with the specified height/width/offsetX/offsetY
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Red))
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = 8;
                    rect.Width = 16;
                    rect.X = -25;
                    rect.Y = -45;
                    e.Graphics.FillRectangle(brush, rect);
                }
            }

            // Draws the black outline for the health bar with the specified height/width/offsetX/offsetY
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Black))
            {
                Pen pen = new Pen(brush);
                Rectangle rect = new Rectangle();
                rect.Height = 8;
                rect.Width = 50;
                rect.X = -25;
                rect.Y = -45;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        /// <summary>
        /// Uses tank ID to determine color of player. Players can be one of 8 colors.
        /// </summary>
        /// <param name="TankID"></param>
        /// <returns> string of player color </returns>
        private Color GetColor (int TankID, out string colorString)
        {
            // Uses the ID of the tank mod 8 to determine which color the tank should be. Sets the colorString and returns
            // a Color based on the appropriate color 
            switch (TankID % 8)
            {
                case 0:
                    {
                        colorString = "Blue";
                        return System.Drawing.Color.Blue;
                    }
                case 1:
                    {
                        colorString = "Dark";
                        return System.Drawing.Color.Purple;
                    }
                case 2:
                    {
                        colorString = "Green";
                        return System.Drawing.Color.Green;
                    }
                case 3:
                    {
                        colorString = "LightGreen";
                        return System.Drawing.Color.LightGreen;
                    }
                case 4:
                    {
                        colorString = "Orange";
                        return System.Drawing.Color.Orange;
                    }
                case 5:
                    {
                        colorString = "Purple";
                        return System.Drawing.Color.Pink;
                    }
                case 6:
                    {
                        colorString = "Red";
                        return System.Drawing.Color.Red;
                    }
                case 7:
                    {
                        colorString = "Yellow";
                        return System.Drawing.Color.Yellow;
                    }
                default:
                    {
                        colorString = "Red";
                        return System.Drawing.Color.Red;
                    }
            }
        }
        
        /// <summary>
        /// Uses wall object o to draw wall tile.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            // Gets the wall from object o
            Wall wall = o as Wall;

            // Draw the wall from the specified path, size, and offset
            DrawImage("WallSprite", Constants.WallSize, Constants.WallSize, -25, e);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for powerups.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            // Gets the powerup from object o
            Powerup p = o as Powerup;

            int size = 20;
            
            // Draw the powerup
            e.Graphics.DrawImage(GameImages.GetImage("Powerup"), 0, 0, size, size);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specificaly for the player's death explosion.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            int size = 50;

            // Draw the explosion
            e.Graphics.DrawImage(GameImages.GetImage("Boom"), 0, 0, size, size);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for a projectile.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            // Gets the projectile from object o
            Projectile p = o as Projectile;

            // Create a string to hold the color of the shot
            string shotColor = "";

            // Gets the color of the shot based on the color of the tank
            switch (p.GetOwner() % 8)
            {
                case 0:
                    {
                        shotColor =  "Blue";
                        break;
                    }
                case 1:
                    {
                        // No dark shot?
                        shotColor = "Brown";
                        break;
                    }
                case 2:
                    {
                        shotColor = "Green";
                        break;
                    }
                case 3:
                    {
                        shotColor = "Grey";
                        break;
                    }
                case 4:
                    {
                        // No orange shot?
                        //shotColor = "Orange";
                        shotColor = "White";
                        break;
                    }
                case 5:
                    {
                        // No purple shot?
                        //shotColor = "Purple";
                        shotColor = "Violet";
                        break;
                    }
                case 6:
                    {
                        shotColor = "Red";
                        break;
                    }
                case 7:
                    {
                        shotColor = "Yellow";
                        break;
                    }
                default:
                    {
                        shotColor = "Red";
                        break;
                    }
            }

            // Draw the projectile
            e.Graphics.DrawImage(GameImages.GetImage(shotColor + "Shot"), -(Constants.ProjectileSize/2), -(Constants.ProjectileSize / 2), Constants.ProjectileSize, Constants.ProjectileSize);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform, specifically for a beam attack.
        /// After performing the necessary transformation (translate/rotate),
        /// DrawObjectWithTransform will invoke this method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            // Gets the beam from object o
            Beam b = o as Beam;

            // Creates a dummy string to pass into GetColor
            string dummy = "";

            // Gets the color the beam needs to be drawn in based on which color tank shot it
            Color color = GetColor(b.GetOwner(), out dummy);

            // Create a brush from the specified color
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(color))
            {
                // Create a pen using the brush
                Pen pen = new Pen(brush);
                pen.Width = 10;

                // Create two points, one representing the tank(where the beam was shot) and where the beam should be drawn to
                Point p1 = new Point(0, -Constants.WorldSize * 2);
                Point p2 = new Point(0, 0);

                // Draw a line(beam) according to the color of the tank, from the tank to a specific point
                e.Graphics.DrawLine(pen, p1, p2);
            }
        }

        /// <summary>
        /// Helper method to draw image on world given it's image name, width, height, and offset.
        /// </summary>
        /// <param name="image"> string of image file name </param>
        /// <param name="width"> width of image to be drawn </param>
        /// <param name="height"> height of image to be drawn </param>
        /// <param name="offset"> image offset </param>
        /// <param name="e"> The PaintEventArgs to access the graphics </param>
        public void DrawImage(string image, int width, int height, int offset, PaintEventArgs e)
        {
            // Create coordinates offset from the upper-left corner of image.
            float x = (float)offset;
            float y = (float)offset;
           
            // Draw image to screen.
            this.Invalidate();
            e.Graphics.DrawImage(GameImages.GetImage(image), x, y, width, height);           
        }

        /// <summary>
        /// Draws the background of game world.
        /// </summary>
        /// <param name="e"></param>
        public void DrawBackground(PaintEventArgs e)
        {
            // Sets different settings for e to increase performance
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            // Draw image to screen.
            this.Invalidate();           
            e.Graphics.DrawImage(GameImages.GetImage("Background"), 0, 0, Constants.WorldSize, Constants.WorldSize);
        }

        /// <summary>
        /// Method invoked every frame to re-draw all world objects in their current state.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Clears the world to rid of any previous drawings.
            e.Graphics.Clear(Color.Black);

            lock (theWorld.Tanks)
            {
                // Draw the players           
                if (theWorld.Tanks.Count > 0)
                {
                    // Gets the location of the tank
                    double playerX = theWorld.Tanks[theWorld.tankID].GetLocation().GetX();
                    double playerY = theWorld.Tanks[theWorld.tankID].GetLocation().GetY();

                    // Calculate view/world size ratio
                    double ratio = (double)Constants.ViewSize / (double)Constants.WorldSize;
                    int halfSizeScaled = (int)(Constants.WorldSize / 2.0 * ratio);

                    // Normalizes and sets the position of the view to follow the tank
                    double inverseTranslateX = -WorldSpaceToImageSpace(Constants.WorldSize, playerX) + halfSizeScaled;
                    double inverseTranslateY = -WorldSpaceToImageSpace(Constants.WorldSize, playerY) + halfSizeScaled;

                    e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);
                }

                if(theWorld.Tanks.Count > 0)
                    DrawBackground(e);

                // Draw each tank and its turret, health, name, and score
                foreach (Tank play in theWorld.Tanks.Values)
                {
                    if (play.GetHitPoints() > 0)
                    {
                        DrawObjectWithTransform(e, play, Constants.WorldSize, play.GetLocation().GetX(), play.GetLocation().GetY(), play.GetOrientation().ToAngle(), PlayerDrawer);
                        DrawObjectWithTransform(e, play, Constants.WorldSize, play.GetLocation().GetX(), play.GetLocation().GetY(), play.GetAiming().ToAngle(), TurretDrawer);
                        DrawObjectWithTransform(e, play, Constants.WorldSize, play.GetLocation().GetX(), play.GetLocation().GetY(), 0, PlayerNameDrawer);
                        DrawObjectWithTransform(e, play, Constants.WorldSize, play.GetLocation().GetX(), play.GetLocation().GetY(), 0, PlayerHealthDrawer);
                    }

                    else
                    {
                        // If the player's health is 0, draw the explosion effect
                        if (play.GetTimer() > 0)
                            DrawObjectWithTransform(e, play, Constants.WorldSize, play.GetLocation().GetX(), play.GetLocation().GetY(), 0, ExplosionDrawer);
                    }
                }
            }

            lock (theWorld.Powerups)
            {
                // Draw the powerups
                foreach (Powerup pow in theWorld.Powerups.Values)
                {
                    // If the powerup is not "dead", draw it
                    if (!pow.Died)
                        DrawObjectWithTransform(e, pow, Constants.WorldSize, pow.GetLocation().GetX(), pow.GetLocation().GetY(), 0, PowerupDrawer);
                }
            }

            lock (theWorld.Walls)
            {
                // Draw the walls. The logic handles the walls having to be drawn top -> bottom, bottom -> top, right -> left, or left -> right.
                foreach (Wall wall in theWorld.Walls.Values)
                {
                    // Gets the change in x or y between the two points where the wall needs to be drawn
                    int wallDistanceX = (int)(wall.GetLocationP1().GetX() - wall.GetLocationP2().GetX());
                    int wallDistanceY = (int)(wall.GetLocationP1().GetY() - wall.GetLocationP2().GetY());

                    // Draws a wall from left -> right
                    if (wallDistanceY == 0 && wallDistanceX < 0)
                    {
                        for (int i = (int)wall.GetLocationP1().GetX(); i <= (int)wall.GetLocationP2().GetX(); i += 50)
                        {
                            DrawObjectWithTransform(e, wall, Constants.WorldSize, i, wall.GetLocationP1().GetY(), 0, WallDrawer);
                        }
                    }

                    // Draws a wall from top -> bottom
                    else if (wallDistanceX == 0 && wallDistanceY < 0)
                    {
                        for (int i = (int)wall.GetLocationP1().GetY(); i <= (int)wall.GetLocationP2().GetY(); i += 50)
                        {
                            DrawObjectWithTransform(e, wall, Constants.WorldSize, wall.GetLocationP1().GetX(), i, 0, WallDrawer);
                        }
                    }

                    // Draws a wall from right -> left
                    else if (wallDistanceY == 0 && wallDistanceX > 0)
                    {
                        for (int i = (int)wall.GetLocationP1().GetX(); i >= (int)wall.GetLocationP2().GetX(); i -= 50)
                        {
                            DrawObjectWithTransform(e, wall, Constants.WorldSize, i, wall.GetLocationP1().GetY(), 0, WallDrawer);
                        }
                    }

                    // Draws a wall from bottom -> top
                    else if (wallDistanceX == 0 && wallDistanceY > 0)
                    {
                        for (int i = (int)wall.GetLocationP1().GetY(); i >= (int)wall.GetLocationP2().GetY(); i -= 50)
                        {
                            DrawObjectWithTransform(e, wall, Constants.WorldSize, wall.GetLocationP1().GetX(), i, 0, WallDrawer);
                        }
                    }
                }
            }

            lock(theWorld.Projectiles)
            {
                // Draws any projectiles that are not "dead"
                foreach (Projectile proj in theWorld.Projectiles.Values)
                {
                    // If the projectile is not "dead", draw it
                    if (!proj.Died)
                        DrawObjectWithTransform(e, proj, Constants.WorldSize, proj.GetLocation().GetX(), proj.GetLocation().GetY(), proj.GetDirection().ToAngle(), ProjectileDrawer);

                    // Otherwise, remove it from theWorld's list of projectiles
                    else
                        theWorld.Projectiles.Remove(proj.GetID());
                }
            }

            lock (theWorld.Beams)
            {
                // Draws any beams
                foreach (Beam beam in theWorld.Beams.Values)
                {
                    // Draw the beam, and each time it is drawn, check if the beam's draw timer is up. If so, remove it from theWorld's list of beams
                    DrawObjectWithTransform(e, beam, Constants.WorldSize, theWorld.Tanks[beam.GetOwner()].GetLocation().GetX(), theWorld.Tanks[beam.GetOwner()].GetLocation().GetY(), beam.GetDirection().ToAngle(), BeamDrawer);
                    if(beam.GetTimer() == 0)
                        theWorld.Beams.Remove(beam.GetID());
                }
            }

            // Do anything that Panel (from which we inherit) needs to do    
            base.OnPaint(e);
        }

        /// <summary>
        /// Class hold Image objects used to draw objects in the World 
        /// </summary>
        public class GameImages
        {
            // Holds all images that will be used in TankGame
            public static Dictionary<string, Image> images = new Dictionary<string, Image>();

            /// <summary>
            /// Returns the image object given the image name.
            /// If image is not already in dictionary, image is added to images and then returned.
            /// </summary>
            /// <param name="image"> Image file name </param>
            /// <returns> Image object holding desired object sprite </returns>
            public static Image GetImage(string image)
            {
                // If images already contains the image, return it
                if (images.ContainsKey(image))
                    return images[image];

                // Otherwise, add the image from its file path, add it to images, then return it
                else
                {
                    string filePath = "..\\..\\..\\..\\TankGameResources\\Images\\";
                    images.Add(image, Image.FromFile(filePath + image + ".png"));
                    return images[image];
                }
            }
        }
    }
}