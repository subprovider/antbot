using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;

namespace ktds.Ant.Activities
{

    [Designer(typeof(MouseClickActivityDesigner))]
    public sealed class MouseClickActivity : CodeActivity
    {
  
        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        [Category("Mouse Info - Design time")]
        public int DesignX { get; set; }
        [Category("Mouse Info - Design time")]
        public int DesignY { get; set; }
        [Category("Mouse Info - Design time")]
        //public MouseButtons MouseButton { get; set; }
        public MouseButtons DesignMouseButton { get; set; }
        [Category("Mouse Info - Design time")]
        public MouseClickType DesignMouseClick { get; set; }

        [Category("Mouse Info - Run time")]
        public InArgument<int> X { get; set; }
        [Category("Mouse Info - Run time")]
        public InArgument<int> Y { get; set; }
        [Category("Mouse Info - Run time")]
        //public MouseButtons MouseButton { get; set; }
        public InArgument<MouseButtons> MouseButton { get; set; }
        [Category("Mouse Info - Run time")]
        public InArgument<MouseClickType> MouseClick { get; set; }

        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }

        public MouseClickActivity()
        {
            //AttributeTableBuilder builder = new AttributeTableBuilder();
            //builder.AddCustomAttributes(typeof(MouseClickActivity), "X", new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            //builder.AddCustomAttributes(typeof(MouseClickActivity), "Y", new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            //MetadataStore.AddAttributeTable(builder.CreateTable());
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

            int x = context.GetValue(this.X);
            int y = context.GetValue(this.Y);

            MouseButtons mouseButton = context.GetValue(this.MouseButton);
            MouseClickType mouseClick = context.GetValue(this.MouseClick);

            Debug.WriteLine("Runtime mouseButton : {0}, click : {1}, x : {2}, y : {3}", mouseButton, mouseClick, x, y);

            if(x <= 0 || y <= 0 || mouseButton == MouseButtons.None || mouseClick == MouseClickType.None )  //runtime 입력값 없음 design time용 변수로 처리
            {
                x = DesignX;
                y = DesignY;
                mouseButton = DesignMouseButton;
                mouseClick = DesignMouseClick;

                Debug.WriteLine("Design time mouseButton : {0}, click : {1}, x : {2}, y : {3}", mouseButton, mouseClick, x, y);
            }

            if (x <= 0 || y <= 0 || mouseButton == MouseButtons.None || mouseClick == MouseClickType.None)
            {
                Debug.WriteLine("필수 값 입력 오류");
                this.ResultBool.Set(context, false);
            }

            MoveCursor(x, y);

            if (mouseClick == MouseClickType.Click)
            {
                DoMouseClickEvent(mouseButton, x, y);
            }

            if (mouseClick == MouseClickType.DblClick)
            {
                DoMouseDblClickEvent(mouseButton, x, y);
            }

            this.ResultBool.Set(context, true);
        }


        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        
        public static void DoMouseEvent(MouseEventFlags value, int x, int y)
        {
            mouse_event((int)value, x, y, 0, 0);
        }

        public static void DoMouseClickEvent(MouseButtons btnType, int x, int y)
        {
            Debug.WriteLine("DoMouseClickEvent");

            if (btnType == MouseButtons.Right)
            {
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
            }
            else if (btnType == MouseButtons.Middle)
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

        public static void DoMouseDblClickEvent(MouseButtons btnType, int x, int y)
        {
            Debug.WriteLine("DoMouseDblClickEvent");

            if (btnType == MouseButtons.Right)
            {
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
            }
            else if (btnType == MouseButtons.Middle)
            {
                mouse_event((int)MouseEventFlags.MiddleDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.MiddleDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.MiddleUp, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.MiddleUp, x, y, 0, 0);
            }
            else // btnType == MouseButtonType.Left
            {
                mouse_event((int)MouseEventFlags.LeftDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.LeftDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.LeftUp, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.LeftUp, x, y, 0, 0);
            }
        }

        public static void MoveCursor(int x, int y)
        {
            Debug.WriteLine("MoveCursor");

            //this.Cursor = new Cursor(Cursor.Current.Handle);

            Cursor.Position = new Point(x, y);   //컴파일 오류시 참조 추가 : System.Drawing
        }

    }

    public enum MouseClickType
    {
        None = 0,
        Click = 1,
        DblClick = 2
    }

    public enum MouseButtons   //system.windows.form.MouseButtons 와 동일
    {
        None = 0,
        Left = 1048576,
        Right = 2097152,
        Middle = 4194304,
        XButton1 = 8388608,
        XButton2 = 16777216
    }

    public enum MouseButtonType //미사용
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

}
