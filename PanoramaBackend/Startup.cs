using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NukesLab.Core.Common;
using PanoramaBackend.Api;
using PanoramaBackend.Data;
using PanoramaBackend.Services.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PanoramaBackend.Services.Data.DTOs;
using Microsoft.AspNetCore.Identity;
using NukesLab.Core.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using static NukesLab.Core.Common.Constants;
using System.Text;
using PanoramaBackend.Services;
using PanoramaBackend.Data.Repository;
using PanoramaBackend.Services.Services;
using Microsoft.Extensions.Logging.Console;
using PanoramaBackend.Api.Jobs;
using PanoramaBackend.Data.CatalogDb.Repos;
using PanoramaBackend.Data.CatalogDb;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Nest;
using PanoramaBackend.Data.Entities;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.FileProviders;
using DinkToPdf.Contracts;
using DinkToPdf;
using PanoramaBackend.Services.Reports;
using Wkhtmltopdf.NetCore;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PanoramaBackend.Service.Syncronization;
using Microsoft.Win32;
using MongoDB.Driver.Core.Configuration;
//using AIB.Data;
//using PanoramaBackend.Service.Syncronization;
namespace PanoramaBackend
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private IServiceProvider serviceProvider;
        private readonly string registryPath;
        //public static readonly ILoggerFactory loggerFactory = new LoggerFactory(new[] {
        //      new ConsoleLoggerProvider((_, __) => true, true)
        //});
        //public ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
        public Startup(IWebHostEnvironment env)
        {
            _env = env;
            Utils._config = new ConfigurationBuilder().SetBasePath(_env.ContentRootPath)
              .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
          .Build();

            registryPath = Utils._config.GetValue<string>("Registry");
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                string connectionString = "";
                if (key != null)
                {
                    connectionString = key.GetValue("ConnectionString") as string;

                    ConnectionStrings.PortalConnectionString = connectionString;
                }
                else
                {
                    throw new ServiceException("Ask your administrator to load ConnectionString in Component Services");
                }
            }

           
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)


        {
            UpdateableFileProvider fileProvider = new UpdateableFileProvider();
            services.TryAddTransient<ITempDataProvider, SessionStateTempDataProvider>();
            services.TryAddSingleton(fileProvider);
            services.TryAddSingleton<IRazorViewEngine, RazorViewEngine>();
            services.AddMvc().AddRazorRuntimeCompilation(delegate (MvcRazorRuntimeCompilationOptions options)
            {
                options.FileProviders.Add(fileProvider);
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.TryAddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.TryAddTransient<IGeneratePdf, GeneratePdf>();

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddCors();

            services.AddControllers().AddNewtonsoftJson(options =>
            {

                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
           

            });

            //var url = Utils._config["elasticsearch:url"];
            //var defaultIndex = Utils._config["elasticsearch:index"];

            //var settings = new ConnectionSettings(new Uri(url))
            //    .DefaultIndex("sales");

            //AddDefaultMappings(settings);

            //var client = new ElasticClient(settings);

            //services.AddSingleton(client);


            //CreateIndex(client, defaultIndex);

            services.AddAutoMapper(typeof(AutoMapperMappings).Assembly);
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserProvider, IdentityUserProvider>();
            services.AddScoped<UserGenerator>();
            ConnectionStrings.AIBConnectionString = Utils._config.GetConnectionString("AIBConnectionString");
            //services.AddDbContext<AIBContext>(options =>
            //{
            //    options.UseSqlServer(Utils._config.GetConnectionString("AIBConnectionString"));
            //    options.UseInternalServiceProvider(serviceProvider);
            //    //option.EnableSensitiveDataLogging(true);
            //    options.EnableDetailedErrors(true);
            //    options.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }));
            //});
            services.AddDbContext<AMFContext>(option =>
            {
                option.UseSqlServer(ConnectionStrings.PortalConnectionString);
                option.UseInternalServiceProvider(serviceProvider);

                //option.EnableSensitiveDataLogging(true);
                option.EnableDetailedErrors(true);
                option.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }));

            }, ServiceLifetime.Scoped);

            services.AddSwaggerGen();
            services.AddIdentity<ExtendedUser, ExtendedRole>(option =>
            {
                option.Password.RequireDigit = false;
                option.Password.RequiredLength = 8;
                option.Password.RequireNonAlphanumeric = true;
                option.Password.RequireUppercase = false;
                option.Password.RequireLowercase = false;



            }).AddEntityFrameworkStores<AMFContext>()
              .AddDefaultTokenProviders();



            services.AddScoped<RequestScope>(services =>
            {
                var logger = services.GetRequiredService<ILogger<Program>>(); ;
                var mapper = services.GetRequiredService<IMapper>();
                return new RequestScope(services, logger, mapper);
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser().Build();
            });

            services.AddAuthentication()
           .AddCookie()
           .AddJwtBearer(config =>
           {
               config.TokenValidationParameters = new TokenValidationParameters()
               {
                   ValidIssuer = JWTConfiguration.JWTIssuer,
                   ValidAudience = JWTConfiguration.JWTAudience,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTConfiguration.JWTKey)),
                   ClockSkew = TimeSpan.Zero,
                   LifetimeValidator = TokenLifetimeValidator.Validate
               };
           });
            services.AddHostedService<AttendanceCronJobService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IExtendedUserRepository, ExtendedUserRepository>();
            services.AddScoped<IExtendedRoleRepository, ExtendedRoleRepository>();
            services.AddScoped<IUserDetailsService, UserDetailsService>();
            services.AddScoped<IUserDetailsRepository, UserDetailsRepository>();
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<IFileUploader, AzureBlobUploader>();
            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<IAccountsService, AccountsService>();
            services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
            services.AddScoped<IAccountTypeService, AccountTypeService>();
            services.AddScoped<ILedgerEntriesRepository, LedgerEntriesRepository>();
            services.AddScoped<ILedgerEntriesService, LedgerEntriesService>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IPaymentMethodService, PaymentMethodService>();
            services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
            services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            //services.AddScoped<ISyncronizationRepository, SyncronizationRepository>();

            services.AddScoped<IInsuranceTypeRepository, InsuranceTypeRepository>();
            services.AddScoped<IInsuranceTypeService, InsuranceTypeService>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IPaymentAndBillingRepository, PaymentAndBillingRepository>();
            services.AddScoped<IPaymentAndBillingService, PaymentAndBillingService>(); services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IPreferredPaymentMethodRepository, PreferredPaymentMethodRepository>();
            services.AddScoped<IPreferredPaymentMethodService, PreferredPaymentMethodService>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IAttachmentsService, AttachmentsService>();
            services.AddScoped<ITermsRepository, TermsRepository>();
            services.AddScoped<ITermsService, TermsService>();

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddScoped<IAgentRepository, AgentRepository>();
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
            services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();

            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IAccountsMappingRepository, AccountsMappingRepository>();
            services.AddScoped<IAccountsMappingService, AccountsMappingService>();
            services.AddScoped<IAccountsDetailTypeRepository, AccountsDetailTypeRepository>();
            services.AddScoped<IAccountDetailTypeService, AccountDetailTypeService>();
            services.AddScoped<IComissionRateReposiotory, ComissionRateReposiotory>();
            services.AddScoped<IComissionRateService, ComissionRateService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IPaymentCreditRepository, PaymentCreditRepository>();
            services.AddScoped<IPaymentCreditService, PaymentCreditService>();
            services.AddScoped<IReconcilationRepository, ReconcilationRepository>();
            services.AddScoped<IReconcilationService, ReconcilationService>();
            services.AddScoped<ICorrectionRepository, CorrectionRepository>();
            services.AddScoped<ICorrectionService, CorrectionService>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ITaskTodoRepository, TaskTodoRepository>();
            services.AddScoped<ITaskTodoService, TaskTodoService>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<IPriorityRepository, PriorityRepository>();
            services.AddScoped<IPriorityService, PriorityService>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
            services.AddScoped<IAnnoucementService, AnnoucementService>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IExpenseCategoryReposiotory, ExpenseCategoryRepository>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IPayrollRepository, PayrollRepository>();
            services.AddScoped<IPayrollService, PayrollService>();
            services.AddScoped<ExpenseExcel>();
            services.AddScoped<IVacationApplicationRepository, VacationApplicationRepository>();
            services.AddScoped<IVacationApplicationService, VacationApplicationService>();

            services.AddScoped<ISetupClientRepository, SetupClientRepository>();
            services.AddScoped<ISetupClientService, SetupClientService>();
            services.AddScoped<IUserCompanyInformationRepository, UserCompanyInformationRepository>();
            services.AddScoped<IUserCompanyInformationService, UserCompanyInformationService>();


            //services.AddScoped<IElasticClient, ElasticClient>();
            //services.AddScoped<ICompanyRepo, CompanyRepo>();
            //services.AddScoped<ICardInfoRepo, CardInfoRepo>();
            //services.AddScoped<IDatabasesRepo, DatabasesRepo>();
            //services.AddScoped<IElasticPoolRepo, ElasticPoolRepo>();
            //services.AddScoped<IOrderRepo, OrderRepo>();
            //services.AddScoped<IPaymentHistoryRepo,PaymentHistoryRepo>();
            //services.AddScoped<ISeverRepo, SeverRepo>();
            //services.AddScoped<IStripeConfigurationRepo, StripeConfigurationRepo>();
            //services.AddScoped<IStripeService, StripeService>();
            //services.AddScoped<ITokenService, TokenService>();
            //services.AddScoped<ISubscriptionPlanRepo, SubscriptionPlanRepo>();
            //services.AddScoped<ITenantsRepo, TenantsRepo>();

            #region EmployeeService&Repos
            services.AddScoped<IEmployeeDetailsRepository, EmployeeDetailsRepository>();
            services.AddScoped<IEmploymentDetailService, EmploymentDetailService>();

            services.AddScoped<ICompensationRepository, CompensationRepository>();
            services.AddScoped<ICompensationService, CompensationService>();

            services.AddScoped<IVacationPolicyRepository, VacationPolicyRepository>();
            services.AddScoped<IVacationPolicyService, VacationPolicyService>();

            services.AddScoped<IBenefitsAndDeductionRepository, BenefitsAndDeductionRepository>();
            services.AddScoped<IBenefitsAndDeductionService, BenefitsAndDeductionService>();

            services.AddScoped<IBenefitsRepository, BenefitsRepository>();
            services.AddScoped<IBenefitsService, BenefitsService>();

            services.AddScoped<IDeductionRepository, DeductionRepository>();
            services.AddScoped<IDeductionService, DeductionService>();

            services.AddScoped<IBDTypeRepositoryy, BDTypeRepository>();
            services.AddScoped<IBDTypeService, BDTypeService>();

            services.AddScoped<IEmployeeFilesRepository, EmployeeFilesRepository>();
            services.AddScoped<IEmployeeFilesService, EmployeeFilesService>();

            services.AddScoped<IBankDetailRepository, BankDetailRepository>();
            services.AddScoped<IBankDetailService, BankDetailService>();

            services.AddScoped<IEmploymentStatusRepository, EmploymentStatusRepository>();
            services.AddScoped<IEmploymentStatusService, EmploymentStatusService>();

            services.AddScoped<IStaffOffBoradingRepository, StaffOffBoradingRepository>();
            services.AddScoped<IStaffOnBoardingService, StaffOnBoardingService>();

            services.AddScoped<ILeaveApplicationRepository, LeaveApplicationRepository>();
            services.AddScoped<ILeaveApplicationService, LeaveApplicationService>();

            services.AddScoped<ITeamsRepository, TeamsRepository>();
            services.AddScoped<ITeamsService, TeamsService>();

            services.AddScoped<IBodyTypeRepository, BodyTypeRepository>();
            services.AddScoped<IBodyTypeService, BodyTypeService>();


            services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
            services.AddScoped<IPolicyTypeService, PolicyTypeService>();

            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IServiceService, ServiceService>();

            #endregion







        }
        //private static void AddDefaultMappings(ConnectionSettings settings)
        //{
        //    settings.
        //    DefaultMappingFor<SalesInvoice>(m => m
        //    );
        //}

        //private static void CreateIndex(IElasticClient client, string indexName)
        //{
        //    var createIndexResponse = client.Indices.Create(indexName,
        //        index => index.Map<SalesInvoice>(x => x.AutoMap())
        //    );
        //} 
        //This method gets called by the runtime.Use this method to configure the HTTP request pipeline.


        public async void Configure(IApplicationBuilder app)
        {

            ServiceActivator.Configure(app.ApplicationServices);
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AMFContext>();
                var databaseExists = dbContext.Database.CanConnect();

                if (!databaseExists)
                {
                    dbContext.Database.Migrate();
                    // Additional code to seed data or perform other initializations if needed
                }
                else
                {
                    Console.WriteLine("ALREADY EXIST");
                }
            }

            ExpenseExcel.ConfigureExcel();
            //var syncronizationService = ServiceActivator.GetScope().ServiceProvider.GetService<ISyncronizationRepository>();
            //await syncronizationService.MergeAll();
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(SwaggerConfiguration.SwaggerEndPointURL, SwaggerConfiguration.SwaggerEndPointName);
                });
            }
            else
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(SwaggerConfiguration.SwaggerEndPointURL, SwaggerConfiguration.SwaggerEndPointName);
                });
                app.UseHsts();
            }
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(_env.ContentRootPath, "Uploads")),
                RequestPath = "/uploads"
            });
            app.UseCors(x => x
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
                        .SetIsOriginAllowed(origin => true) // allow any origin
                       
                        );; // allow credentials
                        
            app.UseRouting();


            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();

            app.UseHttpsRedirection();




            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

