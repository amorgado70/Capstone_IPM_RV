//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IPMRVPark.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class sitetype_service_rate_view
    {
        public long eventId { get; set; }
        public long typeid { get; set; }
        public Nullable<long> styleId { get; set; }
        public long sizeId { get; set; }
        public long serviceId { get; set; }
        public string size { get; set; }
        public string service { get; set; }
        public Nullable<decimal> week { get; set; }
        public Nullable<decimal> day { get; set; }
    }
}
