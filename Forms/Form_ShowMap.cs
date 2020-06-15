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
            AddMenuBar();
            LoadMapAsync();
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

        private async void LoadMapAsync()
        {
            switch (mapToShow)
            {
                case "shorelineMap":
                    this.Text = "Shoreline Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/d/d4/Shoreline_marvelin_2_updated.png?version=f28651df0d566bdc1996aeeacb496d15");
                    break;
                case "woodsMap":
                    this.Text = "Woods Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/0/05/Glory4lyfeWoods_map_v4_marked.png?version=c8013bc33cac57aac03a780f93daf13c");
                    break;
                case "reserveMap":
                    this.Text = "Reserve Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/9/9f/Keys_and_doors_v3.png?version=2cb6d8d74e6f3a52b8af30ca93bc86f5");
                    break;
                case "interchangeMap":
                    this.Text = "Interchange Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/e/e5/InterchangeMap_Updated_4.24.2020.png?version=c1114bd10889074ca8c8d85e3d1fb04b");
                    break;
                case "factoryMap":
                    this.Text = "Factory Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/c/cd/Factory_3D_b_Johnny_Tushonka.jpg?version=ce869f3014fc2e9f03becd479a4a8d4c");
                    break;
                case "customsMap":
                    this.Text = "Customs Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/c/c8/Customs_Nuxx_20190106_1.2.png?version=a3b44edf49616eaad2736c6523c977b0");
                    break;
                case "labsMap":
                    this.Text = "Labs Map";
                    orginalImage = await getImage("https://gamepedia.cursecdn.com/escapefromtarkov_gamepedia/6/6d/The_Lab_3D_map_by_SteelSmith_TTV.png?version=eac76b7104ce4c4e38dac1cfb0b01906");
                    break;                    
            }
            _orginalImage = new Bitmap(orginalImage);
            map.Image = _orginalImage;
            orginalSize = map.Image.Size;
            AddZoomPanEvents();
        }


        private void DisposeImages(object sender, FormClosedEventArgs e)
        {
            map.Image.Dispose();
            orginalImage.Dispose();
            _orginalImage.Dispose();
        }

        async Task<Image> getImage(string imgUrl)
        {
            Image map = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(imgUrl);
                
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var memStream = new MemoryStream();
                        await stream.CopyToAsync(memStream);
                        memStream.Position = 0;
                        map = Image.FromStream(memStream);
                    }
                }
            }
            return map;
        }

        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
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
            if (movingPoint.X < (map.Image.Width - this.Width) * -1)
            {
                movingPoint.X = (map.Image.Width - this.Width) * -1;
            }
            if (movingPoint.X > 0)
            {
                movingPoint.X = 0;
            }
            if(movingPoint.Y < (map.Image.Height - this.Height) * -1)
            {
                movingPoint.Y = (map.Image.Height - this.Height) * -1;
            }
            if (movingPoint.Y > 0)
            {
                movingPoint.Y = 0;
            }
            e.Graphics.Clear(Color.White);            
            e.Graphics.DrawImage(map.Image, movingPoint);
        }

        private void Map_DoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(scale != 1)
                {
                    scale = 1;
                    tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));
                    map.Image = null;
                    _orginalImage.Dispose();
                    _orginalImage = resizeImage(orginalImage, tempSize);
                    map.Image = _orginalImage;
                }
                else
                {
                    scale = 0.2;
                    tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));
                    map.Image = null;
                    _orginalImage.Dispose();
                    _orginalImage = resizeImage(orginalImage, tempSize);
                    map.Image = _orginalImage;
                }
            }
        }

        private void Map_Zoom(object sender, MouseEventArgs e)
        {
            int upOrDown = e.Delta;    
                
            if(upOrDown > 0)
            {
                if (scale <= 1)
                {
                    scale += 0.1;
                }
                else 
                {
                    return;
                }
            }
            if (upOrDown < 0)
            {
                if (scale > 0.3)
                {
                    scale -= 0.1;
                }
                else
                {
                    return;
                }
            } 
            tempSize = new Size((int)(orginalSize.Width * scale), (int)(orginalSize.Height * scale));           
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

        private void RemoveZoomPanEvents()
        {
            map.MouseDown -= Map_MouseDown;
            map.MouseUp -= Map_MouseUp;
            map.MouseMove -= Map_MouseMove;
            map.Paint -= Map_Paint;
        }
    }
}
