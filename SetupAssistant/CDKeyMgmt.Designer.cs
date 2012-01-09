namespace SetupAssistant
{
    partial class CDKeyMgmt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CDKeyMgmt));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.office2003btn = new System.Windows.Forms.Button();
            this.office2007btn = new System.Windows.Forms.Button();
            this.office2010btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(346, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "To remove the CD key associated with installed copy of Microsoft Office";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(186, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "please click one of the buttons below.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(342, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Run any office application in order to enter a new CD key and activate.";
            // 
            // office2003btn
            // 
            this.office2003btn.Enabled = false;
            this.office2003btn.Location = new System.Drawing.Point(15, 52);
            this.office2003btn.Name = "office2003btn";
            this.office2003btn.Size = new System.Drawing.Size(343, 30);
            this.office2003btn.TabIndex = 3;
            this.office2003btn.Text = "Remove Key for Office 2003";
            this.office2003btn.UseVisualStyleBackColor = true;
            this.office2003btn.Click += new System.EventHandler(this.office2003btn_Click);
            // 
            // office2007btn
            // 
            this.office2007btn.Enabled = false;
            this.office2007btn.Location = new System.Drawing.Point(15, 88);
            this.office2007btn.Name = "office2007btn";
            this.office2007btn.Size = new System.Drawing.Size(343, 30);
            this.office2007btn.TabIndex = 4;
            this.office2007btn.Text = "Remove Key for Office 2007";
            this.office2007btn.UseVisualStyleBackColor = true;
            this.office2007btn.Click += new System.EventHandler(this.office2007btn_Click);
            // 
            // office2010btn
            // 
            this.office2010btn.Enabled = false;
            this.office2010btn.Location = new System.Drawing.Point(15, 124);
            this.office2010btn.Name = "office2010btn";
            this.office2010btn.Size = new System.Drawing.Size(343, 30);
            this.office2010btn.TabIndex = 5;
            this.office2010btn.Text = "Remove Key for Office 2010";
            this.office2010btn.UseVisualStyleBackColor = true;
            this.office2010btn.Click += new System.EventHandler(this.office2010btn_Click);
            // 
            // CDKeyMgmt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 192);
            this.Controls.Add(this.office2010btn);
            this.Controls.Add(this.office2007btn);
            this.Controls.Add(this.office2003btn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CDKeyMgmt";
            this.Text = "CDKeyMgmt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button office2003btn;
        private System.Windows.Forms.Button office2007btn;
        private System.Windows.Forms.Button office2010btn;
    }
}