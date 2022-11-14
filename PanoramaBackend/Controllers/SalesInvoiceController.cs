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
using PanoramBackend.Services;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Globalization;
using System.Threading;
using Nest;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Elasticsearch.Net;
using Nest.JsonNetSerializer;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using NLog;
using PanoramBackend.Data.Repository;

namespace PanoramaBackend.Api.Controllers
{

    public class SalesInvoiceController : BaseController<SalesInvoice,int>
    {
        private readonly ISalesInvoiceService _service;
        private readonly IFileUploader _fileUploader;
        private readonly IDocumentService _docService;

        private readonly IServiceProvider _serviceProvider;

        //private readonly IElasticClient _elasticClient;
        private readonly ISalesInvoiceRepository _salesRepo;

//        public string GetElasticsearchBulkJsonFromJson(string jsonStringWithArrayOfObjects, string firstParameterNameOfObjectInJsonStringArrayOfObjects)
//        {
//            return @"{ ""index"":{ } } 
//" + jsonStringWithArrayOfObjects.Substring(1, jsonStringWithArrayOfObjects.Length - 2).Replace(@",{""" + firstParameterNameOfObjectInJsonStringArrayOfObjects + @"""", @" 
//{ ""index"":{ } } 
//{""" + firstParameterNameOfObjectInJsonStringArrayOfObjects + @"""") + @"
//";
//        }
        public SalesInvoiceController(RequestScope requestScope,ISalesInvoiceService service,
                   IFileUploader fileUploader,
                   IServiceProvider serviceProvider,
                   ISalesInvoiceRepository salesRepo,
                   //IElasticClient elasticClient,
                   IDocumentService docService)    
            :base(requestScope,service)
        {
            _service = service;
            _fileUploader = fileUploader;
            _docService = docService;
            _serviceProvider = serviceProvider;
            //_elasticClient = elasticClient;
            _salesRepo = salesRepo;
        }

        [HttpGet("GetPaginated")]
        public async Task<BaseResponse> GetPaginated(int page, int pageSize)
        {
            var data = (await this._service.Get(x => x.Include(x => x.CustomerDetails) //Customer
            .Include(x => x.SalesInvoicePerson)
            //Sales Agent
            .Include(x => x.PaymentMethod)

            .Include(x => x.PolicyType)
            .Include(x => x.Service)
            .Include(x => x.BodyType)
                 .Include(x => x.SaleLineItem)
                                    .ThenInclude(x => x.Vehicle)
            .Include(x => x.InsuranceCompany)
            .Include(x => x.Branch)
            )).ToList();
            //Insurance Company
            
            if (data != null)
            {
                var response = new { data = data.Skip(page * pageSize).Take(pageSize).ToList(), totalRows = data.Count };
         
                OtherConstants.isSuccessful = true;
                return constructResponse(response);
            }
            else
            {
                var response = new { totalRows = 0, data = new List<SalesInvoice>() };
                OtherConstants.isSuccessful = false;
                return constructResponse(response);
            }

        }
        [AllowAnonymous]
        [HttpGet("ElasticSearch")]
        public async Task<BaseResponse> BulkUploadToElasticSearch()
        {
            try
            {
                var documents = (await this._service.Get(x => x.Include(x => x.CustomerDetails) //Customer
            .Include(x => x.SalesInvoicePerson)
            //Sales Agent
            .Include(x => x.PaymentMethod)

            .Include(x => x.PolicyType)
            .Include(x => x.Service)
            .Include(x => x.BodyType)
                 .Include(x => x.SaleLineItem)
                                    .ThenInclude(x => x.Vehicle)
            .Include(x => x.InsuranceCompany)
            .Include(x => x.Branch)
            )).ToList();
            
                
         
                var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200/"));

                var connectionSettings =
                    new ConnectionSettings(pool, sourceSerializer: (builtin, settings) => new JsonNetSerializer(
                        builtin, settings,
                        () => new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, ReferenceLoopHandling = ReferenceLoopHandling.Ignore },
                        resolver => resolver.NamingStrategy = new SnakeCaseNamingStrategy()
                    ));
                connectionSettings.EnableApiVersioningHeader(true);
   
        
            
                connectionSettings.EnableApiVersioningHeader(true);
                var client = new ElasticClient(connectionSettings);
                var jobjs = new List<string>();
                var messages = new List<JObject>();
                documents.ForEach(x =>
                {
                    var j = JsonConvert.SerializeObject(x);
                    jobjs.Add(j);
                });
              //var res=  this.transmitBulkData(jobjs, "sales", "SalesInvocie", client, DateTime.Now, messages);
           
                return constructResponse(null);
            }
       
      

                 catch (Exception ex)
            {
     
                return constructResponse(ex);
      
            }
      


        }
        [AllowAnonymous]
        public async override Task<BaseResponse> Get()
        {
            var sales = (await _service.Get(x => x.Include(x => x.CustomerDetails) //Customer
            .Include(x => x.SalesInvoicePerson)
            //Sales Agent
            .Include(x => x.PaymentMethod)
            .Include(x => x.SaleLineItem)

                                    .ThenInclude(x => x.Vehicle)
                                      .Include(x => x.PolicyType)
            .Include(x => x.Service)
            .Include(x => x.BodyType)
            .Include(x => x.InsuranceCompany)
            .Include(x => x.Branch)
            )); //Insurance Company


            
            OtherConstants.isSuccessful = true;
            OtherConstants.messageType = MessageType.Success;
            return constructResponse(sales);
        }
        //[HttpGet("ElasticSearch")]
        //public async override Task<BaseResponse> Search(string search)
        //{
        //    var sales = _salesRepo.Get(x=>x.)
        //}

        [HttpPost("UploadExcel")]
        public async Task<BaseResponse> UploadExcel([FromForm(Name = "file")] IFormFile file)
        {
            if (file != null)
            {

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    string s = Convert.ToBase64String(fileBytes);
                    var blob = await _fileUploader.UploadFileAsync(file.FileName, fileBytes, "lym-files");
                    var doc = new Documents()
                    {
                        BlobFileName = blob.BlobFileName,
                        BlobURI = blob.BlobURI
                    };
                  var result=  (await _docService.Insert(new[] { doc }));
                    //var headers = _service.GetExcelColumnHeader(blob.BlobURI);
                    OtherConstants.isSuccessful = result.Success;
                    return constructResponse(result.Entities.FirstOrDefault());
                    //blobs.Add(blob);
                }

                //OtherConstants.isSuccessful = true;
                //return new JsonResult(constructResponse(blobs));

            }
            OtherConstants.isSuccessful = false;
            return constructResponse(null);
        }
        [HttpGet("GetColumnHeader")]
        public BaseResponse GetColumnHeader(string fileUrl)
        {
        
         var headers = _service.GetExcelColumnHeader(fileUrl);
         OtherConstants.isSuccessful = true;
         return constructResponse(headers);
        }
        public FileResult downloadFile(string filePath, string fileName)
        {
            IFileProvider provider = new PhysicalFileProvider(filePath);
            IFileInfo fileInfo = provider.GetFileInfo(fileName);
            var readStream = fileInfo.CreateReadStream();
            var mimeType = "application/vnd.ms-excel";
            return File(readStream, mimeType, fileName);
        }
        [HttpPost("BulkUpload")]
        public async Task<BaseResponse> BulkUploadExcelFile([FromForm(Name = "file")] IFormFile file, string? salesDate)
        {
            if (file != null)
            {

                var ms = new MemoryStream();

                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                string s = Convert.ToBase64String(fileBytes);
                var blob = await _fileUploader.UploadFileAsync(file.FileName, fileBytes, "lym-files");
                var doc = new Documents()
                {
                    BlobFileName = blob.BlobFileName,
                    BlobURI = blob.BlobURI
                };
                var filesPath = Directory.GetCurrentDirectory() + "/Uploads/Test.xlsx";
                ms.Close();

                var result = (await _docService.Insert(new[] { doc }));
                ////var headers = _service.GetExcelColumnHeader(blob.BlobURI);
                //if (!System.IO.Directory.Exists(filesPath))
                //{
                //    Directory.CreateDirectory(filesPath);
                //}

                //var filename = Path.GetFileName(file.FileName);
                //var filePath = Path.Combine(filesPath, filename);

                //if (System.IO.File.Exists(filePath))
                //{
                //    System.IO.File.Delete(filePath);
                //}

                //var stream = new FileStream(filePath, FileMode.CreateNew);

                //    await file.CopyToAsync(stream);


                BlobServiceClient blobServiceClient = new BlobServiceClient(Blobs.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("lym-files");
                BlobClient blobClient = containerClient.GetBlobClient(doc.BlobFileName);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



                //FileInfo fileInfo = new FileInfo(doc.BlobFileName);
                //fileInfo.Open(FileMode.Open, FileAccess.Read);
                //File fileInfo = downloadFile(doc.BlobURI, doc.BlobFileName);

                List<SalesInvoice> ListToInsert = new List<SalesInvoice>();
                List<SalesInvoice> RejectedList = new List<SalesInvoice>();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                using (var stream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(true)))
                using (ExcelPackage package = new ExcelPackage(stream))
                {


                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;



                    
                    //char[] charsToTrim = { '*', ' ', '\'' };
                    int col = 1;
                    

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var sales = new SalesInvoice();
                        var saleLine = new SaleLineItem();
                        bool isCustomer = false;
                        bool isBranch = false;
                        bool isPolicyType = false;
                        bool isService = false;
                        bool isBodyType = false;
                        bool isInsuranceType = false;
                        bool isInsuranceCompanies = false;
                        bool isSalesAgent = false;
                        bool isVehicle = false;
                        bool isPaymentMethod = false;
                        bool isRowRejected = false;









                        var Customers = await _serviceProvider.GetRequiredService<ICustomerService>().Get();
                        var Branches = await _serviceProvider.GetRequiredService<IBranchService>().Get();
                        var Policy = await _serviceProvider.GetRequiredService<IPolicyTypeService>().Get();
                        var Services = await _serviceProvider.GetRequiredService<IServiceService>().Get();
                        var Body = await _serviceProvider.GetRequiredService<IBodyTypeService>().Get();
                        var InsuranceTypes = await _serviceProvider.GetRequiredService<IInsuranceTypeService>().Get();
                        var InsuranceCompanies = await _serviceProvider.GetRequiredService<IInsuranceCompanyService>().Get();
                        var SalesAgent = await _serviceProvider.GetRequiredService<IAgentService>().Get();
                        var Vehicles = await _serviceProvider.GetRequiredService<IVehicleService>().Get();
                        var PaymentMethods = await _serviceProvider.GetRequiredService<IPaymentMethodService>().Get();



                        var PolicyNumber = worksheet.Cells[row, col++].Value.ToString();
                        var Customer = worksheet.Cells[row, col++].Value.ToString();
                        var Make = worksheet.Cells[row, col++].Value.ToString();
                        var Model = worksheet.Cells[row, col++].Value.ToString();
                        var BodyType = worksheet.Cells[row, col++].Value.ToString();
                        var PolicyType = worksheet.Cells[row, col++].Value.ToString();
                        var Service = worksheet.Cells[row, col++].Value.ToString();
                        var Branch = worksheet.Cells[row, col++].Value.ToString();
                        var Notes = worksheet.Cells[row, col++].Value.ToString();
                        var PaymentMethod = worksheet.Cells[row, col++].Value.ToString();
                        var InsuranceType = worksheet.Cells[row, col++].Value.ToString();
                        var Premium = Convert.ToDecimal(worksheet.Cells[row, col++].Value.ToString());
                        var Comission = Convert.ToDecimal(worksheet.Cells[row, col++].Value.ToString());
                        var SalesPrice = Convert.ToDecimal(worksheet.Cells[row, col++].Value.ToString());
                        var SalesDate = worksheet.Cells[row, col++].Value.ToString();
                        var Agent = worksheet.Cells[row, col++].Value.ToString();
                        var InsuranceCompany = worksheet.Cells[row, col++].Value.ToString();
                        var Underwritter = worksheet.Cells[row, col++].Value.ToString();


                        isPolicyType = PolicyType != null ? Policy?.FirstOrDefault(x => x.Name == PolicyType) != null : false;
                        isCustomer = Customers != null ? Customers?.FirstOrDefault(x => x.DisplayNameAs == Customer) != null : false;
                        isVehicle = Vehicles != null ? Vehicles?.FirstOrDefault(x => x.Make == Make && x.Model == Model) != null : false;
                        isBodyType = Body != null ? Body?.FirstOrDefault(x => x.Name == BodyType) != null : false;
                        isService = Service != null ? Services?.FirstOrDefault(x => x.Name == Service) != null : false;
                        isBranch = Branches != null ? Branches?.FirstOrDefault(x => x.BranchName == Branch) != null : false;
                        isPaymentMethod = PaymentMethods != null ? PaymentMethods?.FirstOrDefault(x => x.Name == PaymentMethod) != null : false;
                        isInsuranceType = InsuranceTypes != null ? PaymentMethods?.FirstOrDefault(x => x.Name == PaymentMethod) != null : false;
                        isInsuranceCompanies = InsuranceCompanies != null ?
                        InsuranceCompanies?.FirstOrDefault(x => x.DisplayNameAs == InsuranceCompany)?.Id != null : false;
                        isSalesAgent = SalesAgent != null ? SalesAgent?.FirstOrDefault(x => x.DisplayNameAs == Agent) != null : false;

                        if (isCustomer)
                        {
                            sales.CustomerDetailId = Customers.FirstOrDefault(x => x.DisplayNameAs == Customer).Id;
                        }
                        else
                        {
                            var newCustomer = await _serviceProvider.GetRequiredService<ICustomerService>().Insert(new[] { new UserDetails() { DisplayNameAs = Customer, IsCustomer = true } });
                            sales.CustomerDetailId = newCustomer.Entities.FirstOrDefault().Id;
                        }
                        if (isVehicle)
                        {
                            saleLine.VehilcleId = Vehicles?.FirstOrDefault(x => x.Make == Make && x.Model == Model).Id;

                        }
                        else
                        {
                            var newVehicle = await _serviceProvider.GetRequiredService<IVehicleService>().Insert(new[] { new Vehicle() { Make = Make, Model = Model } });
                            saleLine.VehilcleId = newVehicle.Entities.FirstOrDefault().Id;
                        }
                        if (isBodyType)
                        {
                            sales.BodyTypeId = Body?.FirstOrDefault(x => x.Name == BodyType)?.Id;

                        }
                        else
                        {
                            var newBodyType = await _serviceProvider.GetRequiredService<IBodyTypeService>().Insert(new[] { new BodyType() { Name = BodyType } });
                            sales.BodyTypeId = newBodyType.Entities.FirstOrDefault().Id;
                        }
                        if (isPolicyType)
                        {
                            sales.PolicyTypeId = Policy.FirstOrDefault(x => x.Name == PolicyType).Id;
                        }
                        else
                        {
                            var newPolicyType = await _serviceProvider.GetRequiredService<IPolicyTypeService>().Insert(new[] { new PolicyType() { Name = PolicyType } });
                            sales.PolicyTypeId = newPolicyType.Entities.FirstOrDefault().Id;
                        }
                        if (isService)
                        {
                            sales.ServiceId = Services?.FirstOrDefault(x => x.Name == Service)?.Id;
                        }
                        else
                        {
                            var newService = await _serviceProvider.GetRequiredService<IServiceService>().Insert(new[] { new Service() { Name = Service } });
                            sales.ServiceId = newService.Entities.FirstOrDefault().Id;


                        }
                        if (isBranch)
                        {
                            sales.BranchId = Convert.ToInt32(Branches?.FirstOrDefault(x => x.BranchName == Branch).Id);
                        }
                        else
                        {
                            var newBranch = await _serviceProvider.GetRequiredService<IBranchService>().Insert(new[] { new Branch() { BranchName = Branch, BranchAddress = Branch } });
                            sales.BranchId = newBranch.Entities.FirstOrDefault().Id;
                        }
                        if (isPaymentMethod)
                        {
                            sales.PaymentMethodId = PaymentMethods.FirstOrDefault(x => x.Name == PaymentMethod).Id;
                        }
                        if (isInsuranceType)
                        {
                            sales.InsuranceTypeId = InsuranceTypes.FirstOrDefault(x => x.Name == InsuranceType).Id;
                        }
                        if (isSalesAgent)
                        {
                            sales.SalesInvoicePersonId = SalesAgent.FirstOrDefault(x => x.DisplayNameAs == Agent).Id;

                        }
                        else
                        {
                            sales.SalesInvoicePersonId = 930;
                        }

                        if (isInsuranceCompanies)
                        {
                            sales.InsuranceCompanyId = InsuranceCompanies.FirstOrDefault(x => x.DisplayNameAs == InsuranceCompany).Id;
                        }
                       

                              sales.SalesInvoiceDate = DateTime.FromOADate(Convert.ToDouble(SalesDate));
                        sales.UnderWritter = Underwritter;
                        sales.Notes = Notes;

                        saleLine.PolicyNumber = PolicyNumber;
                        saleLine.PremiumPrice = Premium;
                        saleLine.CommisionRate = Comission;
                        saleLine.Gross = Premium;
                        sales.InvoiceType = InvoiceType.Invoice;
                        var com = (saleLine.CommisionRate) / 100;
                        double PriceWithoutvat = (double)saleLine.PremiumPrice / 1.05;
                        double commisionRate = (double)PriceWithoutvat * (double)com;
                        var VAT = (double)saleLine.PremiumPrice - (double)PriceWithoutvat;
                
                        saleLine.ActualComission = (decimal)commisionRate;
                        saleLine.VAT = (decimal)VAT;
                        saleLine.SalesPrice = SalesPrice;
                        saleLine.Net = (decimal) ( (PriceWithoutvat - commisionRate) + VAT);
                        var cultureInfo = new CultureInfo("en-AE");
                
                        sales.SaleLineItem.Add(saleLine);

                        if (sales.CustomerDetailId != null && sales.SalesInvoicePersonId != null && sales.BodyTypeId != null &&
                            sales.PolicyTypeId != null && sales.ServiceId != null && sales.InsuranceCompanyId != null &&
                            sales.InsuranceTypeId != null && sales?.BranchId != null && sales.PaymentMethodId != null &&
                            !string.IsNullOrWhiteSpace(sales.UnderWritter) && !string.IsNullOrWhiteSpace(sales.Notes)
                            && sales?.InvoiceType != null && !string.IsNullOrWhiteSpace(sales.SaleLineItem.FirstOrDefault().PolicyNumber)
                            && sales.SaleLineItem.FirstOrDefault().VehilcleId != null && sales.SaleLineItem.FirstOrDefault().PremiumPrice != null
                            && sales.SaleLineItem.FirstOrDefault().CommisionRate != null && sales.SaleLineItem.FirstOrDefault().SalesPrice != null)
                        {
                            ListToInsert.Add(sales);
                            isRowRejected = false;
                            var status = await _service.Insert(new[] { sales });
                           
                        }
                        else
                        {
                            RejectedList.Add(sales??new SalesInvoice());
                            isRowRejected = true;
                        
                        }
                        col = 1;

                   
                      
                    }
              
                }



                OtherConstants.isSuccessful = true;
                return constructResponse(RejectedList);
            }
            else
            {

                throw new ServiceException("File Does not exists");
            }
        }


        public override async Task<BaseResponse> Put(int id, [FromBody] SalesInvoice entity)
        {
            return constructResponse(await _service.UpdateAsync(id, entity));
        }



    }
}