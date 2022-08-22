using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared
{
    [DefaultValue(None)]
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum KeysanityMode
    {    
        [Description("None")]
        None,

        [Description("Both Games")]
        Both,

        [Description("Zelda Only")]
        Zelda,

        [Description("Metroid Only")]
        SuperMetroid,
    }
}
