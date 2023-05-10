using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Attributes
{
    /// <summary>
    /// Use this attribute to indicate that a method is related to
    /// another type. This helps reflection code to detect relationships
    /// that are not implicitly or otherwise indicated.
    /// </summary>
    /// <example>
    /// [RelatesToType(typeof(IPower))]
    /// public virtual void Power() ...
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RelatesToTypeAttribute : Attribute
    {  
        public Type RelatedType { get; private set; }

        public RelatesToTypeAttribute(Type relatedType)
        {
            RelatedType = relatedType;
        }
    }
}