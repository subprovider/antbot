
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ktds.Ant.Activities
{
    class CustomInlineEditor : PropertyValueEditor
    {

        public CustomInlineEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            var column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

            /*
            var column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            */

            gridFactory.AppendChild(column1);
            //gridFactory.AppendChild(column2);


            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(TextBox));
            Binding textBinding = new Binding("StringValue");
            textBox.SetValue(TextBox.TextProperty, textBinding);
            textBox.SetValue(Grid.ColumnProperty, 0);
            gridFactory.AppendChild(textBox);

            /*
            FrameworkElementFactory slider = new FrameworkElementFactory(typeof(Slider));
            Binding sliderBinding = new Binding("Value");
            sliderBinding.Mode = BindingMode.TwoWay;
            slider.SetValue(Slider.MinimumProperty, 0);
            slider.SetValue(Slider.MaximumProperty, 100);
            slider.SetValue(Slider.ValueProperty, sliderBinding);
            slider.SetValue(Grid.ColumnProperty, 1);
            gridFactory.AppendChild(slider);
            */

            this.InlineEditorTemplate.VisualTree = gridFactory;

        }
    }



    class DataTypeBoolean
    {
        public bool BoolValue { get; set; }

        public DataTypeBoolean(bool bValue)
        {
            BoolValue = bValue;
        }

        public override string ToString()
        {
            return BoolValue.ToString();
        }
    }

    class CustomInlineComboBoxYesNo : PropertyValueEditor
    {

        public CustomInlineComboBoxYesNo()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            var column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

            gridFactory.AppendChild(column1);

            var YesNoList = new List<string>();
            YesNoList.Add("No");
            YesNoList.Add("Yes");

            FrameworkElementFactory comboBox = new FrameworkElementFactory(typeof(ComboBox));
            Binding textBinding = new Binding("StringValue");
            comboBox.SetValue(ComboBox.TextProperty, textBinding);
            comboBox.SetValue(ComboBox.IsEditableProperty, false);

            comboBox.SetValue(ComboBox.ItemsSourceProperty, YesNoList);
            comboBox.SetValue(Grid.ColumnProperty, 0);
            gridFactory.AppendChild(comboBox);

            this.InlineEditorTemplate.VisualTree = gridFactory;
 
        }
    }
}
