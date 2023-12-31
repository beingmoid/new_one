﻿using PanoramaBackend.Data;
using PanoramaBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PanoramaBackend.Data.Repository
{
    public class AgentRepository : EFRepository<UserDetails, int>, IAgentRepository
    {
        public AgentRepository(AMFContext requestScope) : base(requestScope)
        {

        }
        protected override IQueryable<UserDetails> Query => base.Query.Where(x => x.IsAgent == true);

    }
    public interface IAgentRepository : IEFRepository<UserDetails, int>
    {
       
    }
}
