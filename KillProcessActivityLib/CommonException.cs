using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ktds.Ant.CommonUtil
{

    public static class CommonException
	{
	    public static void PrintExceptionLog(Exception ex)
        {
            Debug.WriteLine("\nMessage ---\n{0}", ex.Message);
            Debug.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
            Debug.WriteLine("\nSource ---\n{0}", ex.Source);
            Debug.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
            Debug.WriteLine("\nTargetSite ---\n{0}", ex.TargetSite);
        }

	}
}


//RPA 활동인력 어필 포인트로 작성 추가
// 11월 7일 DRAMA TF 전체적인
//RPA에 고객개발팀 RPA 활동 내역 추가
