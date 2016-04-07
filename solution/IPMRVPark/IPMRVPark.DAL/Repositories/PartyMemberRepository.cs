using IPMRVPark.Contracts.Data;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Repositories
{
    public class PartyMemberRepository : RepositoryBase<partymember>
    {
        public PartyMemberRepository(DataContext context)
            : base(context)
        { if (context == null) throw new ArgumentNullException(); }
    }//end PartyMemberRepository
}
