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
        public string idPlacemarkPolygon { get; set; }
        public string customerName { get; set; }
        public Nullable<int> idReservationOrder { get; set; }
        public Nullable<int> idReservationItem { get; set; }
        public Nullable<bool> inShoppingCart { get; set; }
        public Nullable<bool> paymentReceived { get; set; }
        public Nullable<System.DateTime> checkInDate { get; set; }
        public Nullable<System.DateTime> outOfServiceUntil { get; set; }
    }
}