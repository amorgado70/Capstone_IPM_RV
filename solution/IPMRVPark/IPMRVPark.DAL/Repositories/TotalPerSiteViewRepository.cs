using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class TotalPerSiteViewRepository : RepositoryBase<total_per_site_view>
    {
        public TotalPerSiteViewRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end CountryRepository
}
