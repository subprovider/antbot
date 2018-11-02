
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;

namespace Microsoft.Samples.Activities.Designer.PropertyGridExtensibility
{

    public sealed class ImageDectorActivity : CodeActivity
    {
        public InArgument<string> Text { get; set; }
        public double Capacity { get; set; }
        public string ImageFileName { get; set; }

        // since designer and activity are in same assembly register in static constructor
        static ImageDectorActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ImageDectorActivity), "Capacity", new EditorAttribute(typeof(CustomInlineEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ImageDectorActivity), "ImageFileName", new EditorAttribute(typeof(FilePickerEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        public ImageDectorActivity()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string text = context.GetValue(this.Text);
            
                Console.WriteLine("Value entered was {0}", text);
            Console.WriteLine("Image file name is {0}", ImageFileName);
             
        }
    }


}
