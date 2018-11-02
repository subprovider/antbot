using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ktds.Ant.Activities
{
    using HWND = IntPtr;
    public class ElementInfo
    {

        private AutomationElement uiElement = null;

        private Dictionary<string, string> uiElementAttr = new Dictionary<string, string>();
        private Dictionary<string, bool> uiElementPattn = new Dictionary<string, bool>();
        private string m_PattnStr = "";
        private string m_AttrIsStr = "";
        private string m_ProcessName = "";
        private string m_WindowCaption = "";

        private Bitmap uiElementImage = null;

        private ControlType m_ControlType;

        public Bitmap ElementImage { get { return uiElementImage; } set { uiElementImage = value; } }
        public ControlType ControlType { get { return m_ControlType; } set { m_ControlType = value; } }
        public string PatternList { get { return m_PattnStr; } set { m_PattnStr = value; } }
        public string AttrIsList { get { return m_AttrIsStr; } set { m_AttrIsStr = value; } }
        public string ProcessName { get { return m_ProcessName; } set { m_ProcessName = value; } }
        public string WindowCaption { get { return m_WindowCaption; } set { m_WindowCaption = value; } }

        public event StructureChangedEventHandler StructureChangedHandler;

        public ElementInfo(AutomationElement argUiElement)
        {
            uiElement = argUiElement;

            m_AttrIsStr = ParseElementAttribute();
            m_PattnStr = ParseElementPattern();
            ParseImage();
        }

        public static void PrintExceptionLog(Exception ex)
        {
            Debug.WriteLine("\nMessage ---\n{0}", ex.Message);
            Debug.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
            Debug.WriteLine("\nSource ---\n{0}", ex.Source);
            Debug.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
            Debug.WriteLine("\nTargetSite ---\n{0}", ex.TargetSite);
        }

        AutomationElement FindChildControlElement(AutomationElement parent)
        {
            Condition findCondition = new PropertyCondition(AutomationElement.IsControlElementProperty, true);

            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            Condition condition3 = new PropertyCondition(AutomationElement.Is​Invoke​Pattern​Available​Property, true);

            AutomationElement found = parent.FindFirst(TreeScope.Children, new AndCondition(condition1, condition2, condition3));
            return found;
        }


        public ElementInfo(int x, int y)
        {
            //System.Drawing.Point mouse = System.Windows.Forms.Cursor.Position; // use Windows forms mouse code instead of WPF

            try
            {
                uiElement = AutomationElement.FromPoint(new System.Windows.Point(x, y));

                /* 불필요
                bool isControl = (bool)uiElement.GetCurrentPropertyValue(AutomationElement.IsControlElementProperty);
                bool isInvokeAvailable = (bool)uiElement.GetCurrentPropertyValue(AutomationElement.Is​Invoke​Pattern​Available​Property);
                bool isEnabledControl = (bool)uiElement.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty);

                object obj;
                if (!(isControl && uiElement.TryGetCurrentPattern(InvokePattern.Pattern, out obj)))
                {
                    AutomationElement childElement = FindChildControlElement(uiElement);
                    if (childElement != null)
                        uiElement = childElement;
                }
                */

                int processIdentifier = (int)uiElement.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty);
                ProcessName = Process.GetProcessById(processIdentifier).ProcessName;

                Debug.WriteLine("Process name is " + ProcessName);

                HWND hWnd = GetWindowHandleFromPoint(x, y);
                if (hWnd != IntPtr.Zero)
                {
                    while (GetParent(hWnd) != IntPtr.Zero)
                        hWnd = GetParent(hWnd);

                    StringBuilder title1 = new StringBuilder(2048);
                    int nLength = GetWindowText(hWnd, title1, title1.Capacity);
                    if (nLength > 0)
                       WindowCaption = title1.ToString();

                    uint processId = 0;
                    GetWindowThreadProcessId(hWnd, out processId);
                    ProcessName = Process.GetProcessById((int)processId).ProcessName;

                    Debug.WriteLine("Process name is " + ProcessName);
                    Debug.WriteLine("Window Title is " + WindowCaption);
                }

            }
            catch(Exception ex)
            {
                PrintExceptionLog(ex);
                uiElement = null;
                return;
            }

            if (uiElement == null)
            {
                // no element under mouse
                return;
            }

            ParseImage(); //Image 먼저 Capture

            m_AttrIsStr = ParseElementAttribute();
            m_PattnStr  = ParseElementPattern();

        }

        [DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(HWND hWnd, out uint processId);

        [DllImport("User32.dll")]
        static extern IntPtr GetParent(HWND hwnd);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point p);
        public IntPtr GetWindowHandleFromPoint(int x, int y)
        {
            var point = new Point(x, y);
            return WindowFromPoint(point);
        }

        public ElementInfo(AutomationElement argUiElement, bool bWithChangedEvent)
        {
            uiElement = argUiElement;

            m_AttrIsStr = ParseElementAttribute();
            //ParseElementPattern();
            //ParseImage();


            if (bWithChangedEvent && argUiElement != null)
            {
                Automation.AddStructureChangedEventHandler(uiElement, TreeScope.Element,
                            new StructureChangedEventHandler(OnStructureChanged));
            }
        }

        private void OnStructureChanged(object sender, StructureChangedEventArgs e)
        {
            if (StructureChangedHandler != null)
            {
                StructureChangedHandler(uiElement, e);
            }
        }


        public string GetAttributeValue(string sAttrName)
        {
            string sValue = "";

            uiElementAttr.TryGetValue(sAttrName, out sValue);

            return sValue;
        }

        private string ParseElementAttribute()
        {
            ControlType = uiElement.Current.ControlType;

            string sAttrList = "";  //control 찾기 위해 일부 속성을 보관

            uiElementAttr.Add("Name",          uiElement.Current.Name.Trim() == "" ? "(none)" : uiElement.Current.Name.Trim());
            uiElementAttr.Add("AccleratorKey", uiElement.Current.AcceleratorKey);
            uiElementAttr.Add("AccessKey",     uiElement.Current.AccessKey);
            uiElementAttr.Add("AutomationId",  uiElement.Current.AutomationId);
            uiElementAttr.Add("BoundingRectangle", uiElement.Current.BoundingRectangle.ToString());
            uiElementAttr.Add("ClassName",         uiElement.Current.ClassName);
            uiElementAttr.Add("ControlType",       uiElement.Current.ControlType.ProgrammaticName);
            uiElementAttr.Add("ControlTypeId",     uiElement.Current.ControlType.Id.ToString());
            uiElementAttr.Add("LocalizedControlType", uiElement.Current.LocalizedControlType);
            uiElementAttr.Add("FrameworkId",          uiElement.Current.FrameworkId);
            uiElementAttr.Add("HasKeyboardFocus",     uiElement.Current.HasKeyboardFocus.ToString());
            uiElementAttr.Add("HelpText",             uiElement.Current.HelpText);

            uiElementAttr.Add("IsContentElement", uiElement.Current.IsContentElement.ToString());
            uiElementAttr.Add("IsControlElement", uiElement.Current.IsControlElement.ToString());
            uiElementAttr.Add("IsEnabled",        uiElement.Current.IsEnabled.ToString());
            uiElementAttr.Add("IsKeyboardFocusable", uiElement.Current.IsKeyboardFocusable.ToString());
            uiElementAttr.Add("IsOffscreen",         uiElement.Current.IsOffscreen.ToString());
            uiElementAttr.Add("IsPassword",          uiElement.Current.IsPassword.ToString());
            uiElementAttr.Add("IsRequiredForForm",   uiElement.Current.IsRequiredForForm.ToString());

            uiElementAttr.Add("ItemStatus", uiElement.Current.ItemStatus);
            uiElementAttr.Add("ItemType",   uiElement.Current.ItemType);

            try
            {
                uiElementAttr.Add("LabeledBy", uiElement.Current.LabeledBy.Current.Name);
            }
            catch(Exception ex)
            {
                PrintExceptionLog(ex);
                uiElementAttr.Add("LabeledBy", string.Empty);
            }

            uiElementAttr.Add("NativeWindowHandle", uiElement.Current.NativeWindowHandle.ToString());
            uiElementAttr.Add("Orientation",        uiElement.Current.Orientation.ToString());
            uiElementAttr.Add("ProcessId",          uiElement.Current.ProcessId.ToString());

            //보다 Unique하게 찾기 위해 일부 attribute값을 보관
            sAttrList = String.Format("{0}={1},", "IsContentElement", uiElementAttr["IsContentElement"])
                      + String.Format("{0}={1},", "IsControlElement", uiElementAttr["IsControlElement"])
                      + String.Format("{0}={1},", "IsEnabled",        uiElementAttr["IsEnabled"])
                      + String.Format("{0}={1},", "IsKeyboardFocusable", uiElementAttr["IsKeyboardFocusable"])
                      + String.Format("{0}={1},", "IsOffscreen",      uiElementAttr["IsOffscreen"])
                      + String.Format("{0}={1}", "IsPassword",        uiElementAttr["IsPassword"]);

            Debug.WriteLine("sAttrList : " + sAttrList);

            return sAttrList;
        }




        private string ParseElementPattern()
        {
            Object obj;

            try
            {
                uiElementPattn.Add("Dock", uiElement.TryGetCurrentPattern(DockPattern.Pattern, out obj));
                uiElementPattn.Add("ExpandCollapse", uiElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out obj));
                uiElementPattn.Add("GridItem", uiElement.TryGetCurrentPattern(GridItemPattern.Pattern, out obj));
                uiElementPattn.Add("Grid", uiElement.TryGetCurrentPattern(GridPattern.Pattern, out obj));
                uiElementPattn.Add("Invoke", uiElement.TryGetCurrentPattern(InvokePattern.Pattern, out obj));
                uiElementPattn.Add("ItemContainer", uiElement.TryGetCurrentPattern(ItemContainerPattern.Pattern, out obj));
                uiElementPattn.Add("MultipleView", uiElement.TryGetCurrentPattern(MultipleViewPattern.Pattern, out obj));
                uiElementPattn.Add("RangeValue", uiElement.TryGetCurrentPattern(RangeValuePattern.Pattern, out obj));
                uiElementPattn.Add("ScrollItem", uiElement.TryGetCurrentPattern(ScrollItemPattern.Pattern, out obj));
                uiElementPattn.Add("Scroll", uiElement.TryGetCurrentPattern(ScrollPattern.Pattern, out obj));
                uiElementPattn.Add("Selection", uiElement.TryGetCurrentPattern(SelectionPattern.Pattern, out obj));
                uiElementPattn.Add("SelectionItem", uiElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out obj));
                uiElementPattn.Add("SynchronizedInput", uiElement.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out obj));
                uiElementPattn.Add("Table", uiElement.TryGetCurrentPattern(TablePattern.Pattern, out obj));
                uiElementPattn.Add("TableItem", uiElement.TryGetCurrentPattern(TableItemPattern.Pattern, out obj));
                uiElementPattn.Add("Text", uiElement.TryGetCurrentPattern(TextPattern.Pattern, out obj));
                uiElementPattn.Add("Toggle", uiElement.TryGetCurrentPattern(TogglePattern.Pattern, out obj));
                uiElementPattn.Add("Transform", uiElement.TryGetCurrentPattern(TransformPattern.Pattern, out obj));
                uiElementPattn.Add("Value", uiElement.TryGetCurrentPattern(ValuePattern.Pattern, out obj));
                uiElementPattn.Add("VirtualizedItem", uiElement.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out obj));
                uiElementPattn.Add("Window", uiElement.TryGetCurrentPattern(WindowPattern.Pattern, out obj));
            }
            catch(Exception ex)
            {
                PrintExceptionLog(ex);
            }

            string sPattnStr = "";
            foreach(KeyValuePair<string, bool> pattn in uiElementPattn)
            {
                if (sPattnStr == "")
                    sPattnStr += String.Format("{0}={1}", pattn.Key, pattn.Value);
                else
                    sPattnStr += "," + String.Format("{0}={1}", pattn.Key, pattn.Value);
            }

            Debug.WriteLine("sPattnStr : " + sPattnStr);

            return sPattnStr;
        }

        /*
        public AndCondition GetAndConditionByElementPattern()
        {
            Condition[] cond = new Condition[32];

            int i = 0;
            foreach (KeyValuePair <string, object> Pattern in uiElementPattn)
            {
                cond[i] = Pattern.Value as Condition;
                i++;
            }

            return ( new AndCondition(cond));
        }
        */



        public void ParseImage()
        {
            try
            {
                Rectangle rect = GetElementRect();
                if (rect.Width <= 0 || rect.Height <= 0)
                    uiElementImage = null;

                uiElementImage = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(uiElementImage);
                g.CopyFromScreen(rect.Location, new Point(0, 0), rect.Size);
                g.Save();
            }
            catch(Exception ex)
            {
                PrintExceptionLog(ex);
            }
        }
        
        internal Rectangle GetElementRect()
        {
            System.Windows.Rect wrect = uiElement.Current.BoundingRectangle;
            return new Rectangle((int)wrect.Left, (int)wrect.Top, (int)wrect.Width, (int)wrect.Height);
        }

        public override string ToString()
        {
            return uiElement.Current.LocalizedControlType + " : " + uiElement.Current.Name;
        }
        
    }
}
