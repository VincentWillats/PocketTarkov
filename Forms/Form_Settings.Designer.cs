﻿namespace PocketTarkov
{
    partial class Form_Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textbox_GoogleSheet = new System.Windows.Forms.TextBox();
            this.textbox_Hotkey = new System.Windows.Forms.TextBox();
            this.btn_Save = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textbox_Hotkey02 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Google Sheet Link For Task Items";
            // 
            // textbox_GoogleSheet
            // 
            this.textbox_GoogleSheet.Location = new System.Drawing.Point(237, 12);
            this.textbox_GoogleSheet.Name = "textbox_GoogleSheet";
            this.textbox_GoogleSheet.Size = new System.Drawing.Size(162, 23);
            this.textbox_GoogleSheet.TabIndex = 1;
            // 
            // textbox_Hotkey
            // 
            this.textbox_Hotkey.Location = new System.Drawing.Point(237, 41);
            this.textbox_Hotkey.Name = "textbox_Hotkey";
            this.textbox_Hotkey.ReadOnly = true;
            this.textbox_Hotkey.Size = new System.Drawing.Size(162, 23);
            this.textbox_Hotkey.TabIndex = 2;
            this.textbox_Hotkey.Enter += new System.EventHandler(this.textBox2_Enter);
            this.textbox_Hotkey.Leave += new System.EventHandler(this.textbox_Hotkey_Leave);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(172, 105);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(91, 23);
            this.btn_Save.TabIndex = 3;
            this.btn_Save.Text = "Save Settings";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(51, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Show/Hide Hotkey Combination";
            // 
            // textbox_Hotkey02
            // 
            this.textbox_Hotkey02.Location = new System.Drawing.Point(237, 70);
            this.textbox_Hotkey02.Name = "textbox_Hotkey02";
            this.textbox_Hotkey02.ReadOnly = true;
            this.textbox_Hotkey02.Size = new System.Drawing.Size(162, 23);
            this.textbox_Hotkey02.TabIndex = 2;
            this.textbox_Hotkey02.Enter += new System.EventHandler(this.textBox3_Enter);
            this.textbox_Hotkey02.Leave += new System.EventHandler(this.textbox_Hotkey_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(224, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Interactable  Toggle Hotkey Combination";
            // 
            // Form_Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 140);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textbox_Hotkey02);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.textbox_Hotkey);
            this.Controls.Add(this.textbox_GoogleSheet);
            this.Controls.Add(this.label1);
            this.Name = "Form_Settings";
            this.Text = "C";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Settings_FormClosing);
            this.Load += new System.EventHandler(this.Form_Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textbox_GoogleSheet;
        private System.Windows.Forms.TextBox textbox_Hotkey;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textbox_Hotkey02;
    }
}