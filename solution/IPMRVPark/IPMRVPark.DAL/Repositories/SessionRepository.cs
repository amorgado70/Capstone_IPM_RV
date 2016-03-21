using IPMRVPark.Contracts.Data;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class SessionRepository : RepositoryBase<session>
    {
        public SessionRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }

    }//end CountryRepository
}
