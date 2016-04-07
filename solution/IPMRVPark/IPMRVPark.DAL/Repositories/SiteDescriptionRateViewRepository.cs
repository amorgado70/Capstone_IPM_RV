using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class SiteDescriptionRateViewRepository : RepositoryBase<site_description_rate_view>
    {
        public SiteDescriptionRateViewRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }

}
