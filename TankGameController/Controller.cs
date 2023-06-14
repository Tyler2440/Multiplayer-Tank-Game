//////////////////////////////////////////////
///FileName: Controller.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: Handles networking and game logic
/////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankGameWorld;
using TankWars;

namespace TankGameController
{
    /// <summary>
    /// Handles logic and networking protocol for TankWars Game
    /// </summary>
    public class Controller
    {
        private static SocketState theServer = null; //socket for connection to server 
        private static string name = ""; //player name of local client 

        // Event to update wiew with new info from server
        public delegate void DataHandler(World theWorld);
        public event DataHandler DataEvent;

        // Event to update view of network errors
        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        public World theWorld; //World object 
        ControlCommands cmds = new ControlCommands(); //Holds current user input data to be sent to server

        //holds key and mouse buttons currently being held down 
        public List<string> keysDown = new List<string>();
        public List<string> mouseDown = new List<string>();

        /// <summary>
        /// Sets up initial connection with server 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="playerName"></param>
        /// <param name="theWorld"></param>
        public void Connect(string server, string playerName, World theWorld)
        {
            name = playerName; 
            Networking.ConnectToServer(OnConnect, server, 11000);
            this.theWorld = theWorld;
        }

        /// <summary>
        /// On Network Action method to be called once connection is set up. 
        /// Method sends player name to server and initiates GetData event loop 
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error while connecting."); //update view of error
                return;
            }

            theServer = state;
            Networking.Send(state.TheSocket, name + "\n"); //update server with player name 
            state.OnNetworkAction = OnHandshake; //set up OnNetworkAction for handshake protocol 

            Networking.GetData(state); //initiate receive network loop 
        }

        /// <summary>
        /// On Network Action method to be called with inital server messages arrive: player ID and world size 
        /// If error occured during message retrieval, the view is notified. 
        /// GetData event loop is continued with OnReceive as the new OnNetworkAction delegate
        /// </summary>
        /// <param name="state"></param>
        private void OnHandshake(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error while retrieving setup!"); //update view of error 
                return;
            }

            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Set tank ID
            theWorld.tankID = int.Parse(parts[0]);

            // Set up world
            Constants.WorldSize = int.Parse(parts[1]);

            state.RemoveData(0, parts[0].Length + parts[1].Length); // remove first two (already handled) lines from server string builder 
            DataEvent(theWorld); //update view 
            state.OnNetworkAction = OnReceive; //change OnNetworkAction for normal JSON server communications 
            Networking.GetData(state);
        }

        /// <summary>
        /// On Network Action method to be called when message has been receieved. Method calls ProcessMessages and continues GetData event loop. 
        /// If error occured, view is updated. 
        /// </summary>
        /// <param name="state"></param>
        private void OnReceive(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error while receiving data!"); //update view of error 
                return;
            }

            ProcessMessages(state); //convert JSON data to world objects

            Networking.GetData(state);
        }

        /// <summary>
        /// Parses JSON data and updates the world and notifies view of changes
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])"); //splits string by new lines 

            foreach (string data in parts)
            {
                if (data.Length == 0)
                    continue;

                if (data[data.Length - 1] != '\n') //do not process line if it is not complete 
                    break;

                JObject obj = JObject.Parse(data);

                // update tank values
                JToken token = obj["tank"];
                if (token != null)
                {
                    Tank tank = JsonConvert.DeserializeObject<Tank>(data);
                    lock (theWorld.Tanks)
                    {
                        if (theWorld.Tanks.ContainsKey(tank.GetID()))
                            theWorld.Tanks[tank.GetID()] = tank;
                        else
                            theWorld.Tanks.Add(tank.GetID(), tank);
                    }
                }

                // Add walls to world
                token = obj["wall"];
                if (token != null)
                {
                    //create rebuilt wall from json data 
                    lock (theWorld.Walls)
                    {
                        Wall rebuilt = JsonConvert.DeserializeObject<Wall>(data);

                        //add wall to world
                        theWorld.Walls.Add(rebuilt.GetID(), rebuilt);
                    }
                }

                // update any projectiles values
                token = obj["proj"];
                if (token != null)
                {
                    Projectile proj = JsonConvert.DeserializeObject<Projectile>(data);
                    lock (theWorld.Projectiles)
                    {
                        if (theWorld.Projectiles.ContainsKey(proj.GetID()))
                            theWorld.Projectiles[proj.GetID()] = proj;
                        else
                            theWorld.Projectiles.Add(proj.GetID(), proj);
                    }
                }

                //update beams 
                token = obj["beam"];
                if (token != null)
                {
                    Beam beam = JsonConvert.DeserializeObject<Beam>(data);
                    lock (theWorld.Beams)
                    {
                        if (theWorld.Beams.ContainsKey(beam.GetID()))
                            theWorld.Beams[beam.GetID()] = beam;
                        else
                            theWorld.Beams.Add(beam.GetID(), beam);
                    }
                }

                // update any new powerups/deletion of powerups
                token = obj["power"];
                if (token != null)
                {
                    Powerup powerup = JsonConvert.DeserializeObject<Powerup>(data);
                    lock (theWorld.Powerups)
                    {
                        if (theWorld.Powerups.ContainsKey(powerup.GetID()))
                            theWorld.Powerups[powerup.GetID()] = powerup;
                        else
                            theWorld.Powerups.Add(powerup.GetID(), powerup);
                    }
                }

                state.RemoveData(0, data.Length); //remove JSON string that has not been converted to world object 
            }

            DataEvent(theWorld); // finally, tell view to update with all new values

            ProcessInput(); //Send user input data to server each frame
        }

        /// <summary>
        /// Takes in key code from view and updates Control Commands object and keysDown set
        /// </summary>
        /// <param name="keyCode"></param>
        public void UpdateUserMovement(string keyCode)
        {
            //update movement based on which key is being pressed 
            switch (keyCode)
            {
                case "W":
                    {
                        cmds.SetMoving("up");
                        if(!keysDown.Contains(keyCode))
                            keysDown.Add(keyCode);
                        break;
                    }
                case "A":
                    {
                        cmds.SetMoving("left");
                        if (!keysDown.Contains(keyCode))
                            keysDown.Add(keyCode);
                        break;
                    }
                case "S":
                    {
                        cmds.SetMoving("down");
                        if (!keysDown.Contains(keyCode))
                            keysDown.Add(keyCode);
                        break;
                    }
                case "D":
                    {
                        cmds.SetMoving("right");
                        if (!keysDown.Contains(keyCode))
                            keysDown.Add(keyCode);
                        break;
                    }
                default:
                    {
                        cmds.SetMoving("none");
                        break;
                    }
            }

            
        }

        /// <summary>
        /// Takes in mouse down data from the view and updates Control Commands object and mouseDown set
        /// </summary>
        /// <param name="keyCode"></param>
        public void UpdateUserFiring(string keyCode)
        {
            //update firing status based on which mouse button is pressed
            switch (keyCode)
            {
                case "Left":
                    {
                        cmds.SetFire("main");
                        if (!mouseDown.Contains(keyCode))
                            mouseDown.Add(keyCode);
                        break;
                    }
                case "Right":
                    {
                        cmds.SetFire("alt");
                        if (!mouseDown.Contains(keyCode))
                            mouseDown.Add(keyCode);
                        break;
                    }
                default:
                    {
                        cmds.SetFire("none");
                        break;
                    }
            }         
        }

        /// <summary>
        /// Called when key is released
        /// key is removed from set of currently pressed keys 
        /// </summary>
        public void CancelKeyDown(string keyCode)
        {          
            //remove key from lists of currently held down keys 
            keysDown.Remove(keyCode);
            mouseDown.Remove(keyCode);

            //set user movement or firing to most recent key still held down, if applicable 
            if (keysDown.Count > 0 && !(keyCode.Equals("Left") || keyCode.Equals("Right")))
                UpdateUserMovement(keysDown[keysDown.Count - 1]);
            else if (mouseDown.Count > 0 && (keyCode.Equals("Left") || keyCode.Equals("Right")))
                UpdateUserFiring(mouseDown[mouseDown.Count - 1]);
        }

        /// <summary>
        /// sends JSON representation of Control Commands to server 
        /// </summary>
        public void ProcessInput()
        {
            if (keysDown.Count == 0) //set control command movement to none if no movement keys are being held down currently 
                cmds.SetMoving("none");

            if (mouseDown.Count == 0) //set control command firing to none if no firing keys are being held down currently 
                cmds.SetFire("none");

            //serialize and send control command object to server
            string cmdsString = JsonConvert.SerializeObject(cmds);

            Networking.Send(theServer.TheSocket, cmdsString + "\n");
        }

        /// <summary>
        /// Given the cursor position on the view, method calculates the normalized aiming direction vector
        /// updates the world and control commands 
        /// </summary>
        /// <param name="cursorPos"></param>
        public void UpdateMousePosition(Point cursorPos)
        {
            if(theWorld != null && theWorld.Tanks.ContainsKey(theWorld.tankID)) 
            {
                //calculate aiming vector 
                Vector2D mousePos = new Vector2D(cursorPos.X - Constants.ViewSize/2, cursorPos.Y - Constants.ViewSize/2); 
                mousePos.Normalize();

                theWorld.Tanks[theWorld.tankID].SetTurretDirection(mousePos); //set the tanks turret direction
                cmds.SetOrientation(theWorld.Tanks[theWorld.tankID].GetAiming()); //set control command aiming 
            }
            
        }
    }
}
