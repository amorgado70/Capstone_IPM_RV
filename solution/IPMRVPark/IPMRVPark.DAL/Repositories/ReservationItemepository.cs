using System;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Contracts.Repositories
{
    public class ReservationItemRepository : RepositoryBase<reservationitem>
    {
        public ReservationItemRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }

}
