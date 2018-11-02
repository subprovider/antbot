using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace ktds.Ant.Activities
{
    using HWND = IntPtr;
    public sealed class MaxmizeWindowActivity : CodeActivity
    {
        private string m_ProcessName = "";
        private string m_WindowTitle = "";

        private int m_RetrySec = 30;  //default 30sec

        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        [Category("Window Info")]
        public InArgument<HWND> WindowHandle { get; set; }
        [Category("Window Info")]
        public string ProcessName { get { return m_ProcessName; } set { m_ProcessName = value; } }
        [Category("Window Info")]
        public string WindowTitle { get { return m_WindowTitle; } set { m_WindowTitle = value; } }

        [Category("Invoke Info")]
        public int RetrySec { get { return m_RetrySec; } set { m_RetrySec = value; } }



        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context)
        {
            // 텍스트 입력 인수의 런타임 값을 가져옵니다.
            string text = context.GetValue(this.Text);

            HWND hWnd = context.GetValue(this.WindowHandle);

            if (hWnd == IntPtr.Zero)
            {
                int nRetryCnt = 0;
                while (nRetryCnt < m_RetrySec)
                {
                    Debug.WriteLine("FindWindow {0}, {1}", ProcessName, WindowTitle);

                    hWnd = WindowList.FindWindowByTitle(ProcessName, WindowTitle, false);   //"notepad++", "Notepad"
                    if (hWnd != IntPtr.Zero)
                        break;

                    Thread.Sleep(1000);
                    nRetryCnt++;
                }

                if (hWnd == IntPtr.Zero)
                {
                    Debug.WriteLine("Not Found Window");
                    return;
                }
            }

            Debug.WriteLine("Maxmize Window");

            WindowList.ShowMaxmizeWindow(hWnd);
        }
    }
}
