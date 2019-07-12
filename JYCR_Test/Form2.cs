using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Json.YC.Reader;

namespace JYCR_Test
{
    public partial class Form2 : Form
    {
        List<jLine> _tLines;
        public Form2(string fName, List<jLine> tLines, List<jBlock> blocks = null)
        {
            InitializeComponent();
            using (Bitmap bmp = new Bitmap(fName))
            {
                pictureBox1.Image = bmp.Clone(new Rectangle(new Point(0, 0), bmp.Size), PixelFormat.Format24bppRgb); 
                pictureBox1.BackgroundImage = pictureBox1.Image;
            }
           
            _tLines = tLines;
            using (Graphics gr = Graphics.FromImage(pictureBox1.Image))
            {
                if (blocks!=null)
                {
                    foreach (var b in blocks)
                    {
                        using (Pen thick_pen = new Pen(Color.Green, 3))
                        {
                            Rectangle rect = b.Rect;
                            rect.Inflate(3, 3);
                            gr.DrawRectangle(thick_pen, rect);//draw text block (if needed)
                        }
                    }
                }
                foreach (var tL in tLines)
                {
                    using (Pen thick_pen = new Pen(Color.Blue, 2))
                    {
                        gr.DrawRectangle(thick_pen, tL.Rect);//draw text line rectangle
                    }
                    foreach (var w in tL.Words)
                    {
                        using (Pen thick_pen = new Pen(Color.Red, 1))
                        {
                            gr.DrawRectangle(thick_pen, w.Rect);//draw word rectangle
                        }
                    }
                }
            }
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            panel1.Width = this.Width - 41;
            panel1.Height = this.Height - 87;
            if (this.Height < 550) this.Height = 550;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            Point p = new Point(x, y);
            if (_tLines.Count>0)
            {

                jLine tLine = _tLines.FirstOrDefault(t => t.Rect.Contains(p));//point to line rectangle
                if (tLine != null)
                {
                    toolStripStatusLabel1.Text = tLine.Text;
                    jWord word = tLine.Words.FirstOrDefault(w => w.Rect.Contains(p));//point to word rectangle
                    if (word != null)
                    {
                        toolStripStatusLabel2.Text = word.Text;
                    }
                }
                else
                {
                    toolStripStatusLabel1.Text = "";
                    toolStripStatusLabel2.Text = "";
                }
            }
            toolStripStatusLabel4.Text = p.ToString();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {           
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X, y = e.Y;
            Point p = new Point(x, y);
            if (_tLines.Count > 0)
            {
                //draw rectangle over selected word
                jLine tLine = _tLines.FirstOrDefault(t => t.Rect.Contains(p));
                if (e.Button==MouseButtons.Right)
                {
                    Clipboard.SetText(tLine.Text);
                }
                if (tLine != null)
                {
                    jWord word = tLine.Words.FirstOrDefault(w => w.Rect.Contains(p));
                    if (word != null)
                    {
                        //draw picture over background original
                        using (Bitmap bmp = new Bitmap(pictureBox1.BackgroundImage.Width, pictureBox1.BackgroundImage.Height))
                        {
                            bmp.MakeTransparent();//less memory
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                using (Pen pen = new Pen(Color.Green, 4))
                                {
                                    g.DrawRectangle(pen, word.Rect);//word rectangle
                                }
                            }
                            pictureBox1.Image = (Bitmap)bmp.Clone();
                        }
                        toolStripStatusLabel3.Text = word.Text;
                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel3.Text = "";
            toolStripStatusLabel4.Text = "";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
