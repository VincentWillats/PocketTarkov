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
using PocketTarkov.Classes;

namespace PocketTarkov
{
    public partial class Form_RootOverlay : Form
    {
        public const string TARKOV_WINDOW_NAME = "EscapeFromTarkov";
        public SettingsData settings = new SettingsData();

        bool openCloseFirstHotkeyPressed;
        bool openCloseSecondHotkeyPressed;
        bool clickableFirstHotkeyPressed;
        bool clickableSecondHotkeyPressed;

        bool textChangeEventOn;

        private TarkovItemClass _selectedItem;
        public TarkovItemClass selectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                UpdateItemDetails();
            }
        }

        CustomLabel itemMarketPrice = new CustomLabel();
        CustomLabel itemMarketPriceAvg24h = new CustomLabel();
        CustomLabel itemMarketPricePerSlot = new CustomLabel();
        CustomLabel itemTraderBest= new CustomLabel();
        CustomLabel itemTraderPrice = new CustomLabel();
        CustomLabel itemTraderPricePerSlot = new CustomLabel();
        CustomLabel itemLastUpdated = new CustomLabel();

        public CustomLabel itemMarketPriceVar = new CustomLabel();
        public CustomLabel itemMarketPriceAvg24hVar = new CustomLabel();
        public CustomLabel itemMarketPricePerSlotVar = new CustomLabel();
        public CustomLabel itemTraderBestVar= new CustomLabel();
        public CustomLabel itemTraderPriceVar= new CustomLabel();
        public CustomLabel itemTraderPricePerSlotVar = new CustomLabel();
        public CustomLabel itemLastUpdatedVar = new CustomLabel();

        ComboBox itemSearchBox = new ComboBox();

        public PictureBox itemImg = new PictureBox();

        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem settingsItem;
        private ToolStripMenuItem exitItem;
        private ToolStripMenuItem hideOrShowItem;

        private LowLevelKeyboardHook kbh;

        private Form_Nav navForm = new Form_Nav();

        private Panel itemSearch = new Panel();
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

        private async void InitNavForm()
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

            await AddItemSearchPanel();
            AddMapPanel();
            AddBallisticsPanel();
            AddTaskPanel();
            AddSettingsButton();

            navForm.Show();            
        }

        private void UpdateItemDetails()
        {
            itemMarketPriceVar.Text        = "- " + selectedItem.price.ToString("N0") + " ₽";
            itemMarketPriceAvg24hVar.Text   = "- " + selectedItem.avg24hPrice.ToString("N0") + " ₽";
            itemMarketPricePerSlotVar.Text  = "- " + (selectedItem.price / selectedItem.slots).ToString("N0") + " ₽";
            itemTraderBestVar.Text        = "- " + selectedItem.traderName;
            itemTraderPriceVar.Text        = "- " + selectedItem.traderPrice.ToString("N0") + " " + selectedItem.traderPriceCur;
            itemTraderPricePerSlotVar.Text = "- " + (selectedItem.traderPrice / selectedItem.slots).ToString("N0") + " " + selectedItem.traderPriceCur;
            itemLastUpdatedVar.Text        = "- " + selectedItem.updated.ToShortDateString();
            itemImg.Load(selectedItem.img);
        }

        private async Task AddItemSearchPanel()
        {
            itemSearch.AutoSize = true;            
            itemSearch.Location = new Point(navForm.ClientSize.Width / 4 - mapNavPanel.Size.Width / 2,
                                                0 + 100);
            
            itemSearch.BorderStyle = BorderStyle.FixedSingle;

            navForm.Controls.Add(itemSearch);
            await AddItemPanelObjectsAsync();
        }

        private void AddMapPanel()
        {            
            mapNavPanel.BackColor = Color.Transparent;           
            mapNavPanel.AutoSize = true;            
            mapNavPanel.Top = itemSearch.Bottom + 50;
            mapNavPanel.Left = itemSearch.Left;
            mapNavPanel.BorderStyle = BorderStyle.FixedSingle;    
            
            navForm.Controls.Add(mapNavPanel);
            AddMapPanelButtons();
        }

        private void AddBallisticsPanel()
        {
            ballisticsNavPanel.BackColor = Color.Transparent;
            ballisticsNavPanel.AutoSize = true;    

            ballisticsNavPanel.Top = mapNavPanel.Bottom + 50;
            ballisticsNavPanel.Left = mapNavPanel.Left;
            ballisticsNavPanel.BorderStyle = BorderStyle.FixedSingle;
            navForm.Controls.Add(ballisticsNavPanel);
            AddBallisticsPanelButtons();
        }

        private void AddTaskPanel()
        {
            taskNavPanel.BackColor = Color.Transparent;
            taskNavPanel.AutoSize = true;

            taskNavPanel.Top = ballisticsNavPanel.Bottom + 50;
            taskNavPanel.Left = mapNavPanel.Left;
            taskNavPanel.BorderStyle = BorderStyle.FixedSingle;

            navForm.Controls.Add(taskNavPanel);
            AddTaskPanelButtons();
        }

        private void AddSettingsButton()
        {
            PictureBox settingsImg = new PictureBox();
            settingsImg.Image = Properties.Resources.settings;
            settingsImg.Refresh();
            settingsImg.SizeMode = PictureBoxSizeMode.AutoSize;
            settingsImg.Top = 50;
            settingsImg.Left = 50;
            settingsImg.Name = "settings";
            settingsImg.MouseClick += new MouseEventHandler(SettingsClicked);
            settingsImg.BackColor = Color.Transparent;

            navForm.Controls.Add(settingsImg);
        }

        private async Task AddItemPanelObjectsAsync()
        {
            
            PictureBox itemTitle = new PictureBox();            

            itemMarketPricePerSlot.Text  = "Market Price Per Slot";
            itemMarketPrice.Text        = "Market Price";
            itemMarketPriceAvg24h.Text   = "Market Average 24h";
            itemTraderBest.Text        = "Best Trader";
            itemTraderPrice.Text        = "Trader Price";
            itemTraderPricePerSlot.Text = "Trader Price Per Slot";
            itemLastUpdated.Text        = "Last Updated";
           
            itemMarketPricePerSlot.Font = new Font("Rockwell Nova", 12);
            itemMarketPrice.Font = new Font("Rockwell Nova", 12);
            itemMarketPriceAvg24h.Font = new Font("Rockwell Nova", 12);
            itemTraderBest.Font = new Font("Rockwell Nova", 12);
            itemTraderPrice.Font = new Font("Rockwell Nova", 12);
            itemTraderPricePerSlot.Font = new Font("Rockwell Nova", 12);
            itemLastUpdated.Font = new Font("Rockwell Nova", 12);

            itemMarketPricePerSlotVar.Font = new Font("Rockwell Nova", 12);
            itemMarketPriceVar.Font = new Font("Rockwell Nova", 12);
            itemMarketPriceAvg24hVar.Font = new Font("Rockwell Nova", 12);
            itemTraderBestVar.Font = new Font("Rockwell Nova", 12);
            itemTraderPriceVar.Font = new Font("Rockwell Nova", 12);
            itemTraderPricePerSlotVar.Font = new Font("Rockwell Nova", 12);
            itemLastUpdatedVar.Font = new Font("Rockwell Nova", 12);

            itemMarketPrice.AutoSize = true;
            itemMarketPriceAvg24h.AutoSize = true;
            itemMarketPricePerSlot.AutoSize = true;
            itemTraderBest.AutoSize = true;
            itemTraderPrice.AutoSize = true;
            itemTraderPricePerSlot.AutoSize = true;
            itemLastUpdated.AutoSize = true;


            if (TarkovItemController.allItemsNames.Count <= 0) // If items not loaded, load items.
            {
                await TarkovItemController.GetAllItemNamesListAsync();
            }         

            itemTitle.Image = Properties.Resources.itemSearch;
            itemTitle.Refresh();
            itemTitle.SizeMode = PictureBoxSizeMode.AutoSize;
            itemTitle.Padding = new Padding(10, 10, 0, 0);
            itemTitle.BackColor = Color.Transparent;            

            itemSearchBox.Top = itemTitle.Bottom + 37;
            itemSearchBox.Left = 10;
            itemSearchBox.Width = 200;
            itemSearchBox.AutoCompleteMode = AutoCompleteMode.None;
            itemSearchBox.TextChanged += new EventHandler(SearchItemList);
            textChangeEventOn = true;
            itemSearchBox.SelectedIndexChanged += new EventHandler(ItemSelected);
            itemSearchBox.KeyDown += new KeyEventHandler(ComboKeyPress);
            itemSearchBox.GotFocus += new EventHandler(itemSearchEntered);

            itemImg.BorderStyle = BorderStyle.FixedSingle;
            itemImg.Size = new Size(50, 50);
            itemImg.Top = itemTitle.Bottom + 25;
            itemImg.Left = itemSearchBox.Right + 10;
            itemImg.SizeMode = PictureBoxSizeMode.StretchImage;
            itemImg.BackColor = Color.Transparent;

            itemMarketPrice.Top = itemImg.Bottom + 2;
            itemMarketPrice.Padding = new Padding(10, 0, 0, 0);
            itemMarketPrice.BackColor = Color.Transparent;
            itemMarketPrice.ForeColor = Color.LightBlue;            
            

            itemMarketPriceAvg24h.Top = itemMarketPrice.Bottom + 2;
            itemMarketPriceAvg24h.Padding = new Padding(10, 0, 0, 0);
            itemMarketPriceAvg24h.BackColor = Color.Transparent;
            itemMarketPriceAvg24h.ForeColor = Color.LightBlue;
            

            itemMarketPricePerSlot.Top = itemMarketPriceAvg24h.Bottom + 2;
            itemMarketPricePerSlot.Padding = new Padding(10, 0, 0, 0);
            itemMarketPricePerSlot.BackColor = Color.Transparent;
            itemMarketPricePerSlot.ForeColor = Color.LightBlue;

            itemTraderBest.Top = itemMarketPricePerSlot.Bottom + 2;
            itemTraderBest.Padding = new Padding(10, 0, 0, 0);
            itemTraderBest.BackColor = Color.Transparent;
            itemTraderBest.ForeColor = Color.LightBlue;

            itemTraderPrice.Top = itemTraderBest.Bottom + 2;
            itemTraderPrice.Padding = new Padding(10, 0, 0, 0);
            itemTraderPrice.BackColor = Color.Transparent;
            itemTraderPrice.ForeColor = Color.LightBlue;
            

            itemTraderPricePerSlot.Top = itemTraderPrice.Bottom + 2;
            itemTraderPricePerSlot.Padding = new Padding(10, 0, 0, 0);
            itemTraderPricePerSlot.BackColor = Color.Transparent;
            itemTraderPricePerSlot.ForeColor = Color.LightBlue;
            

            itemLastUpdated.Top = itemTraderPricePerSlot.Bottom + 2;
            itemLastUpdated.Padding = new Padding(10, 0, 0, 10);
            itemLastUpdated.BackColor = Color.Transparent;
            itemLastUpdated.ForeColor = Color.LightBlue;            

            itemSearch.Controls.Add(itemTitle);
            itemSearch.Controls.Add(itemImg);
            itemSearch.Controls.Add(itemSearchBox);
            itemSearch.Controls.Add(itemMarketPrice);
            itemSearch.Controls.Add(itemMarketPriceAvg24h);
            itemSearch.Controls.Add(itemMarketPricePerSlot);
            itemSearch.Controls.Add(itemTraderBest);
            itemSearch.Controls.Add(itemTraderPrice);
            itemSearch.Controls.Add(itemTraderPricePerSlot);
            itemSearch.Controls.Add(itemLastUpdated);

            itemTraderPricePerSlotVar.Left = itemMarketPricePerSlot.Right + 5;
            itemTraderPricePerSlotVar.Top = itemTraderPricePerSlot.Top;
            itemTraderPricePerSlotVar.ForeColor = Color.LightBlue;
            itemTraderPricePerSlotVar.AutoSize = true;

            itemLastUpdatedVar.Left = itemMarketPricePerSlot.Right + 5;
            itemLastUpdatedVar.Top = itemLastUpdated.Top;
            itemLastUpdatedVar.ForeColor = Color.LightBlue;
            itemLastUpdatedVar.AutoSize = true;

            itemTraderBestVar.Left = itemMarketPricePerSlot.Right + 5;
            itemTraderBestVar.Top = itemTraderBest.Top;
            itemTraderBestVar.ForeColor = Color.LightBlue;
            itemTraderBestVar.AutoSize = true;

            itemTraderPriceVar.Left = itemMarketPricePerSlot.Right + 5;
            itemTraderPriceVar.Top = itemTraderPrice.Top;
            itemTraderPriceVar.ForeColor = Color.LightBlue;
            itemTraderPriceVar.AutoSize = true;

            itemMarketPricePerSlotVar.Left = itemMarketPricePerSlot.Right + 5;
            itemMarketPricePerSlotVar.Top = itemMarketPricePerSlot.Top;
            itemMarketPricePerSlotVar.ForeColor = Color.LightBlue;
            itemMarketPricePerSlotVar.AutoSize = true;

            itemMarketPriceAvg24hVar.Left = itemMarketPricePerSlot.Right + 5;
            itemMarketPriceAvg24hVar.Top = itemMarketPriceAvg24h.Top;
            itemMarketPriceAvg24hVar.ForeColor = Color.LightBlue;
            itemMarketPriceAvg24hVar.AutoSize = true;

            itemMarketPriceVar.Left = itemMarketPricePerSlot.Right + 5;
            itemMarketPriceVar.Top = itemMarketPrice.Top;
            itemMarketPriceVar.ForeColor = Color.LightBlue;
            itemMarketPriceVar.AutoSize = true;

            itemSearch.Controls.Add(itemMarketPriceVar);
            itemSearch.Controls.Add(itemMarketPriceAvg24hVar);
            itemSearch.Controls.Add(itemMarketPricePerSlotVar);
            itemSearch.Controls.Add(itemTraderBestVar);
            itemSearch.Controls.Add(itemTraderPriceVar);
            itemSearch.Controls.Add(itemTraderPricePerSlotVar);
            itemSearch.Controls.Add(itemLastUpdatedVar);
        }

        private void itemSearchEntered(object sender, EventArgs e)
        {
           
            textChangeEventOn = true;
           
        }

        private void ComboKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {                
                textChangeEventOn = false;
            }
            if(e.KeyCode == Keys.Back)
            {                                    
                textChangeEventOn = true;                
            }  
            if(itemSearchBox.Text.Length <= 1)
            {
                textChangeEventOn = true;
            }
        }

        private void SearchItemList(object sender, EventArgs e)
        {
            if (!textChangeEventOn) { return; }
            if (itemSearchBox.Text.Length < 2)
            {
                return;
            }            
            
            textChangeEventOn = false;
            itemSearchBox.SelectedIndexChanged -= new EventHandler(ItemSelected);

            var txt = itemSearchBox.Text;
            var list = TarkovItemController.GetMatchingItems(itemSearchBox.Text);
            var listCount = list.Count();
            if(listCount < 30)
            {
                itemSearchBox.DroppedDown = false;
            }
            if (listCount == 1)
            {                
                itemSearchBox.DataSource = list.ToList();
                itemSearchBox.DroppedDown = true;
                Cursor.Current = Cursors.Default;
                itemSearchBox.SelectedIndex = 0;
                selectedItem = itemSearchBox.SelectedItem as TarkovItemClass;                               
            }
            else if (listCount > 1)
            {                
                itemSearchBox.DataSource = list.ToList();
                itemSearchBox.DroppedDown = true;
                Cursor.Current = Cursors.Default;
                itemSearchBox.Text = txt;
                itemSearchBox.SelectionStart = txt.Length;                
            }
            
            else
            {
                itemSearchBox.DroppedDown = false;
                itemSearchBox.SelectionStart = txt.Length;
            }            
            textChangeEventOn = true;
            itemSearchBox.SelectedIndexChanged += new EventHandler(ItemSelected);
        }

        private void ItemSelected(object sender, EventArgs e)
        {            
            try
            {            
                selectedItem = itemSearchBox.SelectedItem as TarkovItemClass;
            }
            catch (Exception ea)
            {
                System.Diagnostics.Debug.WriteLine("Error displaying item details: " + ea.Message);
            }            
        }

        private void AddTaskPanelButtons()
        {
            PictureBox titleTasks = new PictureBox();
            titleTasks.Image = Properties.Resources.quests;
            titleTasks.Refresh();
            titleTasks.SizeMode = PictureBoxSizeMode.AutoSize;
            titleTasks.Padding = new Padding(10, 10, 0, 0);
            titleTasks.BackColor = Color.Transparent;  

            PictureBox wiki = new PictureBox();
            wiki.Image = Properties.Resources.wiki;
            wiki.Refresh();
            wiki.SizeMode = PictureBoxSizeMode.AutoSize;
            wiki.Top = titleTasks.Bottom + 25;
            wiki.Padding = new Padding(10, 0, 0, 0);
            wiki.Name = "wikiTasks";
            wiki.MouseClick += new MouseEventHandler(WebpageClicked);
            wiki.BackColor = Color.Transparent;

            PictureBox taskItemsImg = new PictureBox();
            taskItemsImg.Image = Properties.Resources.questItems;
            taskItemsImg.Refresh();
            taskItemsImg.SizeMode = PictureBoxSizeMode.AutoSize;
            taskItemsImg.Top = wiki.Bottom + 2;
            taskItemsImg.Padding = new Padding(10, 0, 0, 0);
            taskItemsImg.Name = "questItems";
            taskItemsImg.MouseClick += new MouseEventHandler(ImageClicked);
            taskItemsImg.BackColor = Color.Transparent;

            PictureBox taskItemTracker = new PictureBox();
            taskItemTracker.Image = Properties.Resources.questItemTracker;
            taskItemTracker.Refresh();
            taskItemTracker.SizeMode = PictureBoxSizeMode.AutoSize;
            taskItemTracker.Top = taskItemsImg.Bottom + 2;
            taskItemTracker.Padding = new Padding(10, 0, 0, 10);
            taskItemTracker.Name = "taskItemTracker";
            taskItemTracker.MouseClick += new MouseEventHandler(WebpageClicked);
            taskItemTracker.BackColor = Color.Transparent;

            taskNavPanel.Controls.Add(titleTasks);
            taskNavPanel.Controls.Add(wiki);
            taskNavPanel.Controls.Add(taskItemsImg);
            taskNavPanel.Controls.Add(taskItemTracker);
        }

        private void AddBallisticsPanelButtons()
        {
            PictureBox titleAmmo = new PictureBox();
            titleAmmo.Image = Properties.Resources.ammoTitle;
            titleAmmo.Refresh();
            titleAmmo.SizeMode = PictureBoxSizeMode.AutoSize;
            titleAmmo.Padding = new Padding(10, 10, 0, 0);
            titleAmmo.BackColor = Color.Transparent;

            PictureBox eftMonster = new PictureBox();
            eftMonster.Image = Properties.Resources.eftMonster;
            eftMonster.Refresh();
            eftMonster.SizeMode = PictureBoxSizeMode.AutoSize;
            eftMonster.Top = titleAmmo.Bottom + 25;
            eftMonster.Padding = new Padding(10, 0, 0, 0);
            eftMonster.Name = "eftMonster";
            eftMonster.MouseClick += new MouseEventHandler(WebpageClicked);
            eftMonster.BackColor = Color.Transparent;

            PictureBox noFoodsG = new PictureBox();
            noFoodsG.Image = Properties.Resources.nofoodsGDoc;
            noFoodsG.Refresh();
            noFoodsG.SizeMode = PictureBoxSizeMode.AutoSize;
            noFoodsG.Top = eftMonster.Bottom + 2;
            noFoodsG.Padding = new Padding(10, 0, 0, 0);
            noFoodsG.Name = "noFoodGoogleDoc";
            noFoodsG.MouseClick += new MouseEventHandler(WebpageClicked);
            noFoodsG.BackColor = Color.Transparent;

            PictureBox wiki = new PictureBox();
            wiki.Image = Properties.Resources.wiki;
            wiki.Refresh();
            wiki.SizeMode = PictureBoxSizeMode.AutoSize;
            wiki.Top = noFoodsG.Bottom + 2;
            wiki.Padding = new Padding(10, 0, 0, 10);
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
            titleMaps.Padding = new Padding(10, 10, 0, 0);
            titleMaps.BackColor = Color.Transparent;

            PictureBox factoryMap = new PictureBox();
            factoryMap.Image = Properties.Resources.mapNameFactory;
            factoryMap.Refresh();
            factoryMap.SizeMode = PictureBoxSizeMode.AutoSize;
            factoryMap.Top = titleMaps.Bottom + 25;
            factoryMap.Padding = new Padding(10, 0, 0, 0);
            factoryMap.Name = "factoryMap";            
            factoryMap.MouseClick += new MouseEventHandler(ImageClicked);
            factoryMap.BackColor = Color.Transparent;

            PictureBox interchangeMap = new PictureBox();
            interchangeMap.Image = Properties.Resources.mapNameInterchange;
            interchangeMap.Refresh();
            interchangeMap.SizeMode = PictureBoxSizeMode.AutoSize;
            interchangeMap.Top = factoryMap.Bottom + 2;
            interchangeMap.Padding = new Padding(10, 0, 0, 0);
            interchangeMap.Name = "interchangeMap";
            interchangeMap.MouseClick += new MouseEventHandler(ImageClicked);
            //interchangeMap.BackColor = Color.Transparent;

            PictureBox reserveMap = new PictureBox();
            reserveMap.Image = Properties.Resources.mapNameReserve;
            reserveMap.Refresh();
            reserveMap.SizeMode = PictureBoxSizeMode.AutoSize;
            reserveMap.Top = interchangeMap.Bottom + 2;
            reserveMap.Padding = new Padding(10, 0, 0, 0);
            reserveMap.MouseClick += new MouseEventHandler(ImageClicked);
            reserveMap.Name = "reserveMap";
            reserveMap.BackColor = Color.Transparent;

            PictureBox woodsMap = new PictureBox();
            woodsMap.Image = Properties.Resources.mapNameWoods;
            woodsMap.Refresh();
            woodsMap.SizeMode = PictureBoxSizeMode.AutoSize;
            woodsMap.Top = reserveMap.Bottom + 2;
            woodsMap.Padding = new Padding(10, 0, 0, 0);
            woodsMap.MouseClick += new MouseEventHandler(ImageClicked);
            woodsMap.Name = "woodsMap";
            woodsMap.BackColor = Color.Transparent;

            PictureBox shorelineMap = new PictureBox();
            shorelineMap.Image = Properties.Resources.mapNameShoreline;
            shorelineMap.Refresh();
            shorelineMap.SizeMode = PictureBoxSizeMode.AutoSize;
            shorelineMap.Top = woodsMap.Bottom + 2;
            shorelineMap.Padding = new Padding(10, 0, 0, 0);
            shorelineMap.MouseClick += new MouseEventHandler(ImageClicked);
            shorelineMap.Name = "shorelineMap";
            shorelineMap.BackColor = Color.Transparent;

            PictureBox customsMap = new PictureBox();
            customsMap.Image = Properties.Resources.mapNameCustoms;
            customsMap.Refresh();
            customsMap.SizeMode = PictureBoxSizeMode.AutoSize;
            customsMap.Top = shorelineMap.Bottom + 2;
            customsMap.Padding = new Padding(10, 0, 0, 0);
            customsMap.MouseClick += new MouseEventHandler(ImageClicked);
            customsMap.Name = "customsMap";
            customsMap.BackColor = Color.Transparent;

            PictureBox labsMap = new PictureBox();
            labsMap.Image = Properties.Resources.mapNameLabs;
            labsMap.Refresh();
            labsMap.SizeMode = PictureBoxSizeMode.AutoSize;
            labsMap.Top = customsMap.Bottom + 2;
            labsMap.Padding = new Padding(10, 0, 0, 10);
            labsMap.MouseClick += new MouseEventHandler(ImageClicked);
            labsMap.Name = "labsMap";
            labsMap.BackColor = Color.Transparent;

            mapNavPanel.Controls.Add(titleMaps);
            mapNavPanel.Controls.Add(factoryMap);
            mapNavPanel.Controls.Add(interchangeMap);
            mapNavPanel.Controls.Add(reserveMap);
            mapNavPanel.Controls.Add(woodsMap);
            mapNavPanel.Controls.Add(shorelineMap);
            mapNavPanel.Controls.Add(customsMap);
            mapNavPanel.Controls.Add(labsMap);
        }

        private void NotifyIconDoubleClick(object Sender, EventArgs e)
        {
            OpenOrCloseOverlay();
        }

        private void ImageClicked(object sender, MouseEventArgs e)
        {
            PictureBox obj = sender as PictureBox;
            string imgToOpen = obj.Name;

            Form_ShowMap map = new Form_ShowMap(this, imgToOpen);
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
            foreach (Form f in Application.OpenForms)
            {
                if(f is MyForm)
                {
                    MyForm myf = f as MyForm;
                    if (!myf.KeepOpenBool) // If not keep open ticked
                    {
                        myf.Visible = !myf.Visible;
                    }                    
                }
                else
                {
                    f.Visible = !f.Visible;
                }                
            }         
        }


        // Hotkeys
        #region Hotkeys        

        void kbh_OnKeyPressed(object sender, Keys e)
        {
            if (e == settings.hotkey01)// && e == Keys.V)
            {               
                openCloseFirstHotkeyPressed = true;      
            }            
            else if (e == settings.hotkey02)
            {
                openCloseSecondHotkeyPressed = true;
            }
            if (e == settings.hotkey03)
            {
                clickableFirstHotkeyPressed = true;
            }
            else if (e == settings.hotkey04)
            {
                clickableSecondHotkeyPressed = true;
            }
            CheckKeyCombo();
        }

        void kbh_OnKeyUnpressed(object sender, Keys e)
        {
            if (e == settings.hotkey01)// && e == Keys.V)
            {
                openCloseFirstHotkeyPressed = false;
            }
            else if (e == settings.hotkey02)
            {
                openCloseSecondHotkeyPressed = false;
            }
            if (e == settings.hotkey03)
            {
                clickableFirstHotkeyPressed = false;
            }
            else if (e == settings.hotkey04)
            {
                clickableSecondHotkeyPressed = false;
            }
        }

        void CheckKeyCombo()
        {
            if (openCloseFirstHotkeyPressed && openCloseSecondHotkeyPressed)
            {
                OpenOrCloseOverlay();
            }
            else if (openCloseFirstHotkeyPressed && settings.hotkey02 == default)
            {
                OpenOrCloseOverlay();
            }
            if (clickableFirstHotkeyPressed && clickableSecondHotkeyPressed)
            {
                MakeSubFormsClickableOrUnclickable();
            }
            else if (clickableFirstHotkeyPressed && settings.hotkey04 == default)
            {
                MakeSubFormsClickableOrUnclickable();
            }
        }

        private void MakeSubFormsClickableOrUnclickable()
        {           
            foreach(Form f in Application.OpenForms)
            {
                if (f is MyForm)
                {
                    MyForm myf = f as MyForm;
                    myf.clickableCheckBox.Checked = !myf.clickableCheckBox.Checked;
                }
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
                    settings.hotkey03 = Keys.LShiftKey;
                    settings.hotkey04 = Keys.M;
                    settings.googleDocURL = "";                    
                }
                
            }
            else
            {
                settings.hotkey01 = Keys.LShiftKey;
                settings.hotkey02 = Keys.C;
                settings.hotkey03 = Keys.LShiftKey;
                settings.hotkey04 = Keys.M;
                settings.googleDocURL = "";
            }
        }
    }
}
