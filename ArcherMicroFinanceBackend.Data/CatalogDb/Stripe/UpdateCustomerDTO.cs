﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PanoramBackend.Data.CatalogDb.Stripe
{
    public class UpdateCustomerDTO : CreateCustomerDTO
    {
        public string CustomerId { get; set; }
    }
}
