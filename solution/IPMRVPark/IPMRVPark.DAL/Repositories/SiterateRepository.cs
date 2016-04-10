using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class SiterateRepository : RepositoryBase<siterate>
    {
        public SiterateRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}

