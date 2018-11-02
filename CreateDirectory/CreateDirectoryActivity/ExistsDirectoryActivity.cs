using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.IO;
using System.ComponentModel;

namespace ktds.Ant.Activities
{

    public sealed class ExistsDirectoryActivity : CodeActivity
    {
        private string msPath = "";

        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }
        
        [RequiredArgument]
        [Category("Directory")]
        public string Path { get { return msPath; } set { msPath = value; } }


        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }

        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context)
        {
            this.ResultBool.Set(context, Directory.Exists(msPath));
        }
    }
}
