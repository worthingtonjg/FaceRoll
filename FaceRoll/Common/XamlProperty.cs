using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRoll.Common
{
    /// <summary>
    /// This is a do nothing attribute that is simply used to indicate when a property is being referenced in XAML.  
    /// The IDE doesn't show references for properties only referenced in xaml.
    /// By adding this attribute, it creates a hint to the developer that the property is being used in the XAML.
    /// </summary>
    public class XamlProperty : Attribute
    {
    }
}
