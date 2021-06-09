using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using senuvo.Merchants.Ewallet.Models;
using senuvo.Services.HttpClientService;
using senuvo.Merchants.Ewallet.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using senuvo.Merchants.Moneyout.Models;

namespace senuvo.Merchants.Ewallet.Ewallet
{
    public class EwalletService : IEwalletService
    {
        private readonly ILogger _logger;
        private readonly IEwalletRepository _EwalletRepository;
        private static readonly string _className = typeof(EwalletService).FullName;
        private readonly IHttpClientService _httpClientService;
        private readonly IMoneyOutService _moneyOutService;
        private readonly ICurrencyService _currencyService;
        private readonly IAssociateService _associateService;

        public EwalletService(IEwalletRepository repository, ILogger logger, IHttpClientService httpClientService, IMoneyOutService moneyOutService, ICurrencyService currencyService, IAssociateService associateService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _EwalletRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _moneyOutService = moneyOutService ?? throw new ArgumentNullException(nameof(moneyOutService));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _associateService = associateService ?? throw new ArgumentNullException(nameof(associateService));
        }

        public string CreateCustomer(Application app, int responseAssociateId)
        {
            if (app.AssociateId == 0)
                app.AssociateId = responseAssociateId;
            var settings = _EwalletRepository.GetSettings();
            CustomerDetails request = new CustomerDetails
            {
                FirstName = app.FirstName,
                LastName = app.LastName,
                ExternalCustomerID = app.AssociateId.ToString(),
                BackofficeID = app.BackOfficeId,
                CompanyID = settings.CompanyId,
            };

            //var result = CallEwallet("api/Customer/CreateCustomer", request);

            CallEwallet("api/Customer/CreateCustomer", request);

            //var jsonString = result?.Content?.ReadAsStringAsync();
            //jsonString.Wait();
            //var jobject = jsonString?.Result;
            //dynamic data = JObject.Parse(jobject);

            //return data;

            return responseAssociateId.ToString();
        }

        public void UpdateCustomer(Associate req)
        {
            var settings = _EwalletRepository.GetSettings();
            CustomerDetails request = new CustomerDetails
            {
                FirstName = req.DisplayFirstName,
                LastName = req.DisplayLastName,
                ExternalCustomerID = req.AssociateId.ToString(),
                BackofficeID = req.BackOfficeId,
                CompanyID = settings.CompanyId,
            };
            CallEwallet("api/Customer/UpdateCustomer", request);
        }

        public EwalletSettings GetSettings()
        {
            return _EwalletRepository.GetSettings();
        }

        public string CreateToken()
        {
            try
            {
                EwalletSettings settings = _EwalletRepository.GetSettings();
                TokenRequest trequest = new TokenRequest { client_id = settings.CompanyId, username = settings.Username, password = settings.Password };
                string jsonData = JsonConvert.SerializeObject(trequest);

                string apiUrl = settings.ApiUrl + "token";

                HttpResponseMessage data = _httpClientService.PostRequest(apiUrl, trequest);

                if (data?.StatusCode.ToString() == "OK")
                {
                    var jsonString = data?.Content?.ReadAsStringAsync();
                    jsonString.Wait();

                    var jobject = jsonString?.Result?.ToString();
                    dynamic jdata = JObject.Parse(jobject);
                    return jdata?.access_token;
                }
                else
                {
                    throw new Exception(data?.StatusCode.ToString() + data?.Content.ToString() + " URL: "+ apiUrl + " json: "+ jsonData + " reason: " + data.ReasonPhrase);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public dynamic CallEwallet(string requestUrl, object requestData)
        {
            try
            {
                string token = CreateToken();
                var settings = _EwalletRepository.GetSettings();
               
                if (!string.IsNullOrEmpty(token))
                {
                    token = "Bearer " + token;
                }
                else
                {
                    throw new Exception($"Error occured at Method {System.Reflection.MethodBase.GetCurrentMethod().Name} and Error = Token is NULL!");
                }

                var jsonData = JsonConvert.SerializeObject(requestData);
                var apiUrl = settings.ApiUrl + requestUrl;
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(apiUrl));
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var result = _httpClientService.MakeRequestByToken(request, "Authorization", token);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result;
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception($"Error occured at Method {System.Reflection.MethodBase.GetCurrentMethod().Name} and Error = Invalid Token!");
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Error occured at Method {System.Reflection.MethodBase.GetCurrentMethod().Name} and error ='{ex.Message}'");
            }
            return null;
        }

        public PointBalanceResponse GetPointBalance(GetPointBalanceRequest request)
        {
            try
            {
                var settings = _EwalletRepository.GetSettings();
                GetPointBalanceRequest brequest = new GetPointBalanceRequest
                {
                    ExternalCustomerId = request.ExternalCustomerId,
                    CompanyId = settings.CompanyId,
                    PointAccountId = settings.PointAccountId
                };
                var response = CallEwallet("api/CustomerPointBalances/GetCustomerPointBalances", brequest);
                var jsonString = response?.Content?.ReadAsStringAsync();

                jsonString.Wait();
                var jobject = jsonString?.Result;
                dynamic data = JObject.Parse(jobject);

                if (data.status == "Success" && data.data != null)
                {
                    return new PointBalanceResponse
                    { Amount = data.data.amount, HoldAmount = data.data.holdAmount };
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "{1} {2}", e.Message, e.StackTrace);
            }
            return new PointBalanceResponse();
        }

        public string CreatePointTransaction(CustomerPointTransactionsRequest request)
        {
            try
            {
                var settings = _EwalletRepository.GetSettings();

                request.CompanyId = settings.CompanyId;
                request.PointAccountID = settings.PointAccountId;

                var response = CallEwallet("api/CustomerPointTransactions/CreatePointTransaction", request);
                var jsonString = response?.Content?.ReadAsStringAsync();
                jsonString.Wait();
                var jobject = jsonString?.Result;
                dynamic data = JObject.Parse(jobject);
                if (data.status == "Success" && data.data != null)
                    return "success";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_className}.CreatePointTransaction", "Exception thrown when creating point Transaction for Associate: " + request.ExternalCustomerID + ". Exception: " + ex.Message);
            }
            return "";
        }

        public PaymentResponse CreditPayment(string payorId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string cardNumber, string transactionNumber, string authorizationCode)
        {
            var res = new PaymentResponse();
            string response = "";
            paymentAmount = _currencyService.Round(paymentAmount, currencyCode);
            refundAmount = _currencyService.Round(refundAmount, currencyCode);

            var refundTrans = new RefundTransaction
            {
                Id = transactionNumber,
                RefundData = new RefundData
                {
                    Amount = paymentAmount,
                    PartialAmount = refundAmount,
                    Currency = currencyCode.ToUpper()
                }
            };

            try
            {
                CustomerPointTransactionsRequest data = new CustomerPointTransactionsRequest
                { 
                    Amount = refundTrans.RefundData.Amount, 
                    ExternalCustomerID = payorId,
                    RedeemType = RedeemType.Commission, 
                    TransactionType = TransactionType.Credit,
                    ReferenceNo = transactionNumber,
                    Comment = $"Commissions from Associate:{payorId} Amount:{paymentAmount.ToString()}"
                };

                if (!refundTrans.RefundData.Amount.Equals(refundTrans.RefundData.PartialAmount))
                {
                    data.Amount = refundTrans.RefundData.PartialAmount;
                }

                response = CreatePointTransaction(data);

                if (!string.IsNullOrEmpty(response))
                {
                    if (response.Contains("Error"))
                    {
                        res.TransactionNumber = "";
                    }
                    else
                    {
                        res.Status = PaymentStatus.Accepted;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{_className}.ChargeSavedPayment", "Exception thrown when sending or processing Ewallet payment response for order " + orderNumber + ". Exception: " + e);
            }

            _logger.LogInformation($"{_className}.CreditPayment", $"Processed refund for order {orderNumber}. TransactionId: {transactionNumber}, Amount: {refundTrans.RefundData.Amount}, Returned status: {response}.");

            return new PaymentResponse
            {
                Amount = refundAmount,
                AuthorizationCode = authorizationCode,
                Currency = currencyCode.ToUpper(),
                Merchant = 9012,
                OrderNumber = orderNumber,
                TransactionNumber = transactionNumber,
                ResponseId = "0",
                PaymentType = "Credit",
                Response = response,
                Status = response.Equals("success", StringComparison.OrdinalIgnoreCase) ? PaymentStatus.Accepted : PaymentStatus.Rejected
            };
        }

        public int SetActiveCommissionMerchant(SetActiveCommissionMerchantRequest request)
        {
            var result = _moneyOutService.AutoProvisionAccount(request.AssociateId, request.MerchantId);

            if (result?.MerchantId != 0)
            {
                return 1;
            }

            return 0;
        }
    }
}
