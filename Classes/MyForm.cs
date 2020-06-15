﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Windows.Media.Playback;

namespace PocketTarkov.Classes
{
    public class MyForm : Form
    {
        public bool KeepOpenBool = false;

        private const int sizeableGrabSize = 16;
        protected Form_RootOverlay rootOverlay;
        protected MenuStrip ms = new MenuStrip();
        TrackBar opacityBar = new TrackBar();
        Label opacityBarLabel = new Label();

        CheckBox keepOpenCheckBox = new CheckBox();
        Label keepOpenCheckBoxLabel = new Label();

        Label closeButton = new Label();

        public CheckBox clickableCheckBox = new CheckBox();
        Label clickableCheckBoxLabel = new Label();

        ToolStripControlHost opacityBarControlHost;
        ToolStripControlHost opacityBarLabelControlHost;

        ToolStripControlHost keepOpenCheckBoxControlHost;
        ToolStripControlHost keepOpenCheckBoxLabelControlHost;

        ToolStripControlHost clickableCheckBoxControlHost;
        ToolStripControlHost clickableCheckBoxLabelControlHost;

        ToolStripControlHost closeButtonControlHost;

        int initialStyle;


        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);


        protected void AddMenuBar()
        {
            // Set OpacityBar/Label Properties
            opacityBarLabel.Text = "Transparency";
            opacityBar.AutoSize = false;
            opacityBar.Height = 20;
            opacityBar.TickStyle = TickStyle.None;
            opacityBar.Minimum = 20;
            opacityBar.Maximum = 100;
            opacityBar.Value = 100;
            opacityBar.ValueChanged += new EventHandler(ChangeOpacity);

            // Set KeepOpen Checkbox/Label Properties
            keepOpenCheckBoxLabel.Text = "Keep Window Open";
            keepOpenCheckBoxLabel.Padding = new Padding(40, 0, 0, 0);
            keepOpenCheckBox.CheckedChanged += new EventHandler(KeepOpen);

            // Set Clickable Checkbox/Label Properties
            clickableCheckBoxLabel.Text = "Interactable (" + rootOverlay.settings.hotkey03.ToString() + " + " + rootOverlay.settings.hotkey04.ToString() + ")";
            clickableCheckBoxLabel.Padding = new Padding(40, 0, 0, 0);
            clickableCheckBox.Checked = true;
            clickableCheckBox.CheckedChanged += new EventHandler(Clickable);


            // Set Close Button 

            closeButton.TextAlign = ContentAlignment.MiddleCenter;
            closeButton.FlatStyle = FlatStyle.Flat;                        
            closeButton.BorderStyle = BorderStyle.FixedSingle;
            closeButton.ForeColor = Color.Black;
            closeButton.BackColor = Color.Red;
            closeButton.Text = "X";
            closeButton.Click += new EventHandler(CloseWindow);
            



            // Set ToolStripControlHosts
            opacityBarControlHost = new ToolStripControlHost(opacityBar);
            opacityBarLabelControlHost = new ToolStripControlHost(opacityBarLabel);

            keepOpenCheckBoxControlHost = new ToolStripControlHost(keepOpenCheckBox);
            keepOpenCheckBoxLabelControlHost = new ToolStripControlHost(keepOpenCheckBoxLabel);

            clickableCheckBoxControlHost = new ToolStripControlHost(clickableCheckBox);
            clickableCheckBoxLabelControlHost = new ToolStripControlHost(clickableCheckBoxLabel);

            closeButtonControlHost = new ToolStripControlHost(closeButton);
            closeButtonControlHost.Alignment = ToolStripItemAlignment.Right;
            closeButtonControlHost.AutoSize = true;
            closeButtonControlHost.Margin = new Padding(0,0,2,0);


            // Set Add ControlHosts too MainMenuStrip
            ms.Items.Add(opacityBarLabelControlHost);
            ms.Items.Add(opacityBarControlHost);

            ms.Items.Add(keepOpenCheckBoxLabelControlHost);
            ms.Items.Add(keepOpenCheckBoxControlHost);

            ms.Items.Add(clickableCheckBoxLabelControlHost);
            ms.Items.Add(clickableCheckBoxControlHost);

            ms.Items.Add(closeButtonControlHost);

            // Set MainMenuStrip Properties
            ms.Dock = DockStyle.Top;

            this.Controls.Add(ms);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            this.Close();
        }

        protected void LoadFormProperties()
        {
            this.Padding = new Padding(2, 2, 2, 2);
            this.MainMenuStrip = ms;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(rootOverlay.Width / 2, rootOverlay.Height / 2);
            this.Location = new Point(
                rootOverlay.ClientSize.Width / 2 - this.Size.Width / 2 + rootOverlay.Left,
                rootOverlay.ClientSize.Height / 2 - this.Size.Height / 2);
        }       



        // Resize
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
    
                // If mouse on bottom of window
                if (pos.Y >= this.ClientSize.Height - sizeableGrabSize)
                {
                    m.Result = (IntPtr)17;
                    return;
                }   
                // If mouse on right of window
                if (pos.X >= this.ClientSize.Width - sizeableGrabSize)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected void ChangeOpacity(object sender, EventArgs e)
        {
            TrackBar bar = sender as TrackBar;
            double value = bar.Value / 100f;
            this.Opacity = value;
        }

        protected void KeepOpen(object sender, EventArgs e)
        {
            CheckBox box = sender as CheckBox;
            KeepOpenBool = box.Checked;
        }

        protected void Clickable(object sender, EventArgs e)
        {
            SetClickableOrNot();
        }

        public void SetClickableOrNot()
        {
            if (clickableCheckBox.Checked)
            {
                SetClickable();
            }
            else if (!clickableCheckBox.Checked)
            {
                SetUnClickable();
            }
        }

        public void SetUnClickable()
        {
            initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);            
        }

        public void SetClickable()
        {            
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MyForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MyForm";
            this.ResumeLayout(false);

        }
    }
}
