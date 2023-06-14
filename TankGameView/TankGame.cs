//////////////////////////////////////////////
///FileName: TankGame.cs
///Authors: Dallon Haley and Tyler Allen
///Created On: 11/14/2020
///Description: View component of Tank Game 
/////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankGameController;
using TankGameWorld;

namespace TankGameView
{
    /// <summary>
    /// View component of TankWars game. View displays world, errors, and listens for user input events to then inform controller. 
    /// </summary>
    public partial class TankGame : Form
    {
        private Controller controller; 
        private World theWorld;

        //view components 
        DrawingPanel drawingPanel;
        Button startButton;
        Label server;
        TextBox serverTextBox;
        Label nameLabel;
        TextBox nameText;

        /// <summary>
        /// Initializes form elements and event handlers 
        /// </summary>
        public TankGame()
        {
            InitializeComponent();
            controller = new Controller();
            theWorld = new World();

            // Place and add the button
            startButton = new Button();
            startButton.Location = new Point(300, 5);
            startButton.Size = new Size(70, 20);
            startButton.Text = "Start";
            startButton.Click += Connect;
            this.Controls.Add(startButton);

            // Place and add the server label
            server = new Label();
            server.Text = "Server:";
            server.Location = new Point(15, 10);
            server.Size = new Size(40, 15);
            this.Controls.Add(server);

            // Place and add the server textbox
            serverTextBox = new TextBox();
            serverTextBox.Text = "localhost";
            serverTextBox.Location = new Point(65, 5);
            serverTextBox.Size = new Size(70, 15);
            this.Controls.Add(serverTextBox);

            // Place and add the name label
            nameLabel = new Label();
            nameLabel.Text = "Name:";
            nameLabel.Location = new Point(150, 10);
            nameLabel.Size = new Size(40, 15);
            this.Controls.Add(nameLabel);

            // Place and add the name textbox
            nameText = new TextBox();
            nameText.Text = "player";
            nameText.Location = new Point(200, 5);
            nameText.Size = new Size(70, 15);
            this.Controls.Add(nameText);

            // Place and add the drawing panel
            drawingPanel = new DrawingPanel(theWorld);
            drawingPanel.Location = new Point(0, 30);
            drawingPanel.Size = new Size(900, 900);
            this.Controls.Add(drawingPanel);

            //this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMove;

            controller.DataEvent += ProcessData; 
            controller.Error += Error;
        }

        /// <summary>
        /// if server name is not empty, method updates Controller with server name and player name in order to initialize server connection. 
        /// </summary>
        private void Connect(object sender, EventArgs e)
        {
            if(serverTextBox.Text == "") //check for empty server box 
            {
                MessageBox.Show("Please enter a server address.");
                return;
            }

            // Disable the controls and try connecting
            startButton.Enabled = false;
            serverTextBox.Enabled = false;
            nameText.Enabled = false;
            KeyPreview = true;

            controller.Connect(serverTextBox.Text, nameText.Text, theWorld); //initate connection network protocol in controller 
        }

        ///<summary>
        /// Event handler for server updates, updates drawings 
        ///</summary> 
        private void ProcessData(World theWorld) 
        {
            try
            {
                MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
                    this.Invoke(invalidator);               
            }
            catch(Exception)
            {
            }         
        }

        /// <summary>
        /// Error event handler
        /// Displays error message with given err string 
        /// </summary>
        private void Error(string err)
        {
            MessageBox.Show(err);

            this.Invoke(new MethodInvoker(() =>
            {
                startButton.Enabled = true;
                serverTextBox.Enabled = true;
                nameText.Enabled = true;
            }));
        }

        /// <summary>
        /// Key down handler
        /// Updates controller with key press
        /// </summary>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            controller.UpdateUserMovement(e.KeyCode.ToString());       

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Key up handler
        /// Updates controller with released key
        /// </summary>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            controller.CancelKeyDown(e.KeyCode.ToString());
        }

        /// <summary>
        /// Handle mouse down
        /// Updates Controller with Mouse press
        /// </summary>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            controller.UpdateUserFiring(e.Button.ToString());          
        }

        /// <summary>
        /// Handle mouse down
        /// Updates Controller with mouse release
        /// </summary>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Mouse Up");
            controller.CancelKeyDown(e.Button.ToString());
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine((cursorPos.X)+ " " + cursorPos.Y);
            controller.UpdateMousePosition(e.Location);
            
        }
    }
}
