using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms.Design;
using Windows.Data.Xml.Dom;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PocketTarkov
{
    public partial class Form_RootOverlay : Form
    {
        public const string TARKOV_WINDOW_NAME = "EscapeFromTarkov";
        public SettingsData settings = new SettingsData();

        bool firstHotkeyPressed;
        bool secondHotkeyPressed;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem settingsItem;
        private ToolStripMenuItem exitItem;
        private ToolStripMenuItem hideOrShowItem;

        private LowLevelKeyboardHook kbh;

        private Form_Nav navForm = new Form_Nav();

        private Panel mapNavPanel = new Panel();
        private Panel ballisticsNavPanel = new Panel();
        private Panel taskNavPanel = new Panel();

        private RECT rect;

        private IntPtr handle = FindWindow(null, TARKOV_WINDOW_NAME);  

        // DLL Hooks
        #region DLL Hooks
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        #endregion

        public Form_RootOverlay()
        {
            InitializeComponent();
        }
        

        private void Form_RootOverlay_Load(object sender, EventArgs e)
        {
            LoadSettings();
            SetKeyboardHookEvents();
            SetRootOverlayProperties();
            SetupNotifyIcon();
            InitNavForm();
            OpenOrCloseOverlay();
        }  

        private void SetKeyboardHookEvents()
        {
            kbh = new LowLevelKeyboardHook();
            kbh.OnKeyPressed += kbh_OnKeyPressed;
            kbh.OnKeyUnpressed += kbh_OnKeyUnpressed;
            RootHookKeyboard();
        }

        private void SetRootOverlayProperties()
        {
            this.ShowInTaskbar = false;
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;


            // Sets Form Click Through
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            // Sets form position over game
            GetWindowRect(handle, out rect);
            this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
            this.Top = rect.top;
            this.Left = rect.left;


        }  

        private void SetupNotifyIcon()
        {
            contextMenuStrip = new ContextMenuStrip();
            settingsItem = new ToolStripMenuItem();
            settingsItem.Name = "settings";            
            settingsItem.Text = "Settings";
            settingsItem.Click += new EventHandler(NotifyIconClicked);

            exitItem = new ToolStripMenuItem();
            exitItem.Name = "exit";
            exitItem.Text = "Close Overlay";
            exitItem.Click += new EventHandler(NotifyIconClicked);

            hideOrShowItem = new ToolStripMenuItem();
            hideOrShowItem.Name = "openClose";
            hideOrShowItem.Text = "Hide or Show Overlay";
            hideOrShowItem.Click += new EventHandler(NotifyIconClicked);

            contextMenuStrip.Items.Add(hideOrShowItem);
            contextMenuStrip.Items.Add(settingsItem);
            contextMenuStrip.Items.Add(exitItem);


            notifyIcon = new NotifyIcon();

            notifyIcon.Icon = Properties.Resources.notifyIcon32;

            notifyIcon.Text = "Pocket EFT Overlay";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            notifyIcon.DoubleClick += new EventHandler(NotifyIconDoubleClick);
        }

        private void NotifyIconClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string whatClicked = item.Name;

            switch (whatClicked)
            {
                case "settings":
                    RootUnHookKeyboard();
                    Form_Settings settings = new Form_Settings(this);
                    settings.Show();
                    break;
                case "exit":
                    this.Close();
                    break;
                case "openClose":
                    OpenOrCloseOverlay();
                    break;
            }
        }

        private void InitNavForm()
        {
            navForm.ShowInTaskbar = false;
            navForm.TopLevel = true;
            navForm.TopMost = true;
            navForm.BackColor = Color.Wheat;
            navForm.TransparencyKey = Color.Wheat;
            navForm.FormBorderStyle = FormBorderStyle.None;
            navForm.StartPosition = FormStartPosition.Manual;
            navForm.Size = new Size(this.Width / 4, this.Height);
            navForm.Top = this.Top;
            navForm.Left = this.Left;

            AddMapPanel();
            AddBallisticsPanel();
            AddTaskPanel();
            AddSettingsButton();

            navForm.Show();            
        }         
        
        private void AddMapPanel()
        {            
            mapNavPanel.BackColor = Color.Transparent;           
            mapNavPanel.AutoSize = true;            
            mapNavPanel.Location = new Point(navForm.ClientSize.Width / 2 - mapNavPanel.Size.Width / 2,
                                                navForm.ClientSize.Height / 4 - mapNavPanel.Size.Height / 2);            
            navForm.Controls.Add(mapNavPanel);
            AddMapPanelButtons();
        }

        private void AddBallisticsPanel()
        {
            mapNavPanel.BackColor = Color.Transparent;
            ballisticsNavPanel.AutoSize = true;    

            ballisticsNavPanel.Top = mapNavPanel.Bottom + 50;
            ballisticsNavPanel.Left = mapNavPanel.Left;

            navForm.Controls.Add(ballisticsNavPanel);
            AddBallisticsPanelButtons();
        }

        private void AddTaskPanel()
        {
            taskNavPanel.BackColor = Color.Transparent;
            taskNavPanel.AutoSize = true;

            taskNavPanel.Top = ballisticsNavPanel.Bottom + 50;
            taskNavPanel.Left = mapNavPanel.Left;

            navForm.Controls.Add(taskNavPanel);
            AddTaskPanelButtons();
        }

        private void AddSettingsButton()
        {
            PictureBox settingsImg = new PictureBox();
            settingsImg.Image = Properties.Resources.settings;
            settingsImg.Refresh();
            settingsImg.SizeMode = PictureBoxSizeMode.AutoSize;
            settingsImg.Top = taskNavPanel.Bottom + 50;
            settingsImg.Left = mapNavPanel.Left;
            settingsImg.Name = "settings";
            settingsImg.MouseClick += new MouseEventHandler(SettingsClicked);
            settingsImg.BackColor = Color.Transparent;

            navForm.Controls.Add(settingsImg);
        }

        private void AddTaskPanelButtons()
        {
            PictureBox titleTasks = new PictureBox();
            titleTasks.Image = Properties.Resources.tasks;
            titleTasks.Refresh();
            titleTasks.SizeMode = PictureBoxSizeMode.AutoSize;
            titleTasks.Padding = new Padding(0, 0, 0, 25);
            titleTasks.BackColor = Color.Transparent;  

            PictureBox wiki = new PictureBox();
            wiki.Image = Properties.Resources.wiki;
            wiki.Refresh();
            wiki.SizeMode = PictureBoxSizeMode.AutoSize;
            wiki.Top = titleTasks.Bottom + 25;
            wiki.Name = "wikiTasks";
            wiki.MouseClick += new MouseEventHandler(WebpageClicked);
            wiki.BackColor = Color.Transparent;

            PictureBox taskItemTracker = new PictureBox();
            taskItemTracker.Image = Properties.Resources.taskTracker;
            taskNavPanel.Refresh();
            taskItemTracker.SizeMode = PictureBoxSizeMode.AutoSize;
            taskItemTracker.Top = wiki.Bottom;
            taskItemTracker.Name = "taskItemTracker";
            taskItemTracker.MouseClick += new MouseEventHandler(WebpageClicked);
            taskItemTracker.BackColor = Color.Transparent;

            taskNavPanel.Controls.Add(titleTasks);
            taskNavPanel.Controls.Add(wiki);
            taskNavPanel.Controls.Add(taskItemTracker);
        }

        private void AddBallisticsPanelButtons()
        {
            PictureBox titleAmmo = new PictureBox();
            titleAmmo.Image = Properties.Resources.ammoTitle;
            titleAmmo.Refresh();
            titleAmmo.SizeMode = PictureBoxSizeMode.AutoSize;
            titleAmmo.Padding = new Padding(0, 0, 0, 25);
            titleAmmo.BackColor = Color.Transparent;

            PictureBox eftMonster = new PictureBox();
            eftMonster.Image = Properties.Resources.eftMonster;
            eftMonster.Refresh();
            eftMonster.SizeMode = PictureBoxSizeMode.AutoSize;
            eftMonster.Top = titleAmmo.Bottom + 25;
            eftMonster.Name = "eftMonster";
            eftMonster.MouseClick += new MouseEventHandler(WebpageClicked);
            eftMonster.BackColor = Color.Transparent;

            PictureBox noFoodsG = new PictureBox();
            noFoodsG.Image = Properties.Resources.nofoodsGDoc;
            noFoodsG.Refresh();
            noFoodsG.SizeMode = PictureBoxSizeMode.AutoSize;
            noFoodsG.Top = eftMonster.Bottom;
            noFoodsG.Name = "noFoodGoogleDoc";
            noFoodsG.MouseClick += new MouseEventHandler(WebpageClicked);
            noFoodsG.BackColor = Color.Transparent;

            PictureBox wiki = new PictureBox();
            wiki.Image = Properties.Resources.wiki;
            wiki.Refresh();
            wiki.SizeMode = PictureBoxSizeMode.AutoSize;
            wiki.Top = noFoodsG.Bottom;
            wiki.Name = "wikiAmmo";
            wiki.MouseClick += new MouseEventHandler(WebpageClicked);
            wiki.BackColor = Color.Transparent;

            ballisticsNavPanel.Controls.Add(titleAmmo);
            ballisticsNavPanel.Controls.Add(eftMonster);
            ballisticsNavPanel.Controls.Add(noFoodsG);
            ballisticsNavPanel.Controls.Add(wiki);
        }

        private void AddMapPanelButtons()
        {
            PictureBox titleMaps = new PictureBox();
            titleMaps.Image = Properties.Resources.mapsTitle;
            titleMaps.Refresh();
            titleMaps.SizeMode = PictureBoxSizeMode.AutoSize;
            titleMaps.Padding = new Padding(0, 0, 0, 25);
            titleMaps.BackColor = Color.Transparent;

            PictureBox factoryMap = new PictureBox();
            factoryMap.Image = Properties.Resources.mapNameFactory;
            factoryMap.Refresh();
            factoryMap.SizeMode = PictureBoxSizeMode.AutoSize;
            factoryMap.Top = titleMaps.Bottom + 25;
            factoryMap.Left = titleMaps.Left;
            factoryMap.Name = "factoryMap";            
            factoryMap.MouseClick += new MouseEventHandler(ImageClicked);
            factoryMap.BackColor = Color.Transparent;

            PictureBox interchangeMap = new PictureBox();
            interchangeMap.Image = Properties.Resources.mapNameInterchange;
            interchangeMap.Refresh();
            interchangeMap.SizeMode = PictureBoxSizeMode.AutoSize;
            interchangeMap.Top = factoryMap.Bottom;
            interchangeMap.Left = titleMaps.Left;
            interchangeMap.Name = "interchangeMap";
            interchangeMap.MouseClick += new MouseEventHandler(ImageClicked);
            interchangeMap.BackColor = Color.Transparent;

            PictureBox reserveMap = new PictureBox();
            reserveMap.Image = Properties.Resources.mapNameReserve;
            reserveMap.Refresh();
            reserveMap.SizeMode = PictureBoxSizeMode.AutoSize;
            reserveMap.Top = interchangeMap.Bottom;
            reserveMap.Left = titleMaps.Left;
            reserveMap.MouseClick += new MouseEventHandler(ImageClicked);
            reserveMap.Name = "reserveMap";
            reserveMap.BackColor = Color.Transparent;

            PictureBox woodsMap = new PictureBox();
            woodsMap.Image = Properties.Resources.mapNameWoods;
            woodsMap.Refresh();
            woodsMap.SizeMode = PictureBoxSizeMode.AutoSize;
            woodsMap.Top = reserveMap.Bottom;
            woodsMap.Left = titleMaps.Left;
            woodsMap.MouseClick += new MouseEventHandler(ImageClicked);
            woodsMap.Name = "woodsMap";
            woodsMap.BackColor = Color.Transparent;

            PictureBox shorelineMap = new PictureBox();
            shorelineMap.Image = Properties.Resources.mapNameShoreline;
            shorelineMap.Refresh();
            shorelineMap.SizeMode = PictureBoxSizeMode.AutoSize;
            shorelineMap.Top = woodsMap.Bottom;
            shorelineMap.Left = titleMaps.Left;
            shorelineMap.MouseClick += new MouseEventHandler(ImageClicked);
            shorelineMap.Name = "shorelineMap";
            shorelineMap.BackColor = Color.Transparent;

            PictureBox customsMap = new PictureBox();
            customsMap.Image = Properties.Resources.mapNameCustoms;
            customsMap.Refresh();
            customsMap.SizeMode = PictureBoxSizeMode.AutoSize;
            customsMap.Top = shorelineMap.Bottom;
            customsMap.Left = titleMaps.Left;
            customsMap.MouseClick += new MouseEventHandler(ImageClicked);
            customsMap.Name = "customsMap";
            customsMap.BackColor = Color.Transparent;

            mapNavPanel.Controls.Add(titleMaps);
            mapNavPanel.Controls.Add(factoryMap);
            mapNavPanel.Controls.Add(interchangeMap);
            mapNavPanel.Controls.Add(reserveMap);
            mapNavPanel.Controls.Add(woodsMap);
            mapNavPanel.Controls.Add(shorelineMap);
            mapNavPanel.Controls.Add(customsMap);
        }

        private void NotifyIconDoubleClick(object Sender, EventArgs e)
        {
            OpenOrCloseOverlay();
        }

        private void ImageClicked(object sender, MouseEventArgs e)
        {
            PictureBox obj = sender as PictureBox;
            string mapNameToOpen = obj.Name;

            Form_ShowMap map = new Form_ShowMap(this, mapNameToOpen);
            map.ShowInTaskbar = false;
            map.Show();            
        }

        private void WebpageClicked(object sender, MouseEventArgs e)
        {
            PictureBox obj = sender as PictureBox;
            string webpageToOpen = obj.Name;

            Form_ShowWebpage webpage = new Form_ShowWebpage(this, webpageToOpen);
            webpage.ShowInTaskbar = false;
            webpage.Show();
        }

        private void SettingsClicked(object sender, MouseEventArgs e)
        {
            RootUnHookKeyboard();
            Form_Settings settings = new Form_Settings(this);
            settings.Show();
        }

        private void OpenOrCloseOverlay()
        {            
            //this.Visible = !this.Visible;
            foreach (Form c in Application.OpenForms)
            { 
                c.Visible = !c.Visible;
            }            
        }


        // Hotkeys
        #region Hotkeys        

        void kbh_OnKeyPressed(object sender, Keys e)
        {
            if (e == settings.hotkey01)// && e == Keys.V)
            {               
                firstHotkeyPressed = true;      
            }    
            else if (e == settings.hotkey02)
            {
                secondHotkeyPressed = true;
            }
            CheckKeyCombo();
        }

        void kbh_OnKeyUnpressed(object sender, Keys e)
        {
            if (e == settings.hotkey01)// && e == Keys.V)
            {
                firstHotkeyPressed = false;
            }
            else if (e == settings.hotkey02)
            {
                secondHotkeyPressed = false;
            }
        }

        void CheckKeyCombo()
        {
            if (firstHotkeyPressed && secondHotkeyPressed)
            {
                OpenOrCloseOverlay();
            }
            else if (firstHotkeyPressed && settings.hotkey02 == default)
            {
                OpenOrCloseOverlay();
            }
        }

        public void RootHookKeyboard()
        {
            kbh.HookKeyboard();
        }

        public void RootUnHookKeyboard()
        {
            kbh.UnHookKeyboard();
        }

        #endregion


        public struct RECT
        {
            public int left, top, right, bottom;
        }

        private void Form_RootOverlay_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var folder = Application.UserAppDataPath;
            string fileName = "/settings.txt";
            var path = folder + fileName;

            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(stream, settings);
            }
        }

        private void LoadSettings()
        {
           
            var folder = Application.UserAppDataPath;
            string fileName = "/settings.txt";
            var path = folder + fileName;

            if (File.Exists(path))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        settings = formatter.Deserialize(stream) as SettingsData;
                    }   
                }
                catch (Exception ea)
                {
                    System.Diagnostics.Debug.WriteLine("Error Loading: " + ea.Message);
                    settings.hotkey01 = Keys.LShiftKey;
                    settings.hotkey02 = Keys.C;
                    settings.googleDocURL = "";                    
                }
                
            }
            else
            {
                settings.hotkey01 = Keys.LShiftKey;
                settings.hotkey02 = Keys.C;
                settings.googleDocURL = "";
            }
        }
    }
}
