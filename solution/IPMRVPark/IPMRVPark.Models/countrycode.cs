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
    
    public partial class countrycode
    {
        public countrycode()
        {
            this.addresses = new HashSet<address>();
            this.provincecodes = new HashSet<provincecode>();
        }
    
        public string code { get; set; }
        public string name { get; set; }
    
        public virtual ICollection<address> addresses { get; set; }
        public virtual ICollection<provincecode> provincecodes { get; set; }
    }
}
