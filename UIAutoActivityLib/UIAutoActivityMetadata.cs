using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktds.Ant.Activities
{
    class UIAutoActivityMetadata : IRegisterMetadata
    {
        public void Register()
        {
            RegisterAll();
        }
        public static void RegisterAll()
        {
            var builder = new AttributeTableBuilder();
            UIAutoActivityDesigner.RegisterMetadata(builder);
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
     
}
