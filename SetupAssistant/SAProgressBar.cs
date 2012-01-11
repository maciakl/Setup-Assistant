using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SetupAssistant
{
    

    public partial class SAProgressBar : Form
    {
        private Form parent;

        public SAProgressBar(Form parent)
        {
            InitializeComponent();

            this.parent = parent;


            this.Location = new Point(parent.Location.X + 150, parent.Location.Y + 150);
            this.Update();

            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 100;

        }

        public void reposition(Point p)
        {
            this.Location = p;
            this.Update();
        }

        public string ProgressMessage 
        {
            get { return this.progressMessage.Text; }
            set { this.progressMessage.Text = value; }
        }

        public ProgressBar Bar
        {
            get { return this.progressBar1; }
        }

        public void progressUpdate(string message, int value)
        {
            this.progressMessage.Text = message;
            this.progressBar1.Value = value;
            this.progressMessage.Update();

            this.Location = new Point(parent.Location.X+150, parent.Location.Y+150);
            this.progressBar1.Update();
            
        }

        public void progressUpdate(string message)
        {
            progressUpdate(message, progressBar1.Step);
        }
    }
}
