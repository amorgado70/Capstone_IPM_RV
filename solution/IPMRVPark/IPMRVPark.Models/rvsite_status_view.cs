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
    
    public partial class rvsite_status_view
    {
        public long id { get; set; }
        public long Year { get; set; }
        public string RVSite { get; set; }
        public string SiteSize { get; set; }
        public string PowerSupply { get; set; }
        public string backgroundColor { get; set; }
        public Nullable<int> isAvaialable { get; set; }
        public Nullable<System.DateTime> SelectedFrom { get; set; }
        public Nullable<System.DateTime> SelectedUntil { get; set; }
        public Nullable<System.DateTime> ReservedFrom { get; set; }
        public Nullable<System.DateTime> ReservedUntil { get; set; }
        public Nullable<System.DateTime> OutOfServiceFrom { get; set; }
        public Nullable<System.DateTime> OutOfServiceUntil { get; set; }
        public string fullName { get; set; }
        public string mainPhone { get; set; }
    }
}
