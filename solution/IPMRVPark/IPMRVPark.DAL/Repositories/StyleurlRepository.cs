using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class StyleurlRepository : RepositoryBase<styleurl>
    {
        public StyleurlRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}

