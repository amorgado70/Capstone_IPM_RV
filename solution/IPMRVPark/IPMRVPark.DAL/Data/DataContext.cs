using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using IPMRVPark.Models;

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
        public DbSet<ipmevent> ipmevents { get; set; }
        public DbSet<outofservice> outofservices { get; set; }
        public DbSet<party> parties { get; set; }
        public DbSet<partymember> partymembers { get; set; }
        public DbSet<payment> payments { get; set; }
        public DbSet<paymentmode> paymentmodes { get; set; }
        public DbSet<person> people { get; set; }
        public DbSet<placemarkpolygon> placemarkpolygons { get; set; }
        public DbSet<powersupply> powersupplies { get; set; }
        public DbSet<provincecode> provincecodes { get; set; }
        public DbSet<reservationitem> reservationitems { get; set; }
        public DbSet<reservationorder> reservationorders { get; set; }
        public DbSet<siterate> siterates { get; set; }
        public DbSet<sitesize> sitesizes { get; set; }
        public DbSet<sitetype> sitetypes { get; set; }
        public DbSet<staff> staffs { get; set; }
        public DbSet<styleurl> styleurls { get; set; }
        public DbSet<customer_view> customer_view { get; set; }
        public DbSet<rvsite_coord_view> rvsite_coord_view { get; set; }
        public DbSet<rvsite_status_view> rvsite_status_view { get; set; }
    }
}
