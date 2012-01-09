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
    public partial class InformationBox : Form
    {
        public InformationBox()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        public InformationBox(string text, string label) : this ()
        {
            this.infobox.AppendText(text);
            this.label1.Text = label;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.infobox.Text);
        }

        public void display(string text, string label)
        {
            this.infobox.AppendText(text);
            this.label1.Text = label;
            this.Visible = true;
        }

        public void AppendText(string text)
        {
            this.infobox.AppendText(text);
        }

        public string Label
        {
            get { return this.label1.Text;  }
            set { this.label1.Text = value; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                string filename = this.label1.Text.ToLower().Replace(" ", "_");

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Text File|*.txt";
                saveFileDialog1.FileName = DateTime.Now.ToString("yyyy-MM-dd") + "_" + filename + ".txt" ;
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog1.Title = "Save this information into file";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, this.infobox.Text);
                }
            }
        }
    }
}
