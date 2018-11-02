using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Diagnostics;
using System.ComponentModel;
using ktds.Ant.CommonUtil;

namespace ktds.Ant.Activities
{

    [Designer(typeof(KillProcessActivityDesigner))]

    public sealed class KillProcessActivity : CodeActivity
    {

        private string m_ProcessName = "";
        private string m_WindowTitle = "";

        // 형식 문자열의 작업 입력 인수를 정의합니다.
        //public InArgument<string> Text { get; set; }

        [RequiredArgument]
        [Category("Process Info")]
        public string ProcessName { get; set; }

        [Category("Process Info")]
        public string WindowTitle { get; set; }

        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }



        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context) 
        {
            this.ResultBool.Set(context, false);

            // 텍스트 입력 인수의 런타임 값을 가져옵니다.
            //string text = context.GetValue(this.Text);

            List<int> piList = WindowList.GetProcessIdByWindowByTitle(ProcessName, WindowTitle, false);

            foreach(int pid in piList)
            { 
                Debug.WriteLine("Found Pid {0}", pid.ToString());

                try
                {
                    Process proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                catch (ArgumentException ex)
                {
                    // Process already exited.
                    CommonException.PrintExceptionLog(ex);
                }

            }

            this.ResultBool.Set(context, true);
        }
    }
}
