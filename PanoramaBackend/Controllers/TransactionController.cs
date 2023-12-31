﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NukesLab.Core.Api;
using NukesLab.Core.Repository;
using PanoramaBackend.Controllers;
using PanoramaBackend.Data.Entities;
using PanoramaBackend.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PanoramaBackend.Api.Controllers
{

    public class TransactionController : BaseController<Transaction,int>
    {
        private readonly ITransactionService _service;

        public TransactionController(RequestScope requestScope,ITransactionService
            service)
            :base(requestScope,service)
        {
            _service = service;
        }
        public async override Task<BaseResponse> Get()
        {
            var transactions = await _service.Get(x => x.Include(x => x.Payment).Include(x => x.Refund).Include(x => x.SalesInvoice).Include(x => x.UserDetails));
            return constructResponse(transactions);
        }

    }
}
