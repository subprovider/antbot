using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Activities;
using System.Activities.Presentation.Toolbox;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Statements;

using System.Reflection;
using System.IO;
using System.Activities.XamlIntegration;
using Microsoft.Win32;
using ktds.AntBot.Studio.Helpers;
using System.ComponentModel;
using System.Timers;
using Twilio;
using System.Runtime.Versioning;

using ktds.Ant.Activities;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ktds.AntBot.Studio.Views
{

    public partial class MainWindow : INotifyPropertyChanged
    {
        private WorkflowApplication _wfApp;
        private ToolboxControl _wfToolbox;
        private CustomTrackingParticipant _executionLog;

        private string _currentWorkflowFile = string.Empty;
        private Timer _timer;

        private readonly AttributeTableBuilder _builder = new AttributeTableBuilder();

        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(1000);
            _timer.Enabled = false;
            _timer.Elapsed += TrackingDataRefresh;

            //load all available workflow activities from loaded assemblies 
            InitializeActivitiesToolbox();

            //initialize designer
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;

        }


        public string ExecutionLog
        {
            get
            {
                if (_executionLog != null)
                    return _executionLog.TrackData;
                else
                    return string.Empty;
            }
            set { _executionLog.TrackData = value; NotifyPropertyChanged("ExecutionLog"); }
        }


        private void TrackingDataRefresh(Object source, ElapsedEventArgs e)
        {
            NotifyPropertyChanged("ExecutionLog");
        }


        private void consoleExecutionLog_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            consoleExecutionLog.ScrollToEnd();
        }


        /// <summary>
        /// show execution log in ui
        /// </summary>
        private void UpdateTrackingData()
        {
            //retrieve & display execution log
            //consoleExecutionLog.Dispatcher.Invoke(
            //    System.Windows.Threading.DispatcherPriority.Normal,
            //    new Action(
            //        delegate ()
            //        {
            //            //consoleExecutionLog.Text = _executionLog.TrackData;
            NotifyPropertyChanged("ExecutionLog");
            //        }
            //));
        }


        /// <summary>
        /// Retrieves all Workflow Activities from the loaded assemblies and inserts them into a ToolboxControl 
        /// </summary>
        private void InitializeActivitiesToolbox()
        {


           // WfToolboxBorder.Child = CreateToolbox();

            //return;

            try
            {
                _wfToolbox = new ToolboxControl();
                //_wfToolbox.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(89,93,97,100));
               // #FF595D61;

                //LoadDefaultActivities(_wfToolbox);

            //LabelStatusBar.Content = String.Format("Loaded Activities: {0}", activitiesCount.ToString());
            //WfToolboxBorder.Child = _wfToolbox;

            //return;

            //load dependency
            AppDomain.CurrentDomain.Load("Twilio.Api");
                // load Custom Activity Libraries into current domain
                //AppDomain.CurrentDomain.Load("MeetupActivityLibrary");
                // load System Activity Libraries into current domain; uncomment more if libraries below available on your system

                //AppDomain.CurrentDomain.Load("ControlFlow");
                AppDomain.CurrentDomain.Load("System.Activities");
                AppDomain.CurrentDomain.Load("System.ServiceModel.Activities");
                AppDomain.CurrentDomain.Load("System.Activities.Core.Presentation");
                AppDomain.CurrentDomain.Load("Microsoft.Activities.Extensions");
                AppDomain.CurrentDomain.Load("Microsoft.Activities.Extensions.Http");
                AppDomain.CurrentDomain.Load("Microsoft.Activities.UnitTesting");
                AppDomain.CurrentDomain.Load("Excel.Activities");
                AppDomain.CurrentDomain.Load("ZipAndUnzipFile");
                AppDomain.CurrentDomain.Load("WFExtended");

                
                /*
                AppDomain.CurrentDomain.Load("Microsoft.Workflow.Management");
                AppDomain.CurrentDomain.Load("Microsoft.Activities.Extensions");
                AppDomain.CurrentDomain.Load("Microsoft.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.Activities.Hosting");
                AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Utility.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Security.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Management.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Diagnostics.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.Powershell.Core.Activities");
                AppDomain.CurrentDomain.Load("Microsoft.PowerShell.Activities");
                */

                // get all loaded assemblies
                IEnumerable<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name);

                // check if assemblies contain activities
                int activitiesCount = 0;
                foreach (Assembly activityLibrary in appAssemblies)
                {
                    var wfToolboxCategory = new ToolboxCategory(activityLibrary.GetName().Name);
                    var actvities = from
                                        activityType in activityLibrary.GetExportedTypes()
                                    where
                                        (activityType.IsSubclassOf(typeof(Activity))
                                        || activityType.IsSubclassOf(typeof(NativeActivity))
                                        || activityType.IsSubclassOf(typeof(DynamicActivity))
                                        || activityType.IsSubclassOf(typeof(ActivityWithResult))
                                        || activityType.IsSubclassOf(typeof(AsyncCodeActivity))
                                        || activityType.IsSubclassOf(typeof(CodeActivity))
                                        || activityType == typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<Type>)
                                        || activityType == typeof(System.Activities.Statements.FlowNode)
                                        || activityType == typeof(System.Activities.Statements.State)
                                        || activityType == typeof(System.Activities.Core.Presentation.FinalState)
                                        || activityType == typeof(System.Activities.Statements.FlowDecision)
                                        || activityType == typeof(System.Activities.Statements.FlowNode)
                                        || activityType == typeof(System.Activities.Statements.FlowStep)
                                        || activityType == typeof(System.Activities.Statements.FlowSwitch<Type>)
                                        || activityType == typeof(System.Activities.Statements.ForEach<Type>)
                                        || activityType == typeof(System.Activities.Statements.Switch<Type>)
                                        || activityType == typeof(System.Activities.Statements.TryCatch)
                                        || activityType == typeof(System.Activities.Statements.While))
                                        && activityType.IsVisible
                                        && activityType.IsPublic
                                        && !activityType.IsNested
                                        && !activityType.IsAbstract
                                        && (activityType.GetConstructor(Type.EmptyTypes) != null)
                                        && !activityType.Name.Contains('`') //optional, for extra cleanup
                                    orderby
                                        activityType.Name
                                    select
                                        new ToolboxItemWrapper(activityType);

                    actvities.ToList().ForEach(wfToolboxCategory.Add);

                    if (wfToolboxCategory.Tools.Count > 0)
                    {
                        _wfToolbox.Categories.Add(wfToolboxCategory);
                        activitiesCount += wfToolboxCategory.Tools.Count;
                    }
                }



                /*kcjin
                //fixed ForEach
                _wfToolbox.Categories.Add(
                       new System.Activities.Presentation.Toolbox.ToolboxCategory
                       {
                           CategoryName = "CustomForEach",
                           Tools = {
                                new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<>)),
                                new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<>))
                           }
                       }
                );
                */
               
                _wfToolbox.Categories.Add(
                       new System.Activities.Presentation.Toolbox.ToolboxCategory
                       {
                           CategoryName = "AntBot",
                           Tools = {
                                new ToolboxItemWrapper(typeof(ImageDetectorActivity)),
                                new ToolboxItemWrapper(typeof(OpenBrowserActivity)),
                                new ToolboxItemWrapper(typeof(KeyInActivity)),
                                new ToolboxItemWrapper(typeof(CreateDirectoryActivity)),
                                new ToolboxItemWrapper(typeof(ExistsDirectoryActivity)),
                                new ToolboxItemWrapper(typeof(UIAutoActivity)),
                                new ToolboxItemWrapper(typeof(MouseClickActivity)),
                                new ToolboxItemWrapper(typeof(MaxmizeWindowActivity)),
                                new ToolboxItemWrapper(typeof(MoveFileCodeActivity)),
                                new ToolboxItemWrapper(typeof(KillProcessActivity))
                           }
                       }
                );
                

                /*
                _wfToolbox.Categories.Add(
                new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(ImageDetectorActivity)), });
                _wfToolbox.Categories.Add(
                                new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(OpenBrowserActivity)), });

                _wfToolbox.Categories.Add(
                                new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(KeyInActivity)), });
                */

                LabelStatusBar.Content = String.Format("Loaded Activities: {0}", activitiesCount.ToString());
                WfToolboxBorder.Child = _wfToolbox;



            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadDefaultActivities(ToolboxControl tbc)
        {
            var dict = new ResourceDictionary
            {
                Source =
                    new Uri(
                        "pack://application:,,,/System.Activities.Presentation;component/themes/icons.xaml")
            };
            Resources.MergedDictionaries.Add(dict);

            var standtypes = typeof(Activity).Assembly.GetTypes().
                Where(t => (typeof(Activity).IsAssignableFrom(t) ||
                            typeof(IActivityTemplateFactory)
                                .IsAssignableFrom(t)) && !t.IsAbstract &&
                           t.IsPublic &&
                           !t.IsNested && HasDefaultConstructor(t));

            var primary = new ToolboxCategory("Native Activities");

            foreach (var type in standtypes.OrderBy(t => t.Name))
            {
                var w = new ToolboxItemWrapper(type, ToGenericTypeString(type));
                if (!AddIcon(type, _builder))
                {
                    primary.Add(w);
                }
                //else
                //{
                //    //secondary.Add(w);
                //}
            }

            MetadataStore.AddAttributeTable(_builder.CreateTable());
            tbc.Categories.Add(primary);
        }


        protected bool AddIcon(Type type, AttributeTableBuilder builder)
        {
            var secondary = false;

            var tbaType = typeof(ToolboxBitmapAttribute);
            var imageType = typeof(Image);
            var constructor = tbaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { imageType, imageType }, null);

            var resourceKey = type.IsGenericType ? type.GetGenericTypeDefinition().Name : type.Name;
            var index = resourceKey.IndexOf('`');
            if (index > 0)
            {
                resourceKey = resourceKey.Remove(index);
            }
            if (resourceKey == "Flowchart")
            {
                resourceKey = "FlowChart"; // it appears that themes/icons.xaml has a typo here
            }
            resourceKey += "Icon";
            Bitmap small, large;
            var resource = TryFindResource(resourceKey);
            if (!(resource is DrawingBrush))
            {
                //resource = FindResource("GenericLeafActivityIcon");
                //secondary = true;
            }
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
            {
                context.DrawRectangle(((DrawingBrush)resource), null, new Rect(0, 0, 32, 32));
                context.DrawRectangle(((DrawingBrush)resource), null, new Rect(32, 32, 16, 16));
            }
            var rtb = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                enc.Save(outStream);
                outStream.Position = 0;
                large = new Bitmap(outStream);
            }
            rtb = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
            dv.Offset = new Vector(-32, -32);
            rtb.Render(dv);
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                enc.Save(outStream);
                outStream.Position = 0;
                small = new Bitmap(outStream);
            }
            var tba = constructor.Invoke(new object[] { small, large }) as ToolboxBitmapAttribute;
            builder.AddCustomAttributes(type, tba);

            return secondary;
        }

        public static bool HasDefaultConstructor(Type t)
        {
            return t.GetConstructors().Any(c => c.GetParameters().Length <= 0);
        }

        public static string ToGenericTypeString(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));

            var genericArgs = string.Join(",", t.GetGenericArguments().Select(ToGenericTypeString));
            return genericTypeName + "<" + genericArgs + ">";
        }

        private static ToolboxControl CreateToolbox()
        {
            var toolboxControl = new ToolboxControl();

            toolboxControl.Categories.Add(
                new ToolboxCategory("Control Flow")
                    {
                        new ToolboxItemWrapper(typeof(DoWhile)),
                        new ToolboxItemWrapper(typeof(ForEach<>)),
                        new ToolboxItemWrapper(typeof(If)),
                        new ToolboxItemWrapper(typeof(Parallel)),
                        new ToolboxItemWrapper(typeof(ParallelForEach<>)),
                        new ToolboxItemWrapper(typeof(Pick)),
                        new ToolboxItemWrapper(typeof(PickBranch)),
                        new ToolboxItemWrapper(typeof(Sequence)),
                        new ToolboxItemWrapper(typeof(Switch<>)),
                        new ToolboxItemWrapper(typeof(While)),
                    });

            toolboxControl.Categories.Add(
                new ToolboxCategory("Primitives")
                    {
                        new ToolboxItemWrapper(typeof(Assign)),
                        new ToolboxItemWrapper(typeof(Delay)),
                        new ToolboxItemWrapper(typeof(InvokeMethod)),
                        new ToolboxItemWrapper(typeof(WriteLine)),
                    });

            toolboxControl.Categories.Add(
     new ToolboxCategory("FlowChart")
         {
                        new ToolboxItemWrapper(typeof(Flowchart)),
                        new ToolboxItemWrapper(typeof(FlowDecision)),
                        new ToolboxItemWrapper(typeof(FlowStep)),
                        new ToolboxItemWrapper(typeof(FlowSwitch<Type>)),

         });

            toolboxControl.Categories.Add(
                new ToolboxCategory("Error Handling")
                    {
                        new ToolboxItemWrapper(typeof(Rethrow)),
                        new ToolboxItemWrapper(typeof(Throw)),
                        new ToolboxItemWrapper(typeof(TryCatch)),
                    });

            toolboxControl.Categories.Add(
                new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(ImageDetectorActivity)), });
            toolboxControl.Categories.Add(
                            new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(OpenBrowserActivity)), });

            toolboxControl.Categories.Add(
                            new ToolboxCategory("ktds") { new ToolboxItemWrapper(typeof(KeyInActivity)), });
            return toolboxControl;
        }


        /// <summary>
        /// Retrieve Workflow Execution Logs and Workflow Execution Outputs
        /// </summary>
        private void WfExecutionCompleted(WorkflowApplicationCompletedEventArgs ev)
        {
            try
            {
                //retrieve & display execution log
                _timer.Stop();
                UpdateTrackingData();

                //retrieve & display execution output
                foreach (var item in ev.Outputs)
                {
                    consoleOutput.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                            delegate()
                            {
                                consoleOutput.Text += String.Format("[{0}] {1}" + Environment.NewLine, item.Key, item.Value);
                            }
                    ));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        #region Commands Handlers - Executed - New, Open, Save, Run

        /// <summary>
        /// Creates a new Workflow Application instance and executes the Current Workflow
        /// </summary>
        private void CmdWorkflowRun(object sender, ExecutedRoutedEventArgs e)
        {
            //get workflow source from designer
            CustomWfDesigner.Instance.Flush();
            MemoryStream workflowStream = new MemoryStream(Encoding.UTF8.GetBytes (CustomWfDesigner.Instance.Text));  // UTF8Encoding.Default.GetBytes
            DynamicActivity activityExecute = ActivityXamlServices.Load(workflowStream) as DynamicActivity;

            //configure workflow application
            consoleExecutionLog.Text = String.Empty;
            consoleOutput.Text = String.Empty;
            _executionLog = new CustomTrackingParticipant();
            _wfApp = new WorkflowApplication(activityExecute);
            _wfApp.Extensions.Add(_executionLog);
            _wfApp.Completed = WfExecutionCompleted;

            this.Hide();
            //execute 
            _wfApp.Run();
            this.Show();


            //enable timer for real-time logging
            _timer.Start();
        }

        /// <summary>
        /// Stops the Current Workflow
        /// </summary>
        private void CmdWorkflowStop(object sender, ExecutedRoutedEventArgs e)
        {
            //manual stop
            if (_wfApp != null)
            {
                _wfApp.Abort("Stopped by User");
                _timer.Stop();
                UpdateTrackingData();
            }

        }


        /// <summary>
        /// Save the current state of a Workflow
        /// </summary>
        private void CmdWorkflowSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentWorkflowFile == String.Empty)
            {
                var dialogSave = new SaveFileDialog();
                dialogSave.Title = "Save Workflow";
                dialogSave.Filter = "Workflows (.xaml)|*.xaml";

                if (dialogSave.ShowDialog() == true)
                {
                    CustomWfDesigner.Instance.Save(dialogSave.FileName);
                        _currentWorkflowFile = dialogSave.FileName;                
                }
            }
            else
            {
                CustomWfDesigner.Instance.Save(_currentWorkflowFile);
            }
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow 
        /// </summary>
        private void CmdWorkflowNew(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = String.Empty;
            CustomWfDesigner.NewInstance();
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;
        }

        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow 
        /// </summary>
        private void CmdWorkflowNewVB(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = String.Empty;
            CustomWfDesigner.NewInstanceVB();
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow with C# Expression Editor
        /// </summary>
        private void CmdWorkflowNewCSharp(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = String.Empty;
            CustomWfDesigner.NewInstanceCSharp();
            WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
            WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;
        }


        /// <summary>
        /// Loads a Workflow into a new Workflow Designer instance
        /// </summary>
        private void CmdWorkflowOpen(object sender, ExecutedRoutedEventArgs e)
        {            
            var dialogOpen = new OpenFileDialog();
            dialogOpen.Title = "Open Workflow";
            dialogOpen.Filter = "Workflows (.xaml)|*.xaml";

            if (dialogOpen.ShowDialog() == true)
            {
                using (var file = new StreamReader(dialogOpen.FileName, true))
                {
                    CustomWfDesigner.NewInstance(dialogOpen.FileName);
                    WfDesignerBorder.Child = CustomWfDesigner.Instance.View;
                    WfPropertyBorder.Child = CustomWfDesigner.Instance.PropertyInspectorView;

                    _currentWorkflowFile = dialogOpen.FileName;
                }
            }
        }

        #endregion


        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }


    public static class _global_
    {

    }
}


