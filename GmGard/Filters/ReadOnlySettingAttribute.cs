using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Filters
{
    /// <summary>
    /// Indicates this setting should never be overridden.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class ReadOnlySettingAttribute : Attribute
    {
    }
}
