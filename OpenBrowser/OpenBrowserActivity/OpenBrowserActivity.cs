using System;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;

namespace ktds.Ant.Activities
{

    using HWND = IntPtr;
    public sealed class OpenBrowserActivity : CodeActivity
    {
        private InternetBrowserType mBrowserType = InternetBrowserType.IE;
        private string msURL = "";
        private HWND mnHandle = IntPtr.Zero;
        private YesNoType mContinueOnError = YesNoType.No;
        private WindowStateType mWindowState = WindowStateType.Normal;

        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        [Category("Common Option")]
        public YesNoType ContinueOnError { get { return mContinueOnError; }  set { mContinueOnError = value; NotifyPropertyChanged("ContinueOnError"); } }


        [Category("Browse Info")]
        public InternetBrowserType BrowserType { get { return mBrowserType; } set { mBrowserType = value; NotifyPropertyChanged("BrowserType"); } }  //IE, Chrome, Firefox

        [RequiredArgument]
        [Category("Browse Info")]
        public string URL { get { return msURL; } set { msURL = value; NotifyPropertyChanged("URL"); } }

        [Category("Browse Info")]
        // [Editor(typeof(CustomComboBoxWindowStateEditor), typeof(ExtendedPropertyValueEditor))]
        public WindowStateType WindowState { get { return mWindowState; } set { mWindowState = value; NotifyPropertyChanged("WindowState"); } }


        [DefaultValue(null)]
        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }
        [Category("Output")]
        public OutArgument<HWND> ResultHandle { get; set; }

        public enum WindowStateType
        {
            Normal = 0,
            Maxmize = 1,
            Minimize = 2
        }


        
        public enum InternetBrowserType
        {
            IE = 0,
            Chrome = 1
        }


        public OpenBrowserActivity()
        {
            //Properties Editor 설정
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(OpenBrowserActivity), "URL", new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            //builder.AddCustomAttributes(typeof(OpenBrowserActivity), "ContinueOnError", new EditorAttribute(typeof(CustomInlineComboBoxYesNo), typeof(PropertyValueEditor)));
            //builder.AddCustomAttributes(typeof(OpenBrowserActivity), "BrowserType", new EditorAttribute(typeof(CustomInlineComboBoxBrowserType), typeof(PropertyValueEditor)));
            //builder.AddCustomAttributes(typeof(OpenBrowserActivity), "WindowState", new EditorAttribute(typeof(CustomInlineComboBoxWindowState), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(OpenBrowserActivity), "DelayAfter", new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));

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

        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context)
        {
            // 텍스트 입력 인수의 런타임 값을 가져옵니다.
            string text = context.GetValue(this.Text);

            object sUrl = msURL;
             
            if(mBrowserType == InternetBrowserType.IE)
            {
                SHDocVw.InternetExplorer IE;

                IE = new SHDocVw.InternetExplorer();

                object Empty = new object();

                IE.Visible = true;
                IE.Left = 0;
                IE.Top = 0;

                if(mWindowState == WindowStateType.Maxmize)
                    CommonUtil.ShowWindow((IntPtr)IE.HWND, (int)CommonUtil.ShowWindowType.SW_MAXIMIZE);

                if(mWindowState == WindowStateType.Minimize)
                    CommonUtil.ShowWindow((IntPtr)IE.HWND, (int)CommonUtil.ShowWindowType.SW_MINIMIZE);

                IE.Navigate2(ref sUrl);

                while (IE.Busy == true || IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {
                    System.Threading.Thread.Sleep(100);
                }


                //output
                this.ResultHandle.Set(context, (IntPtr)IE.HWND);
                this.ResultBool.Set(context, true);
            }
            else
            {
                MessageBox.Show("Not support");
            }
        }
    }
}
