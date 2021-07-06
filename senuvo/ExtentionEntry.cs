using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using senuvo.Api;
using System;
using System.Collections.Generic;
using Yuansfer.Helper;
using DirectScale.Disco.Extension.Hooks.Commissions;
using DirectScale.Disco.Extension.Hooks;
using DirectScale.Disco.Extension.Hooks.Autoships;
using DirectScale.Disco.Extension.Hooks.Orders;
using senuvo.Merchants.Ewallet.Interfaces;
using senuvo.Services.HttpClientService;
using senuvo.Merchants.Ewallet.Ewallet;
using senuvo.Hooks;
using Hooks;

namespace senuvo
{
    public class ExtensionEntry : IExtension
    {

        public void ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);
            RegisterHooks(services);
            RegisterApis(services);
            RegisterViews(services);
            RegisterMerchants(services);
        }
        private void RegisterMerchants(IServiceCollection services)
        {
            services.AddSingleton<ICommissionMerchant, EwalletMoneyOutMerchant>();
            services.AddScoped<IMoneyInMerchant, ALIPAY>();
            services.AddScoped<IMoneyInMerchant, WECHAT>();
        }
        private void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IsenuvoService, senuvoService>();
            services.AddScoped<IHttpClientService, HttpClientService>();
            services.AddScoped<IEwalletRepository, EwalletRepository>();
            services.AddScoped<IEwalletService, EwalletService>();
        }
        private void RegisterHooks(IServiceCollection services)
        {
            services.AddScoped<IDaily, Daily>();
            services.AddScoped<IHook<DailyRunHookRequest, DailyRunHookResponse>, Daily>();

        }
        private void RegisterApis(IServiceCollection services)
        {
            services.AddScoped<IApiEndpoint, Endpoint1>();
            //services.AddScoped<IApiEndpoint, Endpoint2>();
            //services.AddScoped<IApiEndpoint, Endpoint21>();
        }
        private void RegisterViews(IServiceCollection services)
        {

        }

        //public void Initialize(IHookService hookService)
        //{
        //    hookService.Commisions.DailyRun.Override = (request, func) =>
        //    //hookService.AutoShips.DailyRun.Override = (request, func) =>
        //    {
        //        var result = func(request);
        //        return Hooks.Daily.DailyRunAfter(request);
        //    };
        //}
    }


    public class ALIPAY : RedirectMoneyInMerchant
    {
        private readonly ICompanyService _companyService;
        private readonly string _className;
        private readonly IAssociateService _associateService;

        public ALIPAY(ICompanyService companyService, IAssociateService distributorService) : base(new MerchantInfo { Id = 9322, MerchantName = "Yuansfer AliPay (USD)", DisplayName = "AliPay", Currency = "USD" })
        {
            _companyService = companyService;
            _associateService = distributorService;
            _className = GetType().FullName;
        }

        public override PaymentResponse ChargePayment(int associateId, int orderNumber, Address billingAddress, double amount, string currencyCode, string redirectUrl)
        {
            Associate associate = _associateService.GetAssociate(associateId);
            //DateTime yesterday = DateTime.Now.Date.AddDays(-1);
            if (associate.SignupDate >= /*yesterday && associate.SignupDate <*/ DateTime.Now.Date)
            {
                _associateService.SetAssociateStatus(associateId, (int)AssociateStatus.OnHold);
            }

            var result = new PaymentResponse();
            bool productiveEnvironment = !_companyService.GetRootURL().Contains("stage");
            productiveEnvironment = true;
            string errorUrl = "https://senuvo.shop.directscalestage.com/www/home";
            if (productiveEnvironment)
            {
                errorUrl = "https://senuvo.shop.directscale.com/www/home";
            }

            try
            {
                string vendor = "alipay";
                YuansferSignatureResponse data = Yuansfer.Helper.Util.Login(vendor, amount, orderNumber.ToString(), productiveEnvironment);

                //throw new Exception(_companyService.GetRootURL());

                if (data == null)
                {
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = errorUrl + "?yuansfail";
                }
                else if
                    (data.ret_code == "000100")// Ok
                {
                    //result.Redirect = true;
                    //result.ReferenceNumber = yuansferSignature.reference;
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = data.result.cashierUrl;
                }
                else
                {
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = errorUrl + "?" + data.ret_code;
                }
                return result;

            }
            catch (Exception e)
            {
                result.Merchant = MerchantInfo.Id;
                result.PaymentType = MerchantInfo.MerchantName;
                result.Amount = amount;
                result.Currency = currencyCode;
                result.Merchant = associateId;
                result.OrderNumber = orderNumber;
                result.Status = PaymentStatus.Pending;
                result.RedirectURL = errorUrl + "?" + e.Message + "__" + (e.InnerException != null ? e.InnerException.Message : "");

                return result;
            }




        }

        public override PaymentResponse RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            var res = new PaymentResponse();//Third Part Call
            res.Amount = refundAmount;
            res.Currency = currencyCode;
            res.OrderNumber = orderNumber;
            res.Status = PaymentStatus.Rejected;

            return res;

        }
    }

    public class WECHAT : RedirectMoneyInMerchant
    {
        private readonly ICompanyService _companyService;
        private readonly string _className;
        private readonly IAssociateService _associateService;

        public WECHAT(ICompanyService companyService, IAssociateService distributorService) : base(
            new MerchantInfo { Id = 9323, MerchantName = "Yuansfer weChat (USD)", DisplayName = "weChat", Currency = "USD" })
        {
            _associateService = distributorService;
            _companyService = companyService;
            _className = GetType().FullName;
        }

        public override PaymentResponse ChargePayment(int associateId, int orderNumber, Address billingAddress, double amount, string currencyCode, string redirectUrl)
        {
            bool productiveEnvironment = !_companyService.GetRootURL().Contains("stage");
            productiveEnvironment = true;

            string errorUrl = "https://senuvo.shop.directscalestage.com/www/home";
            if (productiveEnvironment)
            {
                errorUrl = "https://senuvo.shop.directscale.com/www/home";
            }

            Associate associate = _associateService.GetAssociate(associateId);
            //DateTime yesterday = DateTime.Now.Date.AddDays(-1);
            if (associate.SignupDate >= /*yesterday && associate.SignupDate < */DateTime.Now.Date)
            {
                _associateService.SetAssociateStatus(associateId, (int)AssociateStatus.OnHold);
            }
            var result = new PaymentResponse();
            try
            {

                string vendor = "wechatpay";
                YuansferSignatureResponse data = Yuansfer.Helper.Util.Login(vendor, amount, orderNumber.ToString(), productiveEnvironment);
                if (data == null)
                {
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = errorUrl + "?yuansfail";
                }
                else if
                    (data.ret_code == "000100")// Ok
                {
                    //result.Redirect = true;
                    //result.ReferenceNumber = yuansferSignature.reference;
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = data.result.cashierUrl;
                }
                else
                {
                    result.Merchant = MerchantInfo.Id;
                    result.PaymentType = MerchantInfo.MerchantName;
                    result.Amount = amount;
                    result.Currency = currencyCode;
                    result.Merchant = associateId;
                    result.OrderNumber = orderNumber;
                    result.Status = PaymentStatus.Pending;
                    result.RedirectURL = errorUrl + "?" + data.ret_code;
                }
                return result;

            }
            catch (Exception e)
            {
                result.Merchant = MerchantInfo.Id;
                result.PaymentType = MerchantInfo.MerchantName;
                result.Amount = amount;
                result.Currency = currencyCode;
                result.Merchant = associateId;
                result.OrderNumber = orderNumber;
                result.Status = PaymentStatus.Pending;
                result.RedirectURL = errorUrl + "?" + e.Message + "__" + (e.InnerException != null ? e.InnerException.Message : "");
                return result;
            }




        }

        public override PaymentResponse RefundPayment(string payerId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string referenceNumber, string transactionNumber, string authorizationCode)
        {
            var res = new PaymentResponse();//Third Part Call
            res.Amount = refundAmount;
            res.Currency = currencyCode;
            res.OrderNumber = orderNumber;
            res.Status = PaymentStatus.Rejected;

            return res;
        }

    }

    public enum AssociateStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3,
        Suspended = 4,
        Terminated = 5,
        OnHold = 6
    }


}