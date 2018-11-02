using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ktds.Ant.Activities
{
    // MouseClickActivityDesigner.xaml에 대한 상호 작용 논리
    public partial class MouseClickActivityDesigner
    {
        public MouseClickActivityDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(MouseClickActivity), new DesignerAttribute(typeof(MouseClickActivityDesigner)));
            builder.AddCustomAttributes(typeof(MouseClickActivity), new DescriptionAttribute("ktds AntBot's MouseClickActivity"));
        }



    }


}
