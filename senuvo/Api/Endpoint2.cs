using DirectScale.Disco.Extension.Api;
using DirectScale.Disco.Extension.Hooks.Autoships;
using DirectScale.Disco.Extension.Services;
using Hooks;
using System.Collections.Generic;

namespace senuvo.Api
{
    //    public class Endpoint2 : IApiEndpoint
    //    {
    //        private readonly IAssociateService associateService;
    //        private readonly IDataService dataService;
    //        private readonly IOrderService orderService;

    //        public Endpoint2(IAssociateService associateService, IDataService dataService, IOrderService orderService)
    //        {
    //            //  this.ticketService = ticketService;
    //            this.orderService = orderService;
    //            this.associateService = associateService;
    //            this.dataService = dataService;
    //        }
    //        public ApiDefinition GetDefinition()
    //        {
    //            return new ApiDefinition
    //            {
    //                Route = "senuvoServices/testDaily",
    //                RequireAuthentication = false
    //            };
    //        }

    //        public IApiResponse Post(ApiRequest request)
    //        {
    //            Daily daily = new Daily(associateService, dataService, orderService);
    //            ResultDailyRunAfter result = daily.DailyRunAfter();
    //            return new Ok(new { Status = 1, DistributorsIDS = string.Join(", ", result.DistributorsIDS), OrdersIDS = string.Join(", ", result.OrdersIDS) });
    //        }
    //    }
    public class ResultDailyRunAfter
{
    public List<int> DistributorsIDS { get; set; } = new List<int>();
    public List<int> OrdersIDS { get; set; } = new List<int>();
}
}
