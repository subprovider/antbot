using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ktds.UBot.Activities.Common
{
    public static class CommonUtil
    {

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        public enum MouseButtonType
        {
            Left = 0,
            Middle,
            Right
        }

        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }


        public static void DoMouseEvent(MouseEventFlags value, int x, int y)
        {
            mouse_event((int)value, x, y, 0, 0);
        }

        public static void DoMouseClickEvent(MouseButtonType btnType, int x, int y)
        {
            if(btnType == MouseButtonType.Right)
            {
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
            }
            else if (btnType == MouseButtonType.Middle)
            {
                mouse_event((int)MouseEventFlags.MiddleDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.MiddleUp, x, y, 0, 0);
            }
            else // btnType == MouseButtonType.Left
            {
                mouse_event((int)MouseEventFlags.LeftDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.LeftUp, x, y, 0, 0);
            }
        }

        public static void MoveCursor(int x, int y)
        {
            // Set the Current cursor, move the cursor's Position,
            // and set its clipping rectangle to the form. 

            //this.Cursor = new Cursor(Cursor.Current.Handle);

            Cursor.Position = new Point(x, y);

            //Thread.Sleep(300);
        }



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public enum ShowWindowType
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }


    }
}
