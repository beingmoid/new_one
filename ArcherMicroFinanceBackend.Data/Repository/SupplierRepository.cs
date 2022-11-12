﻿using PanoramBackend.Data;
using PanoramBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PanoramBackend.Data.Repository
{
    public class SupplierRepository : EFRepository<UserDetails, int>, ISupplierRepository
    {
        public SupplierRepository(AMFContext requestScope) : base(requestScope)
        {

        }
        protected override IQueryable<UserDetails> Query => base.Query.Where(x=>x.IsSupplier==true);
    }
    public interface ISupplierRepository : IEFRepository<UserDetails, int>
    {

    }
}
