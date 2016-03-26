using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class ReasonForPaymentRepository : RepositoryBase<reasonforpayment>
    {
        public ReasonForPaymentRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end ReasonForPaymentRepository
}
