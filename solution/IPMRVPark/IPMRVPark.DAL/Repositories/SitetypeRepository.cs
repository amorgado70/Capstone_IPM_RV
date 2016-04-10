using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class SitetypeRepository : RepositoryBase<sitetype>
    {
        public SitetypeRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}

