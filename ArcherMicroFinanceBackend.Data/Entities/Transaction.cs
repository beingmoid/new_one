﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NukesLab.Core.Common;
using PanoramaBackend.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PanoramaBackend.Data.Entities
{
    public class Transaction:BaseEntity<int>
    {
        public DateTime TransactionDate { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Memo { get; set; }
        public int? UserDetailId { get; set; } //SaleAgent / Insurance Company /
        public int? SalesInvoiceId { get; set; } //Invoice
        public int? PaymentId { get; set; } //ReceivePayment
        public Payment Payment { get; set; }
        public Guid? BranchId { get; set; }
        public Branch Branch { get; set; }
        public string TransactionReferenceNumber { get; set; }
        public int? RefundId { get; set; } //Refund
        public Refund Refund { get; set; }
        public TransactionTypes? TransactionType { get; set; }
        public SalesInvoice SalesInvoice { get; set; }
        public UserDetails UserDetails { get; set; }

        public int? ExpenseId { get; set; }
        public Expense Expense { get; set; }
        private ICollection<LedgarEntries> _ledger;
        public ICollection<LedgarEntries> LedgarEntries => _ledger ?? (_ledger = new List<LedgarEntries>());
    }
    
    public enum TransactionTypes :int
    {
     
        Invoice=1,
        Payment,
        OpeningBalance,
        InsuranceCredit,
        Transfer,
        Deposit,
        Expense,
        Refund

    }
}
