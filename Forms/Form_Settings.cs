using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows.UI.Core;

namespace PocketTarkov
{
    public partial class Form_Settings : Form
    {
        LowLevelKeyboardHook kbh;
        Form_RootOverlay rootOverlay;
        Keys pressedKey1;
        Keys pressedKey2;
        public Form_Settings(Form_RootOverlay _rootOverlay)
        {
            rootOverlay = _rootOverlay;
            InitializeComponent();
        }

        private void Form_Settings_Load(object sender, EventArgs e)
        {
            LoadProperties();            
        }

        private void LoadProperties()
        {
            this.ShowInTaskbar = false;
            this.Text = "Pocket Tarkov Settings";
            this.TopLevel = true;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.StartPosition = FormStartPosition.Manual;

            this.Location = new Point(
                rootOverlay.ClientSize.Width / 2 - this.Size.Width / 2 + rootOverlay.Left,
                rootOverlay.ClientSize.Height / 2 - this.Size.Height / 2);


            pressedKey1 = rootOverlay.settings.hotkey01;
            pressedKey2 = rootOverlay.settings.hotkey02;

            textbox_Hotkey.Text = pressedKey1.ToString() + " + " + pressedKey2.ToString();
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {            
            pressedKey1 = default;
            pressedKey2 = default;
            TextBox txtbox = sender as TextBox;
            txtbox.Text = "Press Desired Hotkeys";
            SetKeyboardHookEvents();            
        }         

        private void btn_Save_Click(object sender, EventArgs e)
        {            
            this.Close();
        }

        private void Save()
        {
            if (pressedKey1 != default)
            {
                rootOverlay.settings.hotkey01 = pressedKey1;
                rootOverlay.settings.hotkey02 = pressedKey2;
            }
            if (!String.IsNullOrEmpty(textbox_GoogleSheet.Text))
            {
                rootOverlay.settings.googleDocURL = textbox_GoogleSheet.Text;
            }
            rootOverlay.RootHookKeyboard();
        }
        
        void kbh_OnKeyPressed(object sender, Keys e)
        {
            if (pressedKey1 == default || pressedKey2 == default)
            {
                if (pressedKey1 == default)
                {     
                    pressedKey1 = e;
                    textbox_Hotkey.Text = pressedKey1.ToString();
                }
                else if (e != pressedKey1)
                {
                    pressedKey2 = e;
                    textbox_Hotkey.Text = pressedKey1.ToString() + " + " + pressedKey2.ToString();                    
                }
            }
        }

        void kbh_OnKeyUnpressed(object sender, Keys e)
        {
            btn_Save.Focus();
            UnSetKeyboardHookEvents();
        }

        private void SetKeyboardHookEvents()
        {
            kbh = new LowLevelKeyboardHook();
            kbh.OnKeyPressed += kbh_OnKeyPressed;
            kbh.OnKeyUnpressed += kbh_OnKeyUnpressed;
            kbh.HookKeyboard();
        }
        private void UnSetKeyboardHookEvents()
        {
            kbh.OnKeyPressed -= kbh_OnKeyPressed;
            kbh.OnKeyUnpressed -= kbh_OnKeyUnpressed;
            kbh.UnHookKeyboard();
            //kbh = null;
        }

        private void Form_Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save();
        }
    }
}
