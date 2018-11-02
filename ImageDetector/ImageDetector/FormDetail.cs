using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ktds.Ant.Activities
{
    public partial class FormDetail : Form
    {

        private Bitmap imgSelectPos;

        private Rectangle rectSelectPos;
        private bool Dragging = false;
        private int OffsetX, OffsetY;

        public int X = 0, Y = 0;
        public string ActionType = "None";

        public string clickPosition { get; set; }
        public string imageFileName { get; set; }
        public bool result { get; set; }

        public FormDetail()
        {
            InitializeComponent();

            imageFileName = "";
            clickPosition = "";
            result = false;

            imgSelectPos = new Bitmap(Properties.Resources.cross);
            imgSelectPos.MakeTransparent(Color.White);

            rectSelectPos = new Rectangle(10, 10, imgSelectPos.Width, imgSelectPos.Height);

            X = -1;
            Y = -1;
        }


        public bool ShowModal(string sImageFileName, string sActionType, int initX, int initY)
        {
            imageFileName = sImageFileName;
            //clickPosition = sPos;
            cboAction.Text = sActionType;

            if (initX != -1)
                X = initX;

            if (initY != -1)
                Y = initY;


            this.ShowDialog();

            X = rectSelectPos.X + (rectSelectPos.Width / 2);
            Y = rectSelectPos.Y + (rectSelectPos.Height / 2);

            return result;
        }


        private void pictureBoxLocationChange()
        {
            pictureBox1.Left = (panel2.Width - pictureBox1.Width) / 2;
            if (pictureBox1.Left <= 0) pictureBox1.Left = 1;
            pictureBox1.Top = (panel2.Height - pictureBox1.Height) / 2;
            if (pictureBox1.Top <= 0) pictureBox1.Top = 1;
        }


        private void FromDetail_Load(object sender, EventArgs e)
        {
            if (imageFileName != "")
            {
                Image img = new Bitmap(imageFileName);
                pictureBox1.Load(imageFileName); // = img;
                pictureBox1.Width = img.Width;
                pictureBox1.Height = img.Height;

                pictureBoxLocationChange();

                if (X == -1)
                    rectSelectPos.X = (pictureBox1.Width / 2) - (rectSelectPos.Width / 2);
                else
                    rectSelectPos.X = X - (rectSelectPos.Width / 2);

                if (Y == -1)
                    rectSelectPos.Y = (pictureBox1.Height / 2) - (rectSelectPos.Height / 2);
                else
                    rectSelectPos.Y = Y - (rectSelectPos.Height / 2);


                Point pt = ValidationReplectPosition();


                label2.Text = String.Format("Image Width : {0,-5:N0}, Height : {1,-5:N0}    Click Position X : {2,-5:N0}, Y : {3,-5:N0}",
                    pictureBox1.Width, pictureBox1.Height, pt.X, pt.Y);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            result = true;


            X = rectSelectPos.X + (rectSelectPos.Width / 2);
            Y = rectSelectPos.Y + (rectSelectPos.Height / 2);
            ActionType = cboAction.Text;

            this.Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            result = false;
            this.Close();
        }

        private void cboPosition_SelectedValueChanged(object sender, EventArgs e)
        {
            clickPosition = cboAction.Text;

        }

        private bool PointIsOverPicture(int x, int y)
        {
            // See if it's over the picture's bounding rectangle.
            if ((x < rectSelectPos.Left) || (x >= rectSelectPos.Right) ||
                (y < rectSelectPos.Top) || (y >= rectSelectPos.Bottom))
                return false;

            // See if the point is above a non-transparent pixel.
            int i = x - rectSelectPos.X;
            int j = y - rectSelectPos.Y;
            return (imgSelectPos.GetPixel(i, j).A > 0);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(imgSelectPos, rectSelectPos);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // See if we're over the picture.
            if (PointIsOverPicture(e.X, e.Y))
            {
                // Start dragging.
                Dragging = true;

                // Save the offset from the mouse to the picture's origin.
                OffsetX = rectSelectPos.X - e.X;
                OffsetY = rectSelectPos.Y - e.Y;
            }
        }



        // Continue dragging the picture.
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // See if we're dragging.
            if (Dragging)
            {
                // We're dragging the image. Move it.
                rectSelectPos.X = e.X + OffsetX;
                rectSelectPos.Y = e.Y + OffsetY;

                if (rectSelectPos.X <= 0)
                    rectSelectPos.X = 0;

                if (rectSelectPos.Y <= 0)
                    rectSelectPos.Y = 0;

                // Redraw.
                pictureBox1.Invalidate();
            }
            else
            {
                // We're not dragging the image. See if we're over it.
                Cursor new_cursor = Cursors.Default;
                if (PointIsOverPicture(e.X, e.Y))
                {
                    new_cursor = Cursors.Hand;
                }

                
                if (pictureBox1.Cursor != new_cursor)
                    pictureBox1.Cursor = new_cursor;
                    
            }

            Point pt = ValidationReplectPosition();

            label2.Text = String.Format("Image Width : {0,5:N0}, Height : {1,5:N0}    Click Position X : {2,5:n0}, Y : {3,5:N0}",
                pictureBox1.Width, pictureBox1.Height, pt.X, pt.Y);
        }

  
        private Point ValidationReplectPosition()
        {

            if (rectSelectPos.X + (rectSelectPos.Width / 2) >= pictureBox1.Width)
            {
                rectSelectPos.X = pictureBox1.Width - (rectSelectPos.Width / 2);
            }

            if (rectSelectPos.Y + (rectSelectPos.Height / 2) >= pictureBox1.Height)
            {
                rectSelectPos.Y = pictureBox1.Height - (rectSelectPos.Height / 2);
            }

            Point pt = new Point(rectSelectPos.X + (rectSelectPos.Width / 2), rectSelectPos.Y + (rectSelectPos.Height / 2));

            return pt;
        }

        private void FormDetail_Resize(object sender, EventArgs e)
        {
            pictureBoxLocationChange();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Dragging = false;
        }

    }
}
