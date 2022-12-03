﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NukesLab.Core.Api;
using NukesLab.Core.Repository;
using PanoramaBackend.Controllers;
using PanoramBackend.Data.Entities;
using PanoramBackend.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static NukesLab.Core.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Stripe;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace PanoramaBackend.Api.Controllers
{

    public class AgentController : BaseController<UserDetails, int>
    {
        private readonly IAgentService _service;
        private readonly ILedgerEntriesService _entriesService;
        private readonly ITransactionService _tranService;
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _env;

        public AgentController(RequestScope requestScope, IAgentService service, ILedgerEntriesService entriesService
            , ITransactionService tranService,
            IWebHostEnvironment env,
            IConverter converter            )
            : base(requestScope, service)
        {
            _service = service;
            _entriesService = entriesService;
            _tranService = tranService;
            _converter = converter;
            _env = env;
        }
        [HttpPost("SearchAndFilter")]
        public async Task<BaseResponse> SearchAndFilter([FromBody] SearchAndFilter filter, int accountId, string start, string end, string sortBy)
        {

            return constructResponse(await _entriesService.SearchAndFilter(accountId, filter.start, filter.end, filter.sortBy));

        }
        public async override Task<BaseResponse> Get()
        {
            return constructResponse(await _service.GetAgentsWithBalance());
        }

        [HttpGet("GetOne")]
        public async Task<BaseResponse> GetOne(int id)
        {
            var userDetail = (await _service.Get(x =>
              x.Include(x => x.Addresses)
             .Include(x => x.PaymentAndBilling)
               .ThenInclude(x => x.Terms)
             .Include(x => x.PaymentAndBilling)
              .ThenInclude(x => x.PreferredPaymentMethod)
              .Include(x => x.UserDetail)
              .Include(x => x.SalesInvoicePersons).ThenInclude(x => x.Transactions),
              x => x.Id == id
             )).SingleOrDefault();
            OtherConstants.isSuccessful = true;
            return constructResponse(userDetail);

        }

        [AllowAnonymous]
        [HttpGet("statement")]

        public async Task<BaseResponse> GetAccountStatement(int agentId, int pageNo, int pageSize)
        {

            var agent = (await _service.Get(x => x.Include(x => x.Accounts), x => x.Id == agentId)).SingleOrDefault();

            var ledgers = (await _entriesService.Get(x => x.Include(x => x.Transaction)

            .ThenInclude(x => x.SalesInvoice)
                        .ThenInclude(x => x.SaleLineItem)
                            .ThenInclude(x => x.Vehicle)

            .Include(x => x.Transaction)
                   .ThenInclude(x => x.SalesInvoice).ThenInclude(x => x.InsuranceType)
        .Include(x => x.Transaction)
                   .ThenInclude(x => x.SalesInvoice).ThenInclude(x => x.Branch)
            .Include(x => x.Transaction)
                   .ThenInclude(x => x.SalesInvoice).ThenInclude(x => x.Service)
        .Include(x => x.Transaction)
                   .ThenInclude(x => x.SalesInvoice).ThenInclude(x => x.PolicyType)
        .Include(x => x.Transaction)
                   .ThenInclude(x => x.SalesInvoice).ThenInclude(x => x.BodyType)



            , x => x.Transaction.UserDetailId == agentId)).GroupBy(x => x.TransactionId).Select(

                x => new
                {
                    Key = x.Key,
                    Value = x
                }
                );

            List<dynamic> accountStatement = new List<dynamic>();
            PageConfig page = new PageConfig();
            int count = 0;
            page.TotalPages = ledgers.ToList().Count / pageNo;
            page.CurrentPage = pageNo;
            page.PageSize = pageSize;
            decimal debitBalance = 0;
            // decimal creditBalance = 0;
            var list = ledgers.Skip(pageNo * pageSize).Take(pageSize).ToList();
            var lastItem = list.Last();
            foreach (var item in list)
            {
                count++;

                AgentStatementDTO debit = new AgentStatementDTO();
                debit.Num = count;
                if (item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id) != null)
                {

                    debitBalance += item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id).Amount;


                    debit.InvoiceDate = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id).Transaction.SalesInvoice.SalesInvoiceDate;
                    debit.CustomerName = "x => x.DebitAccountId ==agent.Accounts.Id";
                    debit.PolicyNumber = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id).Transaction.SalesInvoice.SaleLineItem.SingleOrDefault().PolicyNumber;
                    debit.PolicyType = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.PolicyType?.Name;
                    debit.ServiceType = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.Service?.Name;
                    debit.InsuranceType = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.InsuranceType?.Name;
                    debit.Vehicle = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.SaleLineItem?.
                                SingleOrDefault().Vehicle?.Make + " " + " | " +
                                item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.SaleLineItem?.
                                SingleOrDefault().Vehicle?.Model;
                    debit.BodyType = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.BodyType?.Name;
                    debit.RefNo = item.Value.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Transaction?.TransactionReferenceNumber;
                    debit.Debit = item?.Value?.SingleOrDefault(x => x.DebitAccountId == agent.Accounts.Id)?.Amount;
                    debit.Credit = 0;
                    debit.Balance = debitBalance;

                }
                else
                {
                    debitBalance += -(item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id).Amount);

                    debit.InvoiceDate = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id).Transaction.SalesInvoice.SalesInvoiceDate;
                    debit.CustomerName = "x => x.CreditAccountId ==agent.Accounts.Id";
                    debit.PolicyNumber = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id).Transaction.SalesInvoice.SaleLineItem.SingleOrDefault().PolicyNumber;
                    debit.PolicyType = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.PolicyType?.Name;
                    debit.ServiceType = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.Service?.Name;
                    debit.InsuranceType = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.InsuranceType?.Name;
                    debit.Vehicle = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.SaleLineItem?.
                                SingleOrDefault().Vehicle?.Make + " " + " | " +
                                item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.SaleLineItem?.
                                SingleOrDefault().Vehicle?.Model;
                    debit.BodyType = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.SalesInvoice?.BodyType?.Name;
                    debit.RefNo = item.Value.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Transaction?.TransactionReferenceNumber;
                    debit.Debit = item?.Value?.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Amount;
                    debit.Credit = -(item?.Value?.SingleOrDefault(x => x.CreditAccountId == agent.Accounts.Id)?.Amount);
                    debit.Debit = 0;
                    debit.Balance = debitBalance;
                }


                accountStatement.Add(debit);


            }


            OtherConstants.isSuccessful = true;
            page.Data.AddRange(accountStatement);
            return constructResponse(page);




        }


        [AllowAnonymous]
        [HttpGet("Download")]
        public async Task<ActionResult> Downnload()
        {
            string path = Path.Combine(_env.ContentRootPath, "EmailTemplates\\statement.html");
            
            if (System.IO.File.Exists(path))
            {
                string html = System.IO.File.ReadAllText(path);
           
                var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4Plus,
    },
                Objects = {
        new ObjectSettings() {
            PagesCount = true,
            HtmlContent = html,
            WebSettings = { DefaultEncoding = "utf-8" },
            HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
        }
    }
            };
            byte[] pdf = _converter.Convert(doc);



            return new FileContentResult(pdf, "application/pdf");
            }
            return NotFound();

        }

        [HttpGet("GetCurrentAccountStatement")]
        public async Task<BaseResponse> GetCurrentAccountStatement(int agentId)
        {
            var agent = (await _service.Get(x => x.Id == agentId)).SingleOrDefault();
            var Debit_Balance = (await _entriesService.Get(x => x.CreditAccountId == agent.DefaultAccountId)).Sum(x => x.Amount);
            var Credit_Balance = (await _entriesService.Get(x => x.CreditAccountId == agent.DefaultAccountId)).Sum(x => x.Amount);
            var OpenBalance = Debit_Balance - Credit_Balance;
            var AgentOpenBalance = new CurrentAgentStatementDTO()
            {
                OpenBalance = OpenBalance
            };
            return constructResponse(AgentOpenBalance);
        }

    }
    public class CurrentAgentStatementDTO
    {
        public decimal OpenBalance { get; set; }

    }
}
