﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NukesLab.Core.Api;
using NukesLab.Core.Repository;
using PanoramaBackend.Controllers;
using PanoramBackend.Data;
using PanoramBackend.Data.Entities;
using PanoramBackend.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PanoramaBackend.Api.Controllers
{

    public class BranchController : BaseController<Branch,int>
    {
        private readonly AMFContext _context;

        public BranchController(RequestScope requestScope,IBranchService
            service,AMFContext context)
            :base(requestScope,service)
        {
            _context = context;
        }

        [HttpGet("GetBranchWithSales")]
        public async Task<BaseResponse> GetBracnhWithSales()
        {
            var data = await _context.Set<Branch>().Include(x => x.Sales).ToListAsync();
            return constructResponse(data);
        }
    }
}
