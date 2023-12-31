﻿using PanoramaBackend.Data;
using PanoramaBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace PanoramaBackend.Data.Repository
{
    public class PreferredPaymentMethodRepository : EFRepository<PreferredPaymentMethod, int>, IPreferredPaymentMethodRepository
    {
        public PreferredPaymentMethodRepository(AMFContext requestScope) : base(requestScope)
        {

        }
    }
    public interface IPreferredPaymentMethodRepository : IEFRepository<PreferredPaymentMethod, int>
    {

    }
}
