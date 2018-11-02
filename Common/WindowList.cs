using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ktds.Ant.Activities
{
    using HWND = IntPtr;

    public static class WindowList
    {
        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                // if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        public static HWND FindWindowByTitle(string sProcessName, string sTitle, bool bWholeWord)
        {
            foreach (KeyValuePair<IntPtr, string> window in WindowList.GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;

                if (sProcessName != "")   //input에 processName이 있으면 process name 일치 여부를 먼저 체크
                {
                    uint pid = 0;
                    GetWindowThreadProcessId(handle, out pid);
                    Process ps = Process.GetProcessById((int)pid);

                    if (ps.ProcessName != sProcessName)
                        continue;
                }

                if (bWholeWord)
                {
                    if (title == sTitle)
                        return handle;
                }
                else
                {
                    if (title.Contains(sTitle))
                        return handle;
                }
            }

            return IntPtr.Zero;
        }


        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }

}
