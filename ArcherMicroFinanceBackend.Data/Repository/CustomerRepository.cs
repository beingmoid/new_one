﻿using PanoramaBackend.Data;
using PanoramaBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PanoramaBackend.Data.Repository
{
   
    public class CustomerRepository : EFRepository<UserDetails, int>, ICustomerRepository
    {
        public CustomerRepository(AMFContext requestScope) : base(requestScope)
        {

        }
        protected override IQueryable<UserDetails> Query => base.Query.Where(x => x.IsCustomer == true);

    }
    public interface ICustomerRepository : IEFRepository<UserDetails, int>
    {

    }
}
