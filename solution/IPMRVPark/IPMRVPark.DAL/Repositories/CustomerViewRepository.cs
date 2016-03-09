using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class CustomerViewRepository : RepositoryBase<customer_view>
    {
        public CustomerViewRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }

}
