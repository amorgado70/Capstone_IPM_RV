using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class PaymentViewRepository : RepositoryBase<payment_view>
    {
        public PaymentViewRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }

}
