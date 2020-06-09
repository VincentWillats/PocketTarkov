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

        string whatHoket;
        Keys pressedKey1;
        Keys pressedKey2;
        Keys pressedKey3;
        Keys pressedKey4;

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
            pressedKey3 = rootOverlay.settings.hotkey03;
            pressedKey4 = rootOverlay.settings.hotkey04;

            textbox_Hotkey.Text = pressedKey1.ToString() + " + " + pressedKey2.ToString();
            textbox_Hotkey02.Text = pressedKey3.ToString() + " + " + pressedKey4.ToString();
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            whatHoket = "show/hide";
            pressedKey1 = default;
            pressedKey2 = default;
            TextBox txtbox = sender as TextBox;
            txtbox.Text = "Press Desired Hotkeys";
            SetKeyboardHookEvents();            
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            whatHoket = "interactable/not";
            pressedKey3 = default;
            pressedKey4 = default;
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
            if (pressedKey3 != default)
            {
                rootOverlay.settings.hotkey03 = pressedKey3;
                rootOverlay.settings.hotkey04 = pressedKey4;
            }
            if (!String.IsNullOrEmpty(textbox_GoogleSheet.Text))
            {
                rootOverlay.settings.googleDocURL = textbox_GoogleSheet.Text;
            }
            rootOverlay.RootHookKeyboard();
        }
        
        void kbh_OnKeyPressed(object sender, Keys e)
        {
            if(whatHoket == "show/hide")
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
            else if (whatHoket == "interactable/not")
            {
                if (pressedKey3 == default || pressedKey4 == default)
                {
                    if (pressedKey3 == default)
                    {
                        pressedKey3 = e;
                        textbox_Hotkey02.Text = pressedKey3.ToString();
                    }
                    else if (e != pressedKey3)
                    {
                        pressedKey4 = e;
                        textbox_Hotkey02.Text = pressedKey3.ToString() + " + " + pressedKey4.ToString();
                    }
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
        }

        private void Form_Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save();
        }

        private void textbox_Hotkey_Leave(object sender, EventArgs e)
        {
            whatHoket = "";
        }
    }
}
