
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ktds.Ant.Activities
{
    // UIAutoActivityDesigner.xaml에 대한 상호 작용 논리

    using HWND = IntPtr;
    public partial class UIAutoActivityDesigner
    {
        
        private string mImageFileName;
        private WindowState mPrevWindowState;

        ElementInfo mElementInfo;

        public string ImageFileName { get { return mImageFileName; } set { mImageFileName = value ; NotifyPropertyChanged("ImageFileName"); } }


        public UIAutoActivityDesigner()
        {
            InitializeComponent();

        }
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(UIAutoActivity), new DesignerAttribute(typeof(UIAutoActivityDesigner)));
            builder.AddCustomAttributes(typeof(UIAutoActivity), new DescriptionAttribute("ktds Ant's Activity"));
        }

        #region Inotify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                mPrevWindowState = Application.Current.MainWindow.WindowState;
                Application.Current.MainWindow.WindowState = WindowState.Minimized; // Hide();
            }

            MouseHook.MouseClicked += new EventHandler<MouseEventExtArgs>(MouseClickEventHook);
            MouseHook.MouseDblClicked += new EventHandler<MouseEventExtArgs>(MouseDblClickEventHook);
            MouseHook.StoppedHookEvent += new EventHandler(StoppedEventHookHandler);

            MouseHook.Start();
        }

         private void StoppedEventHookHandler(object sender, EventArgs e)
        {
            MouseHook.MouseClicked -= MouseClickEventHook;
            MouseHook.MouseDblClicked -= MouseDblClickEventHook;
            MouseHook.StoppedHookEvent -= StoppedEventHookHandler;

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = mPrevWindowState;
        }

        private void MouseClickEventHook(object sender, MouseEventExtArgs e)
        {
            Debug.WriteLine("MouseClickEventHook");
            System.Windows.Forms.MouseButtons  clickedButton = e.Button;
            MouseClickType clickedType =  (MouseClickType) e.Clicks ;

            this.ModelItem.Properties["MouseButton"].SetValue(clickedButton);
            this.ModelItem.Properties["MouseClickAction"].SetValue(clickedType);

            if(MouseHook.Element != null)
            {
                SetElementInfo();
            }
        }

        private void MouseDblClickEventHook(object sender, MouseEventExtArgs e)
        {
            System.Windows.Forms.MouseButtons clickedButton = e.Button;
            MouseClickType clickedType = (MouseClickType)e.Clicks;

            this.ModelItem.Properties["MouseButton"].SetValue(clickedButton);
            this.ModelItem.Properties["MouseClickAction"].SetValue(clickedType);

            MouseHook.stop();
        }


        private void SetElementInfo()
        {
            Debug.WriteLine("SetElementInfo");

            string sControlName = "";
            string sImageString = "";

            sControlName = MouseHook.Element.GetAttributeValue("Name");
            this.ModelItem.Properties["ControlName"].SetValue(sControlName);
            this.ModelItem.Properties["AutomationId"].SetValue(MouseHook.Element.GetAttributeValue("AutomationId"));
            this.ModelItem.Properties["ControlType"].SetValue(MouseHook.Element.GetAttributeValue("ControlType"));
            this.ModelItem.Properties["ControlTypeId"].SetValue(MouseHook.Element.GetAttributeValue("ControlTypeId"));
            this.ModelItem.Properties["PatternList"].SetValue(MouseHook.Element.PatternList);
            this.ModelItem.Properties["AttrIsList"].SetValue(MouseHook.Element.AttrIsList);

            this.ModelItem.Properties["ProcessName"].SetValue(MouseHook.Element.ProcessName);
            this.ModelItem.Properties["WindowTitle"].SetValue(MouseHook.Element.WindowCaption);

            if (MouseHook.Element.ElementImage != null)
            {
                this.ModelItem.Properties["ImageName"].SetValue(sControlName);

                //this.ModelItem.Properties["Image"].SetValue(MouseHook.Element.ElementImage);
                //bmpImage = BitmapToBitmapSource(img);

                string sImageDir = AppDomain.CurrentDomain.BaseDirectory + "workimage";
                Debug.WriteLine(sImageDir);
                Directory.CreateDirectory(sImageDir);

                string sTempControlName = sControlName;
                if (sTempControlName.Length > 20)
                    sTempControlName = sTempControlName.Substring(1, 20) + "...";

                sTempControlName = sanitizeFilename(sTempControlName);

                string sFileName = sImageDir + "\\" + GetSaveFileName(sImageDir, sTempControlName + ".bmp");
                MouseHook.Element.ElementImage.Save(sFileName, ImageFormat.Bmp);

                this.ModelItem.Properties["ImageFileName"].SetValue(sFileName);

                TypeConverter converter = TypeDescriptor.GetConverter(MouseHook.Element.ElementImage.GetType());
                sImageString = Convert.ToBase64String((Byte[])converter.ConvertTo(MouseHook.Element.ElementImage, typeof(Byte[])));
                this.ModelItem.Properties["ImageString"].SetValue(sImageString);
            }
        }



        public string GetSaveFileName(String dirPath, String fileN)
        {
            string fileName = fileN;

            if (fileN.Length > 0)
            {
                int indexOfDot = fileName.LastIndexOf(".");
                string strName = fileName.Substring(0, indexOfDot);
                string strExt = fileName.Substring(indexOfDot);

                bool bExist = true;
                int fileCount = 0;

                string dirMapPath = string.Empty;

                while (bExist)
                {
                    dirMapPath = dirPath;
                    string pathCombine = System.IO.Path.Combine(dirMapPath, fileName);

                    if (System.IO.File.Exists(pathCombine))
                    {
                        fileCount++;
                        fileName = strName + "(" + fileCount + ")" + strExt;
                    }
                    else
                    {
                        bExist = false;
                    }
                }
            }

            return fileName;
        }



        public System.Drawing.Bitmap GetImageByString()
        {
            string sImageString = ModelItem.Properties["ImageString"].Value.ToString();

            if (sImageString != "")
                return (new Bitmap(new MemoryStream(Convert.FromBase64String(sImageString))));

            return null;
        }

        public static String sanitizeFilename(String name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            string invalidCharsRemoved = new string(name
              .Where(x => !invalidChars.Contains(x))
              .ToArray());
            return invalidCharsRemoved;
        }

        //================================================ mouse hook =================================================================

        public static class MouseHook
        {
            public static event EventHandler<MouseEventExtArgs> MouseClicked = delegate { };
            public static event EventHandler<MouseEventExtArgs> MouseDblClicked = delegate { };
            public static event EventHandler StoppedHookEvent = delegate { };

            private static System.Windows.Forms.MouseButtons prevClickedButton;
            private static System.Windows.Forms.MouseButtons leastClickedButton;
            private static System.Drawing.Point leastPoint = new System.Drawing.Point();

            private static bool isFirstClick = true;
            private static bool isDoubleClick = false;
            private static int milliseconds = 0;
            private static int mInterval = 100;

            private static MouseEventExtArgs mouseEventArgs;

            private static System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();

            private static ElementInfo m_Elementinfo;
            public static ElementInfo Element { get { return m_Elementinfo; } set { m_Elementinfo = value; } }

            /*
            public static string mElementName;
            public static Image mElementImage;
            public static string mElementValue;
            public static string mElementText;
            public static string mActionPattern;
            */

            public static void Start()
            {
                Debug.WriteLine("start hook");

                _hookID = SetHook(_proc);

                prevClickedButton = System.Windows.Forms.MouseButtons.None;
                leastClickedButton = System.Windows.Forms.MouseButtons.None;

                doubleClickTimer.Interval = mInterval;
                doubleClickTimer.Tick +=
                    new EventHandler(doubleClickTimer_Tick);

            }
            public static void stop()
            {
                Debug.WriteLine("stop hook");

                if (doubleClickTimer != null)
                {
                    doubleClickTimer.Stop();
                    doubleClickTimer.Tick -= doubleClickTimer_Tick;
                }

                UnhookWindowsHookEx(_hookID);

                StoppedHookEvent(null, new EventArgs());
            }


            private static void doubleClickTimer_Tick(object sender, EventArgs e)
            {
                milliseconds += mInterval;
                Debug.WriteLine("timer tick");

                // The timer has reached the double click time limit.
                if (milliseconds >= System.Windows.Forms.SystemInformation.DoubleClickTime)
                {
                    doubleClickTimer.Stop();

                    // Allow the MouseDown event handler to process clicks again.
                    isFirstClick = true;
                    isDoubleClick = false;
                    milliseconds = 0;

                    stop();
                }
            }


            private static LowLevelMouseProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;

            private static IntPtr SetHook(LowLevelMouseProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc, 
                      GetModuleHandle(curModule.ModuleName), 0);   //WH_MOUSE_LL
                }
            }

            private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

            private struct MouseLLHookStruct
            {
                public System.Drawing.Point Point;
                public int MouseData;
                public int Flags;
                public int Time;
                public int ExtraInfo;
            }


            private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                int nClickCount = 0;

                //if (nCode >= 0 && (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam || MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam))
                if (nCode >= 0 && (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam
                                || MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
                                //|| MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
                                )
                {
                    MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                    leastPoint.X = mouseHookStruct.Point.X;
                    leastPoint.Y = mouseHookStruct.Point.Y;

                    if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                        leastClickedButton = System.Windows.Forms.MouseButtons.Left;

                    if (MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
                        leastClickedButton = System.Windows.Forms.MouseButtons.Right;

                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    short mouseDelta = 0;

                    if (isFirstClick)
                    {
                        isFirstClick = false;
                        isDoubleClick = false;
                        nClickCount = 1;
                        doubleClickTimer.Start();
                    }
                    else
                    {
                        if (milliseconds <= System.Windows.Forms.SystemInformation.DoubleClickTime)
                        {
                            isDoubleClick = true;
                            nClickCount = 2;

                            doubleClickTimer.Stop();
                        }
                        isFirstClick = true;
                    }

                    MouseEventExtArgs mouseArgs = new MouseEventExtArgs(leastClickedButton,
                                                                        nClickCount,  //single CLICK
                                                                        leastPoint.X,
                                                                        leastPoint.Y,
                                                                        0);

                    if (nClickCount == 1) //single click
                    {
                        //별도 쓰레드로 분리 EventHook과 UIAutomation 동일 Thread 사용시 오류 발생
                        //var tasks = Task.Factory.StartNew(GetElement);
                        var tasks = Task.Factory.StartNew(() => GetElementInfo(leastPoint.X, leastPoint.Y));
                        Task.WaitAll(tasks);
                    }


                    if (isDoubleClick)
                    {
                        Debug.WriteLine("Hook Double Clicked");
                        MouseDblClicked(null, mouseArgs);
                    }
                    else
                    {
                        Debug.WriteLine("Hook Single Clicked");
                        
                        MouseClicked(null, mouseArgs);
                    }
                }

                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            private static void GetElementInfo(int x, int y)
            {
                m_Elementinfo = new ElementInfo(x, y);
            }


            public static void PrintExceptionLog(Exception ex)
            {
                Debug.WriteLine("\nMessage ---\n{0}", ex.Message);
                Debug.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
                Debug.WriteLine("\nSource ---\n{0}", ex.Source);
                Debug.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
                Debug.WriteLine("\nTargetSite ---\n{0}", ex.TargetSite);
            }

            static internal Rectangle GetElementRect(AutomationElement uiElement)
            {
                System.Windows.Rect wrect = uiElement.Current.BoundingRectangle;
                return new Rectangle((int)wrect.Left, (int)wrect.Top, (int)wrect.Width, (int)wrect.Height);
            }

            public static System.Drawing.Bitmap GetImage(AutomationElement uiElement)
            {
                System.Drawing.Bitmap uiElementImage;
                try
                {
                    System.Drawing.Rectangle rect = GetElementRect(uiElement);
                    if (rect.Width <= 0 || rect.Height <= 0)
                    {
                        Debug.WriteLine("element rectangle is 0");
                        return null;
                    }

                    uiElementImage = new Bitmap(rect.Width, rect.Height);
                    Graphics g = Graphics.FromImage(uiElementImage);
                    g.CopyFromScreen(rect.Location, new System.Drawing.Point(0, 0), rect.Size);
                    g.Save();
                }
                catch (Exception ex)
                {
                    PrintExceptionLog(ex);
                    return null;
                }

                return uiElementImage;
            }


            private const int WH_MOUSE_LL = 14;

            private enum MouseMessages
            {
                WM_MOUSEMOVE = 0x200,
                WM_LBUTTONDOWN = 0x201,
                WM_RBUTTONDOWN = 0x204,
                WM_MBUTTONDOWN = 0x207,
                WM_LBUTTONUP = 0x202,
                WM_RBUTTONUP = 0x205,
                WM_MBUTTONUP = 0x208,
                WM_LBUTTONDBLCLK = 0x203,
                WM_RBUTTONDBLCLK = 0x206,
                WM_MBUTTONDBLCLK = 0x209,
                WM_MOUSEWHEEL = 0x020A
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MSLLHOOKSTRUCT
            {
                public POINT pt;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook,
              LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
              IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);


        }

    }
}


namespace ktds.Ant.Converter
{

    [ValueConversion(typeof(System.Drawing.Bitmap), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }


            //var image = value as System.Drawing.Bitmap;

            System.Drawing.Bitmap image =  (System.Drawing.Bitmap)value ;

            if (image == null)
                return null;

            // Winforms Image we want to get the WPF Image from...
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            MemoryStream memoryStream = new MemoryStream();
            // Save to a memory stream...
            image.Save(memoryStream, ImageFormat.Bmp);
            // Rewind the stream...
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
     
 
}



/**************************************************************************************************************************
 * 
        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public BitmapImage ConvertBitmapToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public  BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }



        public static System.Windows.Media.Imaging.BitmapImage BitmapToBitmapSource(System.Drawing.Bitmap source)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                source.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
        **********************************************************************************************************/
