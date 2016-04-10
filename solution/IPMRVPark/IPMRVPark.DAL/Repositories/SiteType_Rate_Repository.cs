using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class SiteType_Rate_Repository : RepositoryBase<sitetype_service_rate_view>
    {
        public SiteType_Rate_Repository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}
