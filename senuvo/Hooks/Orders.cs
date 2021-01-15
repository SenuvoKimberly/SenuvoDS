//using DirectScale.Disco.Extension;
//using DirectScale.Disco.Extension.Api;
//using DirectScale.Disco.Extension.Services;
//using Microsoft.Extensions.DependencyInjection;
//using senuvo.Api;
//using System;
//using System.Collections.Generic;
//using Yuansfer.Helper;
//using DirectScale.Disco.Extension.Hooks.Commissions;
//using DirectScale.Disco.Extension.Hooks;
//using DirectScale.Disco.Extension.Hooks.Autoships;
//using DirectScale.Disco.Extension.Hooks.Orders;
//using System.Threading.Tasks;
//using System.Data.SqlClient;

//namespace Hooks
//{
//    public class Orders : IHook<FinalizeNonAcceptedOrderHookRequest, FinalizeNonAcceptedOrderHookResponse>
//    {
//        private readonly IOrderService orderService;
//        private readonly IDataService dataService;
//        //private readonly ITicketService ticketService;

//        public Orders(IOrderService orderService, IDataService dataService)//, ITicketService ticketService)
//        {
//            //  this.ticketService = ticketService;
//            this.orderService = orderService;
//            this.dataService = dataService;
//        }

//        public FinalizeNonAcceptedOrderHookResponse Invoke(FinalizeNonAcceptedOrderHookRequest request, Func<FinalizeNonAcceptedOrderHookRequest, FinalizeNonAcceptedOrderHookResponse> func)
//        {
//            var response = func(request);
//            FinalizeNonAcceptedOrder2(request);
//            return response;
//        }

//        public List<int> FinalizeNonAcceptedOrder2(FinalizeNonAcceptedOrderHookRequest request)
//        {
//            var sqlconnectstring = dataService.ClientConnectionString;
//            string connectionString = sqlconnectstring.ToString();
//            List<int> orderIds = new List<int>();

//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                connection.Open();
//                string today = DateTime.Now.ToString("yyyyMMdd");
//                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
//                string queryString = " SELECT p.OrderNumber  FROM  ORD_ORDER o " +
//                    " inner join ORD_Payments p on p.OrderNumber = o.recordnumber " +
//                    " WHERE  o.OrderDate < '" + today + "' and o.OrderDate > '" + yesterday + "' " +
//                    " and p.PaymentStatus = 'Pending' and p.PayType like '%Yuansfer%' ";
//                SqlCommand command = new SqlCommand(queryString, connection);
//                SqlDataReader reader = command.ExecuteReader();
//                while (reader.Read())
//                {
//                    orderIds.Add(int.Parse(reader["OrderNumber"].ToString()));
//                }
//                reader.Close();
//                connection.Close();
//            }
//            var orders = orderService.GetOrders(orderIds.ToArray());
//            foreach (var order in orders)
//            {
//                orderService.FinalizeNonAcceptedOrder(order);
//            }

//            return orderIds;
//        }
//    }
//}










