using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class PaymentReservationItemRepository : RepositoryBase<paymentreservationitem>
    {
        public PaymentReservationItemRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }

}
