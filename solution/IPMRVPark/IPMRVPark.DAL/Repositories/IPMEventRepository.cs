using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class IPMEventRepository : RepositoryBase<ipmevent>
    {
        public IPMEventRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end CountryRepository
}
