using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class CustomerRepository : RepositoryBase<customer>
    {
        public CustomerRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end CustomerRepository
}
