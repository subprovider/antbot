using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MouseClickActivity
{

    public sealed class MouseClickActivity : CodeActivity
    {
        // 형식 문자열의 작업 입력 인수를 정의합니다.
        [RequiredArgument]
        [Category("Mouse Info")]
        public InArgument<Point> MousePosition { get; set; }


        [Category("Output")]
        public OutArgument<bool> Result { get; set; }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context)
        {
            this.Result.Set(context, false);

            
            Point mousePos = context.GetValue(this.MousePosition);

            Debug.WriteLine(mousePos.ToString());
            
            if (mousePos == null)
                return;

            MoveCursor(mousePos.X, mousePos.Y);

            DoMouseClickEvent(MouseEventFlags.LeftDown, mousePos.X, mousePos.Y);
            DoMouseClickEvent(MouseEventFlags.LeftUp,   mousePos.X, mousePos.Y);

            this.Result.Set(context, true);
        }


        private static void MoveCursor(int x, int y)
        {
            // Set the Current cursor, move the cursor's Position,
            // and set its clipping rectangle to the form. 

            //Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(x, y);
            //Cursor.Clip = new Rectangle(this.Location, this.Size);

            //Thread.Sleep(300);

        }

        public static void DoMouseClickEvent(MouseEventFlags value, int x, int y)
        {
            mouse_event((int)value, x, y, 0, 0);
        }
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
}
