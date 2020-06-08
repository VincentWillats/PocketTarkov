namespace PocketTarkov
{
    partial class Form_RootOverlay
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
            this.SuspendLayout();
            // 
            // Form_RootOverlay
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Form_RootOverlay";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_RootOverlay_FormClosing);
            this.Load += new System.EventHandler(this.Form_RootOverlay_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}