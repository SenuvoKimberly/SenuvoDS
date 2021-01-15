using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace senuvo.Api
{
    public class Endpoint1 : IApiEndpoint
    {
        private readonly IAssociateService _associateService;
        private readonly IRequestParsingService _requestParsing;
        private readonly IDataService _dataService;
        private readonly IOrderService _orderService;
        private readonly ICompanyService _companyService;
        private readonly IAssociateService _distributorService;


        public Endpoint1(IAssociateService associateService, IDataService dataService, IRequestParsingService requestParsing, IOrderService orderService, ICompanyService companyService, IAssociateService distributorService)
        {
            _associateService = associateService;
            _requestParsing = requestParsing;
            _dataService = dataService;
            _orderService = orderService;
            _companyService = companyService;
            _distributorService = distributorService;
        }

        public ApiDefinition GetDefinition()
        {
            return new ApiDefinition
            {
                Route = "senuvoServices/callback",
                RequireAuthentication = false
            };
        }

        public IApiResponse Post(ApiRequest request)
        {

            try
            {
                var rObject = _requestParsing.ParseBody<E1Request>(request);

                var orderInfo = _orderService.GetOrderByOrderNumber(Int32.Parse(rObject.reference)); 
                var orderPayment = orderInfo.Payments/*.Where(x => x.TransactionNumber == rObject.yuansferId)*/.FirstOrDefault();
                if (orderInfo.Status == OrderStatus.Paid)
                {
                    return new DirectScale.Disco.Extension.Api.ApiResponse
                    {
                        Content = Encoding.ASCII.GetBytes("success"),
                        StatusCode = HttpStatusCode.OK,
                        MediaType = "text/plain"
                    };
                }
                var distributorSummary = _distributorService.GetAssociate(orderInfo.AssociateId);
                if (distributorSummary.StatusId == (int)AssociateStatus.OnHold)
                {
                    _distributorService.SetAssociateStatus(orderInfo.AssociateId, (int)AssociateStatus.Active);

                }

                var paymentStatusUpdate = new OrderPaymentStatusUpdate
                {
                    AuthorizationNumber = rObject.yuansferId,
                    OrderPaymentId = orderPayment.PaymentId,
                    PayType = orderPayment.PayType,
                    ReferenceNumber = orderPayment.Reference,
                    SavedPaymentId = orderPayment.SavedPaymentId,
                    TransactionNumber = orderPayment.TransactionNumber
                };

                if (rObject.status.ToLower() == "success")  //Validate Success
                {
                    paymentStatusUpdate.PaymentStatus = PaymentStatus.Accepted;
                    paymentStatusUpdate.ResponseId = "0";
                    paymentStatusUpdate.ResponseDescription = "Success";
                }
                else
                {
                    paymentStatusUpdate.PaymentStatus = PaymentStatus.Rejected;
                    paymentStatusUpdate.ResponseId = "0";
                    paymentStatusUpdate.ResponseDescription = "Rejected";
                }

                _orderService.FinalizeOrderPaymentStatus(paymentStatusUpdate); // Update OrderPayment
                orderInfo = _orderService.GetOrderByOrderNumber(orderInfo.OrderNumber);

                if (orderInfo.Status == OrderStatus.Paid)
                {
                    _orderService.FinalizeAcceptedOrder(orderInfo); // Complete the order
                }
                else if (orderInfo.Status == OrderStatus.Declined)
                {
                    _orderService.FinalizeNonAcceptedOrder(orderInfo); // mark order failed
                }

                var response = new DirectScale.Disco.Extension.Api.ApiResponse
                {
                    Content = Encoding.ASCII.GetBytes("success"),
                    StatusCode = HttpStatusCode.OK,
                    MediaType = "text/plain"
                };
                return response;

            }
            catch (Exception e)
            {
                return new Ok(new { Status = 350, Message = "error"/*(e.InnerException == null ? e.Message : e.InnerException.Message)*/  });
            }



        }
    }

    public class E1Request
    {
        public string yuansferId { get; set; }
        public string status { get; set; }
        public string amount { get; set; }
        public string time { get; set; }
        public string reference { get; set; }
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