using PocketTarkov.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PocketTarkov
{
    public partial class Form_ShowMap : MyForm
    {
        private PictureBox map = new PictureBox();
        private bool panning = false;
        private string mapToShow;        
        private Point startingPoint = Point.Empty;
        private Point movingPoint = Point.Empty;

        private Image orginalImage;
        private Image _orginalImage;
        private Size orginalSize;
        private Size tempSize;
        private double scale = 1;

        public Form_ShowMap(Form_RootOverlay _rootOverlay, string mapName)
        {
            rootOverlay = _rootOverlay;
            mapToShow = mapName;
            InitializeComponent();
        }

        private void Form_ShowMap_Load(object sender, EventArgs e)
        {
            LoadFormProperties();
            LoadMapObject();
            AddMenuBar(true);
            AddMenuBarMapOptions();
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DisposeImages);
        }

        private void LoadMapObject()
        {            
            // Set Map Properties
            map.Top = ms.Bottom;  
            map.MouseDoubleClick += new MouseEventHandler(Map_DoubleClick);            
            map.MouseWheel += new MouseEventHandler(Map_Zoom);            
            map.SizeMode = PictureBoxSizeMode.AutoSize;
            map.Dock = DockStyle.Fill;      
            
            // Add Controls to Form

            this.Controls.Add(map);
        }        

        private void AddMenuBarMapOptions()
        {
            switch (mapToShow)
            {
                case "shorelineMap":
                    mapVersionComboBox.Items.Add("Shoreline 2D");
                    mapVersionComboBox.Items.Add("Shoreline 3D");
                    mapVersionComboBox.Items.Add("Shoreline Keys");
                    mapVersionComboBox.Items.Add("Shoreline Resort");
                    break;
                case "woodsMap":
                    mapVersionComboBox.Items.Add("Woods");
                    break;                    
                case "reserveMap":
                    mapVersionComboBox.Items.Add("Reserve 2D");
                    mapVersionComboBox.Items.Add("Reserve 3D 01");
                    mapVersionComboBox.Items.Add("Reserve 3D 02");
                    mapVersionComboBox.Items.Add("Reserve Underground");
                    break;
                case "interchangeMap":
                    mapVersionComboBox.Items.Add("Interchange");
                    mapVersionComboBox.Items.Add("Interchange Stashes");
                    break;
                case "factoryMap":
                    mapVersionComboBox.Items.Add("Factory");
                    break;
                case "customsMap":
                    mapVersionComboBox.Items.Add("Customs 2D");
                    mapVersionComboBox.Items.Add("Customs Dorms");                    
                    break;
                case "labsMap":
                    mapVersionComboBox.Items.Add("Labs 3D");
                    mapVersionComboBox.Items.Add("Labs Basement");
                    mapVersionComboBox.Items.Add("Labs First Floor");
                    mapVersionComboBox.Items.Add("Labs Second Floor");
                    break;
                case "questItems":
                    mapVersionComboBox.Items.Add("Task Item Tracker");
                    break;
            }
            mapVersionComboBox.SelectedIndexChanged += new EventHandler(SelectedMapChanged);
            mapVersionComboBox.SelectedIndex = 0;
        }

        private void SelectedMapChanged(object sender, EventArgs e)
        {
            if (orginalImage != null) { orginalImage.Dispose(); }
            ComboBox comboBox = (ComboBox)sender;
            string selectedMap = comboBox.GetItemText(comboBox.SelectedItem);
            switch (selectedMap)
            {
                case "Shoreline 2D":
                    orginalImage = Properties.Resources.shoreline2d;
                    break;
                case "Shoreline 3D":
                    orginalImage = Properties.Resources.shoreline3d;
                    break;
                case "Shoreline Keys":
                    orginalImage = Properties.Resources.shorelineKeys;
                    break;
                case "Shoreline Resort":
                    orginalImage = Properties.Resources.shorelineResort;
                    break;
                case "Woods":
                    orginalImage = Properties.Resources.woods;
                    break;
                case "Reserve 2D":
                    orginalImage = Properties.Resources.reserve2d;
                    break;
                case "Reserve 3D 01":
                    orginalImage = Properties.Resources.reserve3d01;
                    break;
                case "Reserve 3D 02":
                    orginalImage = Properties.Resources.reserve3d02;
                    break;
                case "Reserve Underground":
                    orginalImage = Properties.Resources.reserveUnderground;
                    break;
                case "Interchange":
                    orginalImage = Properties.Resources.interchange;
                    break;
                case "Interchange Stashes":
                    orginalImage = Properties.Resources.interchangeStashes;
                    break;
                case "Factory":
                    orginalImage = Properties.Resources.factory3d;
                    break;
                case "Customs 2D":
                    orginalImage = Properties.Resources.customs2d;
                    break;
                case "Customs Dorms":
                    orginalImage = Properties.Resources.customsDorms;
                    break;
                case "Labs 3D":
                    orginalImage = Properties.Resources.theLab3d;
                    break;
                case "Labs Basement":
                    orginalImage = Properties.Resources.theLabBasement;
                    break;
                case "Labs First Floor":
                    orginalImage = Properties.Resources.theLabFirstFloor;
                    break;
                case "Labs Second Floor":
                    orginalImage = Properties.Resources.theLabSecondFloor;
                    break;
                case "Task Item Tracker":
                    orginalImage = Properties.Resources.questItemRequirements;
                    mapVersionComboBox.Visible = false;
                    mapVersionComboBoxLabel.Visible = false;
                    break;
            }
            orginalSize = orginalImage.Size;
            scale = 0.3;
            tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));
            
            if(map.Image != null)
            {                
                map.Image.Dispose();
            }
            if(_orginalImage != null)
            {
                _orginalImage.Dispose();
            }   
            _orginalImage = resizeImage(orginalImage, tempSize);
            map.Image = _orginalImage;
            AddZoomPanEvents();
        } 

        private void DisposeImages(object sender, FormClosedEventArgs e)
        {
            map.Image.Dispose();
            orginalImage.Dispose();
            _orginalImage.Dispose();
        }


        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                panning = true;
                startingPoint = new Point(e.Location.X - movingPoint.X,
                                          e.Location.Y - movingPoint.Y);
            }            
        }

        private void Map_MouseUp(object sender, MouseEventArgs e)
        {
            panning = false;
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (panning)
            {
                movingPoint = new Point(e.Location.X - startingPoint.X,
                                        e.Location.Y - startingPoint.Y);
                map.Invalidate();
            }
        }

        private void Map_Paint(object sender, PaintEventArgs e)
        {
            if (movingPoint.X < (map.Image.Width - this.Width) * -1)
            {
                movingPoint.X = (map.Image.Width - this.Width) * -1;
            }
            if (movingPoint.X > 0)
            {
                movingPoint.X = 0;
            }
            if (movingPoint.Y < (map.Image.Height - this.Height) * -1)
            {
                movingPoint.Y = (map.Image.Height - this.Height) * -1;
            }
            if (movingPoint.Y > 0)
            {
                movingPoint.Y = 0;
            }

            e.Graphics.Clear(Color.White);            
            e.Graphics.DrawImage(map.Image, movingPoint);
            System.Diagnostics.Debug.WriteLine("Moving point = X: " + movingPoint.X.ToString() + " Y: " + movingPoint.Y.ToString());
        }

        private void Map_DoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(scale != 1)
                {
                    scale = 1;
                }
                else
                {
                    scale = 0.2;   
                }
                // Set zoom point to middle of the screen.
                double xPerc = (double)(movingPoint.X * -1 + e.X) / (double)(tempSize.Width);
                double yPerc = (double)(movingPoint.Y * -1 + e.Y) / (double)(tempSize.Height);
                tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));
                int newX = (int)(tempSize.Width * xPerc);
                int newY = (int)(tempSize.Height * yPerc);
                movingPoint = new Point(newX * -1 + (map.Width / 2), newY * -1 + (map.Height / 2));

                map.Image = null;
                _orginalImage.Dispose();
                _orginalImage = resizeImage(orginalImage, tempSize);
                map.Image = _orginalImage;
            }            
        }

        private void Map_Zoom(object sender, MouseEventArgs e)
        {
            int upOrDown = e.Delta;    
                
            if(upOrDown > 0)
            {
                if ((scale + 0.1) <= 1)
                {
                    scale += 0.1;
                }
            }
            if (upOrDown < 0)
            {
                if ((scale - 0.1) > 0.3)
                {
                    scale -= 0.1;
                }
            }

            // Set zoom point to middle of the screen.
            double xPerc = (double)(movingPoint.X * -1 + e.X) / (double)(tempSize.Width);
            double yPerc = (double)(movingPoint.Y * -1 + e.Y) / (double)(tempSize.Height);      
            tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));
            int newX = (int)(tempSize.Width * xPerc);
            int newY = (int)(tempSize.Height * yPerc);
            movingPoint = new Point(newX*-1 + (map.Width / 2), newY*-1 + (map.Height / 2));
   
            map.Image = null;
            _orginalImage.Dispose();
            _orginalImage = resizeImage(orginalImage, tempSize);
            map.Image = _orginalImage;

            
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private void AddZoomPanEvents()
        {
            map.MouseDown += new MouseEventHandler(Map_MouseDown);
            map.MouseUp += new MouseEventHandler(Map_MouseUp);
            map.MouseMove += new MouseEventHandler(Map_MouseMove);
            map.Paint += new PaintEventHandler(Map_Paint);
        }
    }
}
