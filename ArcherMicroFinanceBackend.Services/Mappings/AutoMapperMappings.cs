﻿
using PanoramaBackend.Services.Data.DTOs;
using PanoramaBackend.Data.Entities;
using AutoMapper;
using NukesLab.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using PanoramaBackend.Data.Repository;
using PanoramaBackend.Services.Services;
using PanoramaBackend.Data.DTOs;

namespace PanoramaBackend.Services.Mapper
{
    public class AutoMapperMappings : Profile
    {
        public AutoMapperMappings()
        {

			this.CreateMap<Payroll>();
            this.CreateMap<SetupClient>();
            this.CreateMap<UserCompanyInformation>();
            this.CreateMap<Announcement>();
            this.CreateMap<UserDetails>()
				.ForMember(x => x.Addresses, x => x.MapFrom(x => x.Addresses))
				.ForMember(x => x.Attachments, x => x.MapFrom(x => x.Attachments))
				.ForMember(x => x.PaymentAndBilling, x => x.MapFrom(x => x.PaymentAndBilling));
			

			this.CreateMap<TeamMemberDTO, ExtendedUser>().ForMember(x => x.UserDetails,x=>x.MapFrom(x=>x.UserDetails));
			this.CreateMap<Payment>();
			this.CreateMap<Accounts>().ForMember(x => x.DebitLedgarEntries, x => x.MapFrom(x => x.DebitLedgarEntries))
				.ForMember(x => x.CreditLedgarEntries, x => x.MapFrom(x => x.CreditLedgarEntries)).ReverseMap()
				.ForMember(x => x.CreditPayment, x => x.MapFrom(x => x.CreditPayment)).ReverseMap()
				.ForMember(x => x.DepositPayments, x => x.MapFrom(x => x.DepositPayments)).ReverseMap()
				.ForMember(x => x.AccountDetailType, x => x.MapFrom(x => x.AccountDetailType)).ReverseMap()
				.ForMember(x => x.SubAccounts, x => x.MapFrom(x => x.SubAccounts)).ReverseMap()
				.ForMember(x => x.UserDetail, x => x.MapFrom(x => x.UserDetail)).ReverseMap();
			
			this.CreateMap<AccountDetailType>();
			this.CreateMap<AccountType>();
			this.CreateMap<LedgarEntries>();
			this.CreateMap<Login>();
			this.CreateMap<PaymentMethod>();
			this.CreateMap<Refund>();
			this.CreateMap<SalesInvoice>();
			this.CreateMap<Branch>();
			this.CreateMap<VacationApplication>();

			this.CreateMap<SalesInvoice>();
			
			this.CreateMap<Transaction,LedgarEntries >().ForMember(x => x.Transaction,x=>x.MapFrom(x=>x.LedgarEntries))
					.ForMember(x=>x.Transaction,x=>x.MapFrom(x=>x.LedgarEntries));
			this.CreateMap<InsuranceType>();
			this.CreateMap<ComissionRate>();
			this.CreateMap<AccountsMapping>();
			this.CreateMap<Vehicle>();
			//this.CreateMap<InsuranceType>();
			this.CreateMap<PanoramaBackend.Data.Entities.Transaction>();
			this.CreateMap<Address>();

			this.CreateMap<PaymentAndBilling>();
			this.CreateMap<PreferredPaymentMethod>();
			this.CreateMap<Attachments>();
			this.CreateMap<Terms>();
			this.CreateMap<BodyType>();
			this.CreateMap<PanoramaBackend.Data.Entities.Service>();
			this.CreateMap<PolicyType>();
			this.CreateMap<Reconcilation>();
			this.CreateMap<PanoramaBackend.Data.Entities.CompanyInformation>();
			this.CreateMap<Documents>();
			this.CreateMap<Statement, SalesStatementReconcilation>().ReverseMap();

			this.CreateMap<EmploymentDetails>();
			this.CreateMap<Compensation>();
			this.CreateMap<VacationPolicy>();
			this.CreateMap<BenefitsAndDeduction>();
			this.CreateMap<Benefits>();
			this.CreateMap<Deduction>();
			this.CreateMap<BDType>();
			this.CreateMap<EmployeeFiles>();
			this.CreateMap<BankDetails>();
			this.CreateMap<EmploymentStatus>();
			this.CreateMap<StaffOffBoarding>();
			this.CreateMap<LeaveApplication>();
			this.CreateMap<Teams>();
			this.CreateMap<TaskTodo>();
			this.CreateMap<Status>();
			this.CreateMap<Priority>();
			this.CreateMap<Expense>();
			this.CreateMap<ExpenseCategory>();
            CreateMap<UserCompanyInformation, SetupCompanyAccountDTO>()
        .ForMember(dest => dest.LogoBase64, opt => opt.Ignore());
            CreateMap<SetupCompanyAccountDTO, UserCompanyInformation>();

			CreateMap<SetupCompanyAccountDTO, ExtendedUser>()
	   .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.LoginEmail))
	   .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress));
    

            CreateMap<SetupCompanyAccountDTO, UserDetails>()
                   .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
       .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DisplayNameAs, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));

        }
    }


	public static class ExtensionMethods
	{
		public static IMappingExpression<TEntity, TEntity> CreateMap<TEntity>(this Profile profile)
			where TEntity : IBaseEntity
		{
			var map = profile.CreateMap<TEntity, TEntity>()
				//.ForMember(o => o.TenantId, o => o.Ignore())
				.ForMember(o => o.CreateTime, o => o.Ignore())
				.ForMember(o => o.CreateUserId, o => o.Ignore())
				.ForMember(o => o.EditTime, o => o.Ignore())
				.ForMember(o => o.EditUserId, o => o.Ignore())
				.ForMember(o => o.IsDeleted, o => o.Ignore())
				.ForMember(o => o.IsNew, o => o.Ignore())
				.ForMember(o => o.Timestamp, o => o.Ignore());

            //var isIdAutoGenerated = (Attribute.GetCustomAttribute(typeof(TEntity).GetProperty("Id"), typeof(DatabaseGeneratedAttribute)) as DatabaseGeneratedAttribute)?
            //    .DatabaseGeneratedOption != DatabaseGeneratedOption.None;

            //if (isIdAutoGenerated)
            //{
            //    map.ForMember("Id", o => o.Ignore());
            //}

            return map;
		}
	}
}
