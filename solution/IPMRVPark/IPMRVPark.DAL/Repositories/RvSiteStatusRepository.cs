using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class RvSiteStatusRepository : RepositoryBase<rvsite_status_view>
    {
        public RvSiteStatusRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}
