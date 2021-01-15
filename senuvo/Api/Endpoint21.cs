//using DirectScale.Disco.Extension.Api;
//using DirectScale.Disco.Extension.Hooks.Autoships;
//using DirectScale.Disco.Extension.Hooks.Orders;
//using DirectScale.Disco.Extension.Services;
//using Hooks;
//using System.Collections.Generic;

//namespace senuvo.Api
//{
//    public class Endpoint21 : IApiEndpoint
//    {
//        private readonly IOrderService orderService;
//        private readonly IDataService dataService;

//        public Endpoint21(IOrderService orderService, IDataService dataService)
//        {
//            this.orderService = orderService;
//            this.dataService = dataService;
//        }
//        public ApiDefinition GetDefinition()
//        {
//            return new ApiDefinition
//            {
//                Route = "senuvoServices/testOrders",
//                RequireAuthentication = false
//            };
//        }

//        public IApiResponse Post(ApiRequest request)
//        {
//            Orders orders = new Orders(orderService, dataService);
//            List<int> result = orders.FinalizeNonAcceptedOrder2(new FinalizeNonAcceptedOrderHookRequest { });
//            return new Ok(new { Status = 1, Message = string.Join(", ", result) });
//        }
//    }
//}
