﻿using NukesLab.Core.Common;
using PanoramaBackend.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PanoramaBackend.Data.Entities
{
    public class Accounts:BaseEntity<int>
    {

        public int? AccountId { get; set; }
        public Accounts Account { get; set; }
        public int AccountDetailTypeId { get; set; }
      
        public AccountDetailType AccountDetailType { get; set; }

        [Column(TypeName ="nvarchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Number { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Description { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string DefaultTaxCode { get; set; }
        public bool? IsSubAccount { get; set; }
        public decimal? OpeningBalanceEquity { get; set; }
        public DateTime? AsOf { get; set; }
        public bool? isDeleteApplicable { get; set; }


        private ICollection<Accounts> _accounts;
        public ICollection<Accounts> SubAccounts => _accounts ?? (_accounts = new List<Accounts>());
        private ICollection<UserDetails> _UserDetail;
        public ICollection<UserDetails> UserDetail => _UserDetail ?? (_UserDetail = new List<UserDetails>());
        private ICollection<AccountsMapping> _AccountsMapping;
        public ICollection<AccountsMapping> AccountsMappings => _AccountsMapping ?? (_AccountsMapping = new List<AccountsMapping>());
        private ICollection<LedgarEntries> _Debitledger;
        public ICollection<LedgarEntries> DebitLedgarEntries => _Debitledger ?? (_Debitledger = new List<LedgarEntries>());
        private ICollection<LedgarEntries> _Creditledger;
        public ICollection<LedgarEntries> CreditLedgarEntries => _Creditledger ?? (_Creditledger = new List<LedgarEntries>());
        private ICollection<Payment> _depositPayment;
        public ICollection<Payment> DepositPayments => _depositPayment ?? (_depositPayment = new List<Payment>());
    
        private ICollection<Payment> _creditPayment;
        public ICollection<Payment> CreditPayment => _creditPayment ?? (_creditPayment = new List<Payment>());

        private ICollection<Expense> _expenses;
        public ICollection<Expense> Expenses => _expenses ?? (_expenses = new List<Expense>());
        private ICollection<Payroll> _payrollExpenseAccount;
        public ICollection<Payroll> PayrollExpenseAccount => _payrollExpenseAccount ?? (_payrollExpenseAccount = new List<Payroll>());


    }
}
