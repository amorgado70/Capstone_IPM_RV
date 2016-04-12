﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using IPMRVPark.Models;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;

namespace IPMRVPark.Contracts.Data
{
    public class DataContext : DbContext
    {
        public DataContext() : base("ipmrvparkDbContext")
        {

        }
        public DbSet<checkinout> checkinouts { get; set; }
        public DbSet<coordinate> coordinates { get; set; }
        public DbSet<countrycode> countrycodes { get; set; }
        public DbSet<customer> customers { get; set; }
        public DbSet<customeraccount> customeraccounts { get; set; }
        public DbSet<fee> fees { get; set; }
        public DbSet<ipmevent> ipmevents { get; set; }
        public DbSet<outofserviceitem> outofserviceitems { get; set; }
        public DbSet<partymember> partymembers { get; set; }
        public DbSet<paydoctype> paydoctypes { get; set; }
        public DbSet<payment> payments { get; set; }
        public DbSet<paymentmethod> paymentmethods { get; set; }
        public DbSet<paymentreservationitem> paymentreservationitems { get; set; }
        public DbSet<person> people { get; set; }
        public DbSet<placeinmap> placeinmaps { get; set; }
        public DbSet<provincecode> provincecodes { get; set; }
        public DbSet<reasonforpayment> reasonforpayments { get; set; }
        public DbSet<reservationitem> reservationitems { get; set; }
        public DbSet<reservationitem_partymember> reservationitem_partymember { get; set; }
        public DbSet<selecteditem> selecteditems { get; set; }
        public DbSet<service> services { get; set; }
        public DbSet<session> sessions { get; set; }
        public DbSet<siterate> siterates { get; set; }
        public DbSet<sitesize> sitesizes { get; set; }
        public DbSet<sitetype> sitetypes { get; set; }
        public DbSet<staff> staffs { get; set; }
        public DbSet<styleurl> styleurls { get; set; }
        public DbSet<customer_view> customer_view { get; set; }
        public DbSet<partymember_view> partymember_view { get; set; }
        public DbSet<payment_view> payment_view { get; set; }
        public DbSet<province_view> province_view { get; set; }
        public DbSet<rvsite_available_view> rvsite_available_view { get; set; }
        public DbSet<rvsite_coord_view> rvsite_coord_view { get; set; }
        public DbSet<rvsite_status_view> rvsite_status_view { get; set; }
        public DbSet<site_description_rate_view> site_description_rate_view { get; set; }
        public DbSet<sitetype_service_rate_view> sitetype_service_rate_view { get; set; }
        public DbSet<staff_view> staff_view { get; set; }

        public virtual ObjectResult<sp_delete_sitetype_dependants_Result> sp_delete_sitetype_dependants(Nullable<long> typeID, ObjectParameter success, ObjectParameter errMsg)
        {
            var typeIDParameter = typeID.HasValue ?
                new ObjectParameter("typeID", typeID) :
                new ObjectParameter("typeID", typeof(long));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_delete_sitetype_dependants_Result>("sp_delete_sitetype_dependants", typeIDParameter, success, errMsg);
        }

        public virtual ObjectResult<sp_reset_event_derivatives_Result> sp_reset_event_derivatives(Nullable<long> eventID, ObjectParameter success, ObjectParameter errMsg)
        {
            var eventIDParameter = eventID.HasValue ?
                new ObjectParameter("eventID", eventID) :
                new ObjectParameter("eventID", typeof(long));

            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_reset_event_derivatives_Result>("sp_reset_event_derivatives", eventIDParameter, success, errMsg);
        }
    }
}
