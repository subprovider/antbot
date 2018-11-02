using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Automation;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Runtime.InteropServices;
using System.Threading;

namespace ktds.Ant.Activities
{
    using HWND = IntPtr;

    [Designer(typeof(UIAutoActivityDesigner))]
    public sealed class UIAutoActivity : CodeActivity
    {
        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        private string m_ProcessName = "";
        private string m_WindowTitle = "";

        private string m_ControlName = "";
        private string m_ControlType = "";
        private string m_AutomationId = "";
        private string m_ControlTypeId = "";
        private string m_PattrnStr = "";
        private string m_AttrIsStr = "";

        private string m_ImageNmae = "";
        private string m_ImageString = "";
        private string m_ImageFileName = "";

        private System.Windows.Forms.MouseButtons m_MouseButton = MouseButtons.None;
        private MouseClickType m_MouseClickAction = MouseClickType.None;

        private string m_ActionPattern = "";
        private HWND m_FoundWindowHandle = IntPtr.Zero;
        private int m_RetrySec = 30;  //default 30sec

        [Category("Window Info")]
        public InArgument<HWND> WindowHandle { get; set; }
        [Category("Window Info")]
        public string ProcessName { get { return m_ProcessName; } set { m_ProcessName = value; } }
        [Category("Window Info")]
        public string WindowTitle { get { return m_WindowTitle; } set { m_WindowTitle = value; } }


        [Category("Element Info")]
        public string ControlName { get { return m_ControlName; } set { m_ControlName = value; } }
        [Category("Element Info")]
        public string ControlType { get { return m_ControlType; } set { m_ControlType = value; } }
        [Category("Element Info")]
        public string ControlTypeId { get { return m_ControlTypeId; } set { m_ControlTypeId = value; } }
        [Category("Element Info")]
        public string AutomationId { get { return m_AutomationId; } set { m_AutomationId = value; } }
        [Category("Element Info")]
        public string PatternList { get { return m_PattrnStr; } set { m_PattrnStr = value; } }
        [Category("Element Info")]
        public string AttrIsList { get { return m_AttrIsStr; } set { m_AttrIsStr = value; } }


        [Category("Element Info")]
        public string ImageName { get { return m_ImageNmae; } set { m_ImageNmae = value; } }
        [Category("Element Info")]
        public string ImageFileName { get { return m_ImageFileName; } set { m_ImageFileName = value; NotifyPropertyChanged("ImageFileName"); } }
        [Category("Element Info")]
        public string ImageString { get { return m_ImageString; } set { m_ImageString = value; } }


        [Category("Invoke Info")]
        public System.Windows.Forms.MouseButtons MouseButton { get { return m_MouseButton; } set { m_MouseButton = value; } }
        [Category("Invoke Info")]
        public MouseClickType MouseClickAction { get { return m_MouseClickAction; } set { m_MouseClickAction = value; } }

        [Category("Invoke Info")]
        public int RetrySec { get { return m_RetrySec; } set { m_RetrySec = value; } }

        public string ActionPattern { get { return m_ActionPattern; } set { m_ActionPattern = value; } }

        public List<Activity> Activities { get; set; }


        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }


        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public UIAutoActivity()
        {
            Activities = new List<Activity>();
        }

        private System.Drawing.Bitmap ControlImage { get { return new Bitmap(new MemoryStream(Convert.FromBase64String(m_ImageString))); } }

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

            this.ResultBool.Set(context, false);

            HWND hWnd = context.GetValue(this.WindowHandle);

            if(hWnd == IntPtr.Zero)
            {
                int nRetryCnt = 0;
                while(nRetryCnt < m_RetrySec)
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

            m_FoundWindowHandle = hWnd;

            Debug.WriteLine("Found Window");

            ControlType controlType;
            AutomationElement foundElement = null;
            if (hWnd != IntPtr.Zero)
            {
                System.Windows.Automation.ControlType.Button.ToString();  // ControlType class bug ㅜ.ㅜ, 첫번째 호출시 결과 return 안됨. tostring 호출하여 먼저 초기화 필요

                int nRetryCnt = 0;
                while (nRetryCnt < m_RetrySec)
                {
                    controlType = System.Windows.Automation.ControlType.LookupById(int.Parse(m_ControlTypeId));

                    Debug.WriteLine("controlTypeId :  {0}, {1}", m_ControlTypeId, controlType);
                    Debug.WriteLine("Find element {0}, {1}", ControlName, controlType.ToString());

                    foundElement = FindElement(hWnd, AutomationId, ControlName, controlType);

                    if (foundElement != null)
                        break;

                    Thread.Sleep(1000);
                    nRetryCnt++;
                }
            }

            if (foundElement == null)
            {
                Debug.WriteLine("Not Found Element");
                return;
            }

            Debug.WriteLine("Found Element");
            /*
                        try
                        {
                            foundElement.SetFocus();
                        }
                        catch(Exception ex)
                        {
                            PrintExceptionLog(ex);
                        }
                        */

            int x = 0, y = 0;
            try
            {
                var clickablePoint = foundElement.GetClickablePoint();

                x = (int)clickablePoint.X;
                y = (int)clickablePoint.Y;
            }
            catch (Exception ex)
            {
                System.Windows.Rect rect = foundElement.Current.BoundingRectangle;

                if(rect.X > 0 && rect.Y > 0)
                {
                    x = (int) (rect.X + rect.Width) / 2;   //center
                    y = (int) (rect.Y + rect.Height) / 2;  //center 
                }
            }


            Debug.WriteLine("Point {0}, {1}", x, y);
            if (x > 0 && y > 0)
            {
                Cursor.Position = new System.Drawing.Point(x, y);
                //Cursor.Clip = new Rectangle(this.Location, this.Size);
            }

          
            if ((x <= 0 || y <= 0) && MouseClickAction != MouseClickType.None)
            {
                object objPattern;
                if (true == foundElement.TryGetCurrentPattern(InvokePattern.Pattern, out objPattern))
                {
                    InvokeControl(foundElement);
                }
            }
            else
            { 
                if (MouseClickAction == MouseClickType.Click)
                {
                    DoMouseClickEvent(MouseButton, x, y);
                }

                if (MouseClickAction == MouseClickType.DblClick)
                {
                    DoMouseDblClickEvent(MouseButton, x, y);
                }
            }

            WaitForCompleted();


            this.ResultBool.Set(context, true);

        }

        public static void InvokeControl(AutomationElement targetControl)
        {
            InvokePattern invokePattern = null;

            try
            {
                invokePattern =
                    targetControl.GetCurrentPattern(InvokePattern.Pattern)
                    as InvokePattern;
            }
            catch (ElementNotEnabledException)
            {
                // Object is not enabled
                return;
            }
            catch (InvalidOperationException)
            {
                // object doesn't support the InvokePattern control pattern
                return;
            }

            invokePattern.Invoke();
        }

        private void WaitForCompleted()
        {
            int nTimeout = 30;
            int nRetryCount = 0;
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            foreach(SHDocVw.InternetExplorer ie in shellWindows)
            {
                if(ie.HWND == (int) m_FoundWindowHandle)
                {
                    Debug.WriteLine("Found SHDocVw ");

                    try
                    {
                        while (ie.Busy == true || ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                        {

                            System.Threading.Thread.Sleep(1000);
                            nRetryCount++;
                            if (nRetryCount > nTimeout)
                                break;
                        }
                    }
                    catch 
                    {
                        //Maybe window is closed
                        return;
                    }

                    break;
                }
            }
        }

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

        public static void DoMouseClickEvent(System.Windows.Forms.MouseButtons btnType, int x, int y)
        {
            Debug.WriteLine("DoMouseClickEvent");

            if (btnType == System.Windows.Forms.MouseButtons.Right)
            {
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
            }
            else if (btnType == System.Windows.Forms.MouseButtons.Middle)
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

            if (btnType == System.Windows.Forms.MouseButtons.Right)
            {
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightDown, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
                mouse_event((int)MouseEventFlags.RightUp, x, y, 0, 0);
            }
            else if (btnType == System.Windows.Forms.MouseButtons.Middle)
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
            // Set the Current cursor, move the cursor's Position,
            // and set its clipping rectangle to the form. 

            //this.Cursor = new Cursor(Cursor.Current.Handle);

            Cursor.Position = new Point(x, y);   //컴파일 오류시 참조 추가 : System.Drawing

            //Thread.Sleep(300);
        }

        public AutomationElement FindElement(HWND hWndMain, string sAutomationID, string sElementName, ControlType controlType)
                                                            //string sClassName, string sText)
        {
            AutomationElement foundElement = null;
            AutomationElement rootElement = AutomationElement.FromHandle(hWndMain);

            //automation id로 찾기
            if (sAutomationID != "")
            {
                Debug.WriteLine("try find by automationid");
                //call under test
                foundElement = rootElement.FindFirst(TreeScope.Children,
                                                     new PropertyCondition(AutomationElement.AutomationIdProperty, sAutomationID));
                if (foundElement != null)
                    return foundElement;
            }

            //Element Name이 (none)이면 최초 Recording시 Name이 없었던것임
            if (sElementName == "(none)")
                sElementName = "";

            /*
                        AutomationElementCollection elementList = rootElement.FindAll(TreeScope.Children | TreeScope.Descendants,
                                                                                      new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType),
                                                                                                       new PropertyCondition(AutomationElement.NameProperty, sElementName),
                                                                                                       new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                                                                                                       new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                                                                                                       new PropertyCondition(AutomationElement.Is​Invoke​Pattern​Available​Property, true)
                                                                                                       )
                                                                                     );
             */

            List<Condition> cond =  GetPatternCondition(sElementName, controlType);

            Debug.WriteLine("try find by attribute {0}, {1}, attr : {2}", sElementName, controlType, cond.Count);
            AutomationElementCollection elementList = rootElement.FindAll(TreeScope.Children | TreeScope.Descendants,
                                                                          new AndCondition(cond.ToArray())
                                                                         );

            if (elementList.Count <= 0)  // not found
                return null;

            if (elementList.Count == 1)  //only one
                return elementList[0];


            Debug.WriteLine("try find by image");
            System.Drawing.Bitmap controlImg = ControlImage;
            foreach (AutomationElement element in elementList)
            {
                System.Windows.Rect boundingRect = (System.Windows.Rect)
                                                   element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

                if (controlImg != null)
                {

                    Image<Bgr, byte> source = new Image<Bgr, byte>(controlImg);
                    Image<Bgr, byte> template = new Image<Bgr, byte>(GetImage(element));  //null check 안해도 될지??

                    using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        System.Drawing.Point[] minLocations, maxLocations;

                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        if (maxValues[0] > (double)0.8)  //80%일치
                        {
                            return element;
                        }
                    }
                }

                //ClassName이 의미가 있는가??
                /*------------------------------------
                 *  if (sClassName != "") // class name으로 찾기
                    {
                        //call under test
                        foundElement = element.FindFirst(TreeScope.Children,
                                                         new PropertyCondition(AutomationElement.ClassNameProperty, sClassName));
                        if (foundElement != null)
                            return foundElement;
                    }
                 *---------------------------------------*/
            }

            return elementList[0];  //첫번째 element 리턴
        }

        private Dictionary<string, object> uiElementPattn = new Dictionary<string, object>();
        private void InitializePattern()
        { 
            uiElementPattn.Add("Dock",            DockPattern.Pattern.Id );
            uiElementPattn.Add("ExpandCollapse",  ExpandCollapsePattern.Pattern.Id );
            uiElementPattn.Add("Grid",            GridPattern.Pattern.Id);
            uiElementPattn.Add("GridItem",        GridItemPattern.Pattern.Id);
            uiElementPattn.Add("Invoke",          InvokePattern.Pattern.Id);
            uiElementPattn.Add("MultipleView",    MultipleViewPattern.Pattern.Id);
            uiElementPattn.Add("RangeValue",      RangeValuePattern.Pattern.Id);
            uiElementPattn.Add("Scroll",          ScrollPattern.Pattern.Id);
            uiElementPattn.Add("ScrollItem",      ScrollItemPattern.Pattern.Id);
            uiElementPattn.Add("Selection",       SelectionPattern.Pattern.Id);
            uiElementPattn.Add("SelectionItem",   SelectionItemPattern.Pattern.Id);
            uiElementPattn.Add("Table",           TablePattern.Pattern.Id);
            uiElementPattn.Add("TableItem",       TableItemPattern.Pattern.Id);
            uiElementPattn.Add("Text",            TextPattern.Pattern.Id);
            uiElementPattn.Add("Toggle",          TogglePattern.Pattern.Id);
            uiElementPattn.Add("Transform",       TransformPattern.Pattern.Id);
            uiElementPattn.Add("Value",           ValuePattern.Pattern.Id);
            uiElementPattn.Add("Window",          WindowPattern.Pattern.Id);
        }

        private List<Condition> GetPatternCondition(string sElementName, ControlType controlType)
        {
            int i = 0;
            List<Condition> andCond = new List<Condition>();

            andCond.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
            andCond.Add(new PropertyCondition(AutomationElement.NameProperty, sElementName));

            Dictionary<string, bool> AttrDic = AttrIsList.Split(',')
              .Select(value => value.Split('='))
              .ToDictionary(pair => pair[0], pair => bool.Parse(pair[1]));

            foreach(KeyValuePair<string, bool> attr in AttrDic)
            {
                if (attr.Key == "IsContentElement")
                    andCond.Add(new PropertyCondition(AutomationElement.IsContentElementProperty, attr.Value));
                if (attr.Key == "IsControlElement")
                    andCond.Add(new PropertyCondition(AutomationElement.IsControlElementProperty, attr.Value));
                if (attr.Key == "IsEnabled")
                    andCond.Add(new PropertyCondition(AutomationElement.IsEnabledProperty, attr.Value));
                if (attr.Key == "IsKeyboardFocusable")
                    andCond.Add( new PropertyCondition(AutomationElement.IsKeyboardFocusableProperty, attr.Value));
                if (attr.Key == "IsOffscreen")
                    andCond.Add( new PropertyCondition(AutomationElement.IsOffscreenProperty, attr.Value));
                if (attr.Key == "IsPassword")
                    andCond.Add( new PropertyCondition(AutomationElement.IsPasswordProperty, attr.Value));
            }

            //return andCond;

            Dictionary<string, bool> PattnDic = AttrIsList.Split(',')
                                               .Select(value => value.Split('='))
                                               .ToDictionary(pair => pair[0], pair => bool.Parse(pair[1]));

            foreach(KeyValuePair<string, bool> pattn in PattnDic)
            {
                if (pattn.Key == "Dock" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsDockPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "ExpandCollapse" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsExpandCollapsePatternAvailableProperty, pattn.Value));

                if (pattn.Key == "GridItem" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsGridItemPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Grid" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsGridPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Invoke" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, pattn.Value));

                if (pattn.Key == "ItemContainer" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsItemContainerPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "MultipleView" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsMultipleViewPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "RangeValue" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsRangeValuePatternAvailableProperty, pattn.Value));

                if (pattn.Key == "ScrollItem" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsScrollItemPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Scroll" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsScrollPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "SelectionItem" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsSelectionPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "SynchronizedInput" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsSynchronizedInputPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "TableItem" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsTableItemPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Text" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Toggle" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsTogglePatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Transform" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsTransformPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Value" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsValuePatternAvailableProperty, pattn.Value));

                if (pattn.Key == "VirtualizedItem" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsVirtualizedItemPatternAvailableProperty, pattn.Value));

                if (pattn.Key == "Window" && pattn.Value)
                    andCond.Add( new PropertyCondition(AutomationElement.IsWindowPatternAvailableProperty, pattn.Value));
            }

            return andCond;
        }


        private Rectangle GetElementRect(AutomationElement uiElement)
        {
            System.Windows.Rect wrect = uiElement.Current.BoundingRectangle;
            return new Rectangle((int)wrect.Left, (int)wrect.Top, (int)wrect.Width, (int)wrect.Height);
        }

        private System.Drawing.Bitmap GetImage(AutomationElement uiElement)
        {
            System.Drawing.Bitmap uiElementImage;
            try
            {
                System.Drawing.Rectangle rect = GetElementRect(uiElement);
                if (rect.Width <= 0 || rect.Height <= 0)
                {
                    Debug.WriteLine("element rectangle is 0");
                    return null;
                }

                uiElementImage = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(uiElementImage);
                g.CopyFromScreen(rect.Location, new System.Drawing.Point(0, 0), rect.Size);
                g.Save();
            }
            catch (Exception ex)
            {
                PrintExceptionLog(ex);
                return null;
            }

            return uiElementImage;
        }

        public static void PrintExceptionLog(Exception ex)
        {
            Debug.WriteLine("\nMessage ---\n{0}", ex.Message);
            Debug.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
            Debug.WriteLine("\nSource ---\n{0}", ex.Source);
            Debug.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
            Debug.WriteLine("\nTargetSite ---\n{0}", ex.TargetSite);
        }

    } //end class

    public enum MouseClickType
    {
        None = 0,
        Click = 1,
        DblClick = 2
    }
}
