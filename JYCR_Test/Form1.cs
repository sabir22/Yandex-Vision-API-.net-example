using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Json.YC.Reader;

namespace JYCR_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string fText;
        string fName;
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            fName = openFileDialog1.FileName;
            if (!string.IsNullOrEmpty(fName))
            {
                textBox1.Text = fName;
                textBox2.Clear();
                fText = File.ReadAllText(fName, Encoding.UTF8);//reading text from json request
                textBox2.Text = fText;
            }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(fText))
            {
                textBox3.Clear();
                tLines = JYCR.getTextLines(fText);//parsing
                Size pSize = JYCR.getPageSize(fText);                
                foreach (var l in tLines)
                {
                    foreach (var w in l.Words)
                    {
                        textBox3.AppendText(w.Text + " ");
                    }
                    textBox3.AppendText(Environment.NewLine);
                }
                
            }
        }
        List<jLine> tLines = new List<jLine>();
        List<jBlock> tBlocks = new List<jBlock>();
        private void button3_Click(object sender, EventArgs e)//for async operation: private async void button3_Click... JYC.RycRecognizeImageASYNC(fNName)...
        {
            openFileDialog2.ShowDialog();
            fName = openFileDialog2.FileName;
            if (!string.IsNullOrEmpty(fName))
            {
                textBox1.Text = fName;
                textBox2.Clear();
                //simple steps - first get json response
                fText = JYCR.ycRecognizeImageSYNC(fName);//recognize image with YC 
                //secont step - convert response in objects 
                tBlocks = JYCR.getTextBlocks(fText);//for text blocks
                tLines = JYCR.getTextLines(fText);
                Size pSize = JYCR.getPageSize(fText);
                textBox2.Text = fText;
                textBox2.AppendText(Environment.NewLine + "Pagesize = " + pSize.ToString());
            } 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(fName))
            {
                var form2 = new Form2(fName, tLines, tBlocks.Count>0 ? tBlocks : null);//show form with image and text view
                form2.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Click(object sender, EventArgs e)
        {
            
        }
        
    }
}
