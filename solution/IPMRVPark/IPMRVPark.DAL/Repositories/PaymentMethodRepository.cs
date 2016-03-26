using IPMRVPark.Contracts.Data;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class PaymentMethodRepository : RepositoryBase<paymentmethod>
    {
        public PaymentMethodRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end PaymentMethodRepository
}
