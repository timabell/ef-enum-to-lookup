using System;
using System.Diagnostics;

namespace EfEnumToLookup.LookupGenerator
{
    [DebuggerDisplay("{nq:DebugDisplay")]
    internal class Reference
    {
        public string ReferencingTable { get; set; }
        public string ReferencingField { get; set; }
        public Type EnumType { get; set; }

        public string DebugDisplay
        {
            get { return string.Format("{0}.{1}", ReferencingTable, ReferencingField); }
        }
    }
}
