using Microsoft.Toolkit.Forms.UI.Controls;
using PocketTarkov.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows.UI.Xaml;

namespace PocketTarkov
{
    public partial class Form_ShowWebpage : MyForm
    {
        WebViewCompatible webBrowser = new WebViewCompatible();
        string webpageName;
        string webpageUrlString;
        Uri webpageUri;

        public Form_ShowWebpage(Form_RootOverlay _rootOverlay, string _webpageName)
        {
            webpageName = _webpageName;
            rootOverlay = _rootOverlay;
            InitializeComponent();
        }

        private void Form_ShowWebpage_Load(object sender, EventArgs e)
        {
            LoadFormProperties();
            LoadWebBrowserObject();
            AddMenuBar(false);
            AddWebNavigateButtons();
            LoadWebpage();
            this.FormClosing += new FormClosingEventHandler(this.DisposeBrowser);
        }

        private void DisposeBrowser(object sender, FormClosingEventArgs e)
        {
            webBrowser.Dispose();            
        }

        private void LoadWebpage()
        {
            switch (webpageName)
            {
                case "eftMonster":
                    this.Text = "EFT.Monster";
                    webpageUrlString = "https://eft.monster/";
                    break;
                case "noFoodGoogleDoc":
                    this.Text = "NoFoodAfterMidnight's EFT Ammo Chart";                    
                    webpageUrlString = "https://docs.google.com/spreadsheets/d/1jjWcIue0_PCsbLQAiL5VrIulPK8SzM5jjiCMx9zUuvE/htmlview?pru=AAABcrM5Ot0*zfa_Zlp1oww5fmcam1YG1w#";
                    break;
                case "wikiAmmo":
                    this.Text = "EFT Wiki";
                    webpageUrlString = "https://escapefromtarkov.gamepedia.com/Ballistics";
                    break;
                case "wikiTasks":
                    this.Text = "EFT Wiki";
                    webpageUrlString = "https://escapefromtarkov.gamepedia.com/Quests";
                    break;
                case "taskItemTracker":
                    this.Text = "Google Sheet Item Tracker";
                    if (String.IsNullOrWhiteSpace(rootOverlay.settings.googleDocURL))
                    {
                        webpageUrlString = "https://docs.google.com/spreadsheets/d/1FZMjvxB0RM89Nf7o7nNIWYf78ahp8-0q4nV6CrP-Kw8/edit?usp=sharing";
                    }
                    else
                    {
                        webpageUrlString = rootOverlay.settings.googleDocURL;
                    }                    
                    break;
            }
            try
            {
                webpageUri = new Uri(webpageUrlString);
                webBrowser.Navigate(webpageUri);
            }
            catch (Exception ea)
            {
                System.Diagnostics.Debug.WriteLine("Error connecting to webpage: " + ea.Message);
            }            
        }

        private void AddWebNavigateButtons()
        {
            Button backBtn = new Button();
            Button forwardBtn = new Button();
            ToolStripControlHost backBtnControlHost;
            ToolStripControlHost forwardBtnControlHost;

            backBtn.Text = "Back";
            backBtn.Click += new EventHandler(BackClicked);
            forwardBtn.Text = "Forward";
            forwardBtn.Click += new EventHandler(ForwardClicked);


            backBtnControlHost = new ToolStripControlHost(backBtn);
            forwardBtnControlHost = new ToolStripControlHost(forwardBtn);           

            ms.Items.Add(backBtnControlHost);
            ms.Items.Add(forwardBtnControlHost);
        }

        private void ForwardClicked(object sender, EventArgs e)
        {
            webBrowser.GoForward();
        }

        private void BackClicked(object sender, EventArgs e)
        {
           webBrowser.GoBack();
        }

        private void LoadWebBrowserObject()
        {
            // webBrowser = new WebViewCompatible();
            webBrowser.Size = this.Size;
            webBrowser.Dock = DockStyle.Fill;
            this.Controls.Add(webBrowser);
        }
    }
}
