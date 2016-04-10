using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;


namespace IPMRVPark.Contracts.Repositories
{
    public class CoordinateRepository : RepositoryBase<coordinate>
    {
        public CoordinateRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }
}
