using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PocketTarkov.Classes
{
    public class MyForm : Form
    {
        public bool KeepOpenBool = false;       

        protected Form_RootOverlay rootOverlay;
        protected MenuStrip ms = new MenuStrip();
        TrackBar opacityBar = new TrackBar();
        Label opacityBarLabel = new Label();

        CheckBox keepOpenCheckBox = new CheckBox();
        Label keepOpenCheckBoxLabel = new Label();

        public CheckBox clickableCheckBox = new CheckBox();
        Label clickableCheckBoxLabel = new Label();

        ToolStripControlHost opacityBarControlHost;
        ToolStripControlHost opacityBarLabelControlHost;

        ToolStripControlHost keepOpenCheckBoxControlHost;
        ToolStripControlHost keepOpenCheckBoxLabelControlHost;

        ToolStripControlHost clickableCheckBoxControlHost;
        ToolStripControlHost clickableCheckBoxLabelControlHost;

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

            // Set ToolStripControlHosts
            opacityBarControlHost = new ToolStripControlHost(opacityBar);
            opacityBarLabelControlHost = new ToolStripControlHost(opacityBarLabel);

            keepOpenCheckBoxControlHost = new ToolStripControlHost(keepOpenCheckBox);
            keepOpenCheckBoxLabelControlHost = new ToolStripControlHost(keepOpenCheckBoxLabel);

            clickableCheckBoxControlHost = new ToolStripControlHost(clickableCheckBox);
            clickableCheckBoxLabelControlHost = new ToolStripControlHost(clickableCheckBoxLabel);

            // Set Add ControlHosts too MainMenuStrip
            ms.Items.Add(opacityBarLabelControlHost);
            ms.Items.Add(opacityBarControlHost);

            ms.Items.Add(keepOpenCheckBoxLabelControlHost);
            ms.Items.Add(keepOpenCheckBoxControlHost);

            ms.Items.Add(clickableCheckBoxLabelControlHost);
            ms.Items.Add(clickableCheckBoxControlHost);

            // Set MainMenuStrip Properties
            ms.Dock = DockStyle.Top;

            this.Controls.Add(ms);
        }

        protected void LoadFormProperties()
        {
            this.MainMenuStrip = ms;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(rootOverlay.Width / 2, rootOverlay.Height / 2);
            this.Location = new Point(
                rootOverlay.ClientSize.Width / 2 - this.Size.Width / 2 + rootOverlay.Left,
                rootOverlay.ClientSize.Height / 2 - this.Size.Height / 2);
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
    }
}
