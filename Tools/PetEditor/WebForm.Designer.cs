﻿namespace PetEditor
{
    partial class WebForm
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
            this.webView1 = new Microsoft.Toolkit.Forms.UI.Controls.WebView();
            ((System.ComponentModel.ISupportInitialize)(this.webView1)).BeginInit();
            this.SuspendLayout();
            // 
            // webView1
            // 
            this.webView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView1.Location = new System.Drawing.Point(0, 0);
            this.webView1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webView1.Name = "webView1";
            this.webView1.Size = new System.Drawing.Size(800, 450);
            this.webView1.Source = new System.Uri("http://www.e-chat.co/room/19829319", System.UriKind.Absolute);
            this.webView1.TabIndex = 0;
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WebForm";
            this.Text = "Web Community";
            this.Load += new System.EventHandler(this.WebForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Toolkit.Forms.UI.Controls.WebView webView1;
    }
}