using Microsoft.Win32;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace ktds.Ant.Activities
{
    class FilePickerEditor : System.Activities.Presentation.PropertyEditing.DialogPropertyValueEditor
    {
        public FilePickerEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            var column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

            var column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));

            gridFactory.AppendChild(column1);
            gridFactory.AppendChild(column2);
             
            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(TextBox));
            Binding textBinding = new Binding("StringValue");
            textBox.SetValue(TextBox.TextProperty, textBinding);
            textBox.SetValue(Grid.ColumnProperty, 0);
            gridFactory.AppendChild(textBox);

            FrameworkElementFactory editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            editModeSwitch.SetValue(Grid.ColumnProperty, 1);
            gridFactory.AppendChild(editModeSwitch);

            this.InlineEditorTemplate.VisualTree = gridFactory;
            
        }



        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Filter = "Image files (*.jpg)|*.jpg";
            ofd.Title = "Select Image file";

            if (ofd.ShowDialog() == true)
            {
                propertyValue.Value = ofd.FileName; //.Substring(ofd.FileName.LastIndexOf('\\')+1);
            }
        }
    }

      
    
}
