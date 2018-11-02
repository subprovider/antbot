using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;





namespace ktds.Ant.Activities
{
    // ImageDetectorActivityDesign.xaml에 대한 상호 작용 논리
    public partial class ImageDetectorActivityDesign
    {
        public ImageDetectorActivityDesign()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), new DesignerAttribute(typeof(ImageDetectorActivityDesign)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), new DescriptionAttribute("ktds ubot's ImageDetectorActivity"));
        }


        public void btnCaptureClick(object sender, RoutedEventArgs e)
        {

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Hide();

            FormCapture formCapture = new FormCapture();
            formCapture.ShowDialog();

            if (!formCapture.Captured)
            {
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.Show();

                return;
            }

            this.ModelItem.Properties["ImageFileName"].SetValue(formCapture.CaptureImageFileName);

            //Image img = (Image)(sender as Button).FindName("targetImage");
            //img.Source = new BitmapImage(new Uri(formCapture.CaptureImageFileName, UriKind.Relative));

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Show();

        }
 

        #region Inotify
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            
            FormDetail frmDetail = new FormDetail();
            bool result = frmDetail.ShowModal(this.ModelItem.Properties["ImageFileName"].Value.ToString(),
                                              this.ModelItem.Properties["ActionType"].Value.ToString(), 
                                              int.Parse(this.ModelItem.Properties["OffsetX"].Value.ToString()),
                                              int.Parse(this.ModelItem.Properties["OffsetY"].Value.ToString()));

            if (result)  // clicked cancel button 
            {
                this.ModelItem.Properties["OffsetX"].SetValue(frmDetail.X);
                this.ModelItem.Properties["OffsetY"].SetValue(frmDetail.Y);
                this.ModelItem.Properties["ActionType"].SetValue(frmDetail.ActionType);
            }

            frmDetail.Dispose();
        }


        public class StringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                ModelItem modelItem = value as ModelItem;

                if (value != null)
                {
                    InArgument<string> inArgument = modelItem.GetCurrentValue() as InArgument<string>;

                    if (inArgument != null && inArgument.Expression as Literal<string> != null)
                    {
                        return (inArgument.Expression as Literal<string>).Value;
                    }
                }

                return string.Empty;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value != null)
                {
                    return new InArgument<string>(new Literal<string>(value as string));
                }

                return null;
            }
        }
    }


 
}
