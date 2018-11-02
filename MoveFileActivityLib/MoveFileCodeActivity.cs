using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace ktds.Ant.Activities
{

    [Designer(typeof(MoveFileActivityDesigner))]

    public sealed class MoveFileCodeActivity : CodeActivity
    {
        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        public string SourceFullName { get; set; }
        private string m_SourceFileName;

        //private string m_SourcePathName;
        public string TargetPathName { get; set; }
        public bool IgnoreError { get; set; }


        public OutArgument<bool> ResultBool { get; set; }

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
            this.ResultBool.Set(context, false);

            if (!File.Exists(SourceFullName))
            {
                Debug.WriteLine("Source File not found");

                if (IgnoreError)
                    this.ResultBool.Set(context, true);

                return;
            }

            m_SourceFileName = Path.GetFileName(SourceFullName);

            if (!System.IO.Directory.Exists(TargetPathName))
            {
                System.IO.Directory.CreateDirectory(TargetPathName);
            }

            //string sourceFile = System.IO.Path.Combine(SourcePathName, SourceFileName);
            string destFile = System.IO.Path.Combine(TargetPathName, m_SourceFileName);

            try
            {
                System.IO.File.Move(SourceFullName, destFile);
            }
            catch
            {
                if (IgnoreError)
                    this.ResultBool.Set(context, true);
                return;
            }

            this.ResultBool.Set(context, true);

            return;
        }
    }
}
