using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetEditor
{
    public partial class SelectFrameDialog : Form
    {
        private int _tileX, _tileY;

        public int SelectedIndex { get; set; }

        public SelectFrameDialog()
        {
            InitializeComponent();
        }

        private void SelectFrameDialog_Load(object sender, EventArgs e)
        {
            Height = (int)pictureBox1.Image.PhysicalDimension.Height + 50;
            Width = (int)pictureBox1.Image.PhysicalDimension.Width + 20;

            SelectedIndex = -1;

            //pictureBox2.Width = Width;
            //pictureBox2.Height = Height;
        }

        public void SetImage(Image img, int tilesX, int tilesY)
        {
            pictureBox1.Image = img;
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Parent = pictureBox1;

            _tileX = tilesX;
            _tileY = tilesY;

            Bitmap bmp = new Bitmap((int)img.PhysicalDimension.Width, (int)img.PhysicalDimension.Height);
            Graphics graph = Graphics.FromImage(bmp);
            for (var y = 0; y < bmp.PhysicalDimension.Height; y += 2)
            {
                for (var j = 0; j < tilesX; j++)
                {
                    bmp.SetPixel((int)((bmp.PhysicalDimension.Width / tilesX) * j), y, Color.Red);
                }
            }
            for (var x = 0; x < bmp.PhysicalDimension.Width; x += 3)
            {
                for (var j = 0; j < tilesY; j++)
                {
                    bmp.SetPixel(x, (int)((bmp.PhysicalDimension.Height / tilesY) * j), Color.Red);
                }
            }

            int index = 0;
            var f = new Font(Font.FontFamily, (float)10.0, FontStyle.Bold);
            

            for (var y = img.PhysicalDimension.Height / 2 / tilesY; y < img.PhysicalDimension.Height; y += img.PhysicalDimension.Height / tilesY)
            {
                for (var x = img.PhysicalDimension.Width / 2 / tilesX; x < img.PhysicalDimension.Width; x += img.PhysicalDimension.Width / tilesX)
                {
                    graph.DrawString(index.ToString(), f, Brushes.White, x + 1, y + 1);
                    graph.DrawString(index.ToString(), f, Brushes.White, x + 1, y - 1);
                    graph.DrawString(index.ToString(), f, Brushes.White, x - 1, y + 1);
                    graph.DrawString(index.ToString(), f, Brushes.White, x - 1, y - 1);
                    graph.DrawString(index.ToString(), f, Brushes.DarkBlue, x, y);
                    index++;
                }
            }
            pictureBox2.Image = bmp;
        }

        private void SelectFrameDialog_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Visible = true;
        }

        private void SelectFrameDialog_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
        }

        private void SelectFrameDialog_MouseHover(object sender, EventArgs e)
        {
            pictureBox2.Visible = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Point cp = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            SelectedIndex = 0;
            for (var y = 1;y <= _tileY; y++)
            {
                for (var x = 1; x <= _tileX; x++)
                {
                    if(cp.X < x * (Width / _tileX) && cp.Y < y * (Height / _tileY))
                    {
                        DialogResult = DialogResult.OK;
                        Hide();
                        return;
                    }
                    SelectedIndex++;
                }
            }
            SelectedIndex = -1;
        }
    }
}
