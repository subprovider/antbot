
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ktds.Ant.Activities;
using System.Activities.Statements;
using System.Activities.Expressions;

namespace ktds.Ant.Activities
{

    [Designer(typeof(ImageDetectorActivityDesign))]
    public sealed class ImageDetectorActivity : CodeActivity
    {
        private int m_capacity = 95;
        private int m_retry = 1;
        private YesNoType mContinueOnError = YesNoType.No;
        private int mnDelayAfter = 0;  //mili-seconds
        private string msActionType = "None";
        private int mnOffsetX = -1, mnOffsetY = -1; //click position

        public InArgument<string> Text { get; set; }


        [Category("Common Option")]
        public YesNoType ContinueOnError { get { return mContinueOnError; } set { mContinueOnError = value; } }

        [Category("Common Option")]
        public int DelayAfter { get { return mnDelayAfter; } set { mnDelayAfter = value; } }


     [RequiredArgument]
        [Category("Detect Image Option")]
        public string ImageFileName { get; set; }

        [Category("Detect Image Option")]
        public int Capacity { get { return m_capacity; } set { m_capacity = value; } }

        [Category("Detect Image Option")]
        public int Retry { get { return m_retry; } set { m_retry = value; } }

     [DefaultValue(null)]

        [Category("Execution Option")]
        public string ActionType { get { return msActionType; }  set { msActionType = value; NotifyPropertyChanged("ActionType"); } }

        [Category("Execution Option")]
        public int OffsetX { get { return mnOffsetX; } set { mnOffsetX = value; NotifyPropertyChanged("OffsetX"); } }

        [Category("Execution Option")]
        public int OffsetY { get { return mnOffsetY; } set { mnOffsetY = value; NotifyPropertyChanged("OffsetY"); } }

        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }

        [Category("Output")]
        public OutArgument<int> OutImagePosLeft { get; set; }
        [Category("Output")]
        public OutArgument<int> OutImagePosTop { get; set; }
        [Category("Output")]
        public OutArgument<int> OutImagePosWidth { get; set; }
        [Category("Output")]
        public OutArgument<int> OutImagePosHeight { get; set; }


        public ImageDetectorActivity()
        {
            //Properties Editor 설정
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "Capacity",        new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "ImageFileName",   new EditorAttribute(typeof(FilePickerEditor), typeof(DialogPropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "Retry",           new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "OffsetX",         new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "OffsetY",         new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDetectorActivity), "DelayAfter",      new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));

            MetadataStore.AddAttributeTable(builder.CreateTable());
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


        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string text = context.GetValue(this.Text);

            Rectangle detectPosition = new Rectangle(0, 0, 0, 0);

            this.ResultBool.Set(context, false);

            // 이미지 검색
            Mat mat = CvInvoke.Imread(ImageFileName, Emgu.CV.CvEnum.ImreadModes.AnyColor);

            Image<Bgr, byte> source = new Image<Bgr, byte>(CaptureScreen());
            Image<Bgr, byte> template = mat.ToImage<Bgr, Byte>();

            int nRetry = 0; //이미지 검색 재시도 횟수
            do
            {
                using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if (maxValues[0] > (double)Capacity / 100) //0.9
                    {
                        detectPosition = new Rectangle(maxLocations[0], template.Size);

                        this.OutImagePosLeft.Set(context,   detectPosition.Left);
                        this.OutImagePosTop.Set(context,    detectPosition.Top);
                        this.OutImagePosWidth.Set(context,  detectPosition.Width);
                        this.OutImagePosHeight.Set(context, detectPosition.Height);

                        Debug.WriteLine(detectPosition.ToString());
                        //MessageBox.Show(detectPosition.ToString());

                        if (msActionType != "None" && msActionType != "")
                        {
                            //MessageBox.Show(String.Format("X : {0}, Y : {1}", detectPosition.Left + OffsetX, detectPosition.Top + OffsetY));
                            int nMousePosX = detectPosition.Left + OffsetX;
                            int nMousePosY = detectPosition.Top + OffsetY;

                            CommonUtil.MoveCursor(nMousePosX, nMousePosY);

                            if (msActionType == "LButtonClick")
                                CommonUtil.DoMouseClickEvent(CommonUtil.MouseButtonType.Left, nMousePosX, nMousePosY);

                            if (msActionType == "RButtonClick")
                                CommonUtil.DoMouseClickEvent(CommonUtil.MouseButtonType.Right, nMousePosX, nMousePosY);

                            if (msActionType == "MButtonClick")
                                CommonUtil.DoMouseClickEvent(CommonUtil.MouseButtonType.Middle, nMousePosX, nMousePosY);
                        }

                        break;
                    }
                }

                nRetry++;
                Thread.Sleep(100);

            } while (nRetry < Retry);

            //nRetry 값이 Retry 값보다 크면 Image 찾기 실패
            if (nRetry >= Retry)
                return;

            //Delay After 처리
            if (mnDelayAfter > 0)
            {
                Thread.Sleep(mnDelayAfter);
            }

            this.ResultBool.Set(context, true);
            /*
            Assign<bool> assign = new Assign<bool>
            {
                To = new ArgumentReference<bool>("ResultBool"),
                Value = new InArgument<bool>(true)
            };

            Sequence sequence = new Sequence();
            sequence.Activities.Add(assign);

            this.Implementation = () => sequence;
            */
        }

        private Bitmap CaptureScreen()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height);
            //PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

      
    } //class


}
