using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Notes:
//  - The Canvas must have a non-null background to make it generate mouse events.

namespace ktds.Ant.Activities
{

    public partial class FormCapture : Window, INotifyPropertyChanged
    {
        public bool Captured { get; set; }
        public string CaptureImageFileName { get; set; }

        private System.Drawing.Point _point;

        private readonly double _screenWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _screenHeight = SystemParameters.PrimaryScreenHeight;

        // True if a drag is in progress.
        private bool _dragInProgress = false;

        // The drag's last point.
        private System.Drawing.Point _lastPoint;

        // The part of the rectangle under the mouse.
        private HitType _mouseHitType = HitType.None;

        public System.Drawing.Point RectPoint
        {
            get { return this._point; }
            private set
            {
                this._point = value;
                this.OnPropertyChanged( "RectPoint" );
            }
        }

        public double AreaWidth { get; private set; }
        public double AreaHeight { get; private set; }

        public FormCapture()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        // The part of the rectangle the mouse is over.
        private enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };

   


        private void Form_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }



        private void Form_ScreenCapture(object sender, RoutedEventArgs e)
        {
            //this.Background.Opacity = 0.0;

            System.Drawing.Size sz   = new System.Drawing.Size((int)(ActualWidth - 4), (int)(ActualHeight - 25 - 4));
            System.Drawing.Point loc1 = new System.Drawing.Point((int)Left + 2, (int)Top + 25 + 2);
            System.Drawing.Point loc2 = new System.Drawing.Point((int)(Left + captureArea.Width -2), (int)(Top + captureArea.Height-2));

     

            this.Hide();
            Bitmap capturedImage = CaptureScreen(loc1, loc2, sz);
            this.Show();

            SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.DefaultExt = "bmp";
            saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|jpg files (*.jpg)|*.jpg|gif files (*.gif)|*.gif|tiff files (*.tiff)|*.tiff|png files (*.png)|*.png";
            saveFileDialog.Title = "Save screenshot to...";
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Captured = false;
                CaptureImageFileName = "";
                return;
            }

            Captured = true;

            CaptureImageFileName = saveFileDialog.FileName;
            string extension = new FileInfo(CaptureImageFileName).Extension;

            switch (extension)
            {
                case ".bmp":
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Bmp);
                    break;
                case ".jpg":
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Jpeg);
                    break;
                case ".gif":
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Gif);
                    break;
                case ".tiff":
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Tiff);
                    break;
                case ".png":
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Png);
                    break;
                default:
                    capturedImage.Save(CaptureImageFileName, ImageFormat.Bmp);
                    break;
            }

            this.Close();
            //this.Background.Opacity = 0.1;
        }


        private void ButtonBase_OnClick( object sender, RoutedEventArgs e )
        {
            Close();
        }

        #region Inotify
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion


        public static Bitmap CaptureScreen(System.Drawing.Point pt1, 
                                           System.Drawing.Point pt2,
                                           System.Drawing.Size  size)
        {
            using (Bitmap bitmap = new Bitmap(size.Width, size.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(pt1, new System.Drawing.Point(0, 0), size);
                }

                Bitmap img = new Bitmap(bitmap);  // (Image)bitmap;
                return img;
            }
        }

        public string ClickPosText { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
