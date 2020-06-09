using PocketTarkov.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
            AddMenuBar();
            LoadMap();
        }

        private void LoadMapObject()
        {     
            // Set Map Properties
            map.Top = ms.Bottom;
            map.MouseDoubleClick += new MouseEventHandler(Map_DoubleClick);            
            map.SizeMode = PictureBoxSizeMode.StretchImage;
            map.Dock = DockStyle.Fill;            
            // Add Controls to Form
            this.Controls.Add(map);            
        }

        private void LoadMap()
        {
            switch (mapToShow)
            {
                case "shorelineMap":
                    this.Text = "Shoreline Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/d/d4/Shoreline_marvelin_2_updated.png?version=f28651df0d566bdc1996aeeacb496d15");
                    break;
                case "woodsMap":
                    this.Text = "Woods Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/0/05/Glory4lyfeWoods_map_v4_marked.png?version=c8013bc33cac57aac03a780f93daf13c");
                    break;
                case "reserveMap":
                    this.Text = "Reserve Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/9/9f/Keys_and_doors_v3.png?version=2cb6d8d74e6f3a52b8af30ca93bc86f5");
                    break;
                case "interchangeMap":
                    this.Text = "Interchange Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/e/e5/InterchangeMap_Updated_4.24.2020.png?version=c1114bd10889074ca8c8d85e3d1fb04b");
                    break;
                case "factoryMap":
                    this.Text = "Factory Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/c/cd/Factory_3D_b_Johnny_Tushonka.jpg?version=ce869f3014fc2e9f03becd479a4a8d4c");
                    break;
                case "customsMap":
                    this.Text = "Customs Map";
                    map.LoadAsync("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/c/c8/Customs_Nuxx_20190106_1.2.png?version=a3b44edf49616eaad2736c6523c977b0");
                    break;
            }
        }

        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            panning = true;
            startingPoint = new Point(e.Location.X - movingPoint.X,
                                      e.Location.Y - movingPoint.Y);
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
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawImage(map.Image, movingPoint);
        }

        private void Map_DoubleClick(object sender, MouseEventArgs e)
        {
            if(map.SizeMode == PictureBoxSizeMode.StretchImage)
            {
                float xPerc = (float)e.Location.X / (float)map.Width;
                float yPerc = (float)e.Location.Y / (float)map.Height;   
                
                map.SizeMode = PictureBoxSizeMode.Normal;

                float xConverted = map.Image.Width * xPerc;
                float yConverted = map.Image.Height * yPerc;

                int posX = (int)(xConverted * -1) + (map.Width / 2);
                int posY = (int)(yConverted * -1) + (map.Height / 2);


                movingPoint = new Point(posX, posY);
                AddZoomPanEvents();
            }
            else if (map.SizeMode == PictureBoxSizeMode.Normal)
            {
                RemoveZoomPanEvents();
                map.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void AddZoomPanEvents()
        {
            map.MouseDown += new MouseEventHandler(Map_MouseDown);
            map.MouseUp += new MouseEventHandler(Map_MouseUp);
            map.MouseMove += new MouseEventHandler(Map_MouseMove);
            map.Paint += new PaintEventHandler(Map_Paint);
        }

        private void RemoveZoomPanEvents()
        {
            map.MouseDown -= Map_MouseDown;
            map.MouseUp -= Map_MouseUp;
            map.MouseMove -= Map_MouseMove;
            map.Paint -= Map_Paint;
        }
    }
}
