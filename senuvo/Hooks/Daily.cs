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
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Hooks
{
    public class Daily : IHook<DailyRunHookRequest, DailyRunHookResponse>, IDaily
    {
        private readonly IAssociateService associateService;
        private readonly IDataService dataService;
        private readonly IOrderService orderService;
        //private readonly ITicketService ticketService;
        public Daily(IAssociateService associateService, IDataService dataService, IOrderService orderService)//, ITicketService ticketService)
        {
            //this.ticketService = ticketService;
            this.orderService = orderService;
            this.associateService = associateService;
            this.dataService = dataService;
        }
        public DailyRunHookResponse Invoke(DailyRunHookRequest request, Func<DailyRunHookRequest, DailyRunHookResponse> func)
        {
            //ticketService.LogEvent(1, "start DailyRunAfter");
            var response = func(request);
            DailyRunAfter();
            //ticketService.LogEvent(1, "end DailyRunAfter");
            return response;
        }


        public ResultDailyRunAfter DailyRunAfter()
        {
            ResultDailyRunAfter result = new ResultDailyRunAfter();
            result.DistributorsIDS = FinalizeNonAcceptedDistributors();
            result.OrdersIDS = FinalizeNonAcceptedOrder();

            return result;
        }

        public List<int> FinalizeNonAcceptedDistributors()
        {
            var sqlconnectstring = dataService.ClientConnectionString;
            string connectionString = sqlconnectstring.ToString();
            List<int> DistributorsIDS = new List<int>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string today = DateTime.Now.ToString("yyyyMMdd");
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                string queryString = "SELECT recordnumber  FROM  CRM_Distributors  WHERE  StatusID = 6 and SignupDate < '" + today + "' and SignupDate > '" + yesterday + "'";
                SqlCommand command = new SqlCommand(queryString, connection);//command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DistributorsIDS.Add(int.Parse(reader["recordnumber"].ToString()));
                }
                reader.Close();
                connection.Close();
            }
            foreach (var distributorsId in DistributorsIDS)
            {
                // ticketService.LogEvent(distributorsId, distributorsId.ToString() + " DailyRunAfter - SetAssociateStatus Terminated");
                associateService.SetAssociateStatus(distributorsId, (int)AssociateStatus.Terminated);
            }

            return DistributorsIDS;
        }
        public List<int> FinalizeNonAcceptedOrder()
        {
            var sqlconnectstring = dataService.ClientConnectionString;
            string connectionString = sqlconnectstring.ToString();
            List<int> orderIds = new List<int>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string today = DateTime.Now.ToString("yyyyMMdd");
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                string queryString = " SELECT p.OrderNumber  FROM  ORD_ORDER o " +
                    " inner join ORD_Payments p on p.OrderNumber = o.recordnumber " +
                    " WHERE  o.OrderDate < '" + today + "' and o.OrderDate > '" + yesterday + "' " +
                    " and p.PaymentStatus = 'Pending' and p.PayType like '%Yuansfer%' ";
                SqlCommand command = new SqlCommand(queryString, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    orderIds.Add(int.Parse(reader["OrderNumber"].ToString()));
                }
                reader.Close();
                connection.Close();
            }
            //var orders = orderService.GetOrders(orderIds.ToArray());
            //foreach (var order in orders)
            //{
            //    order.Status = OrderStatus.Declined;
            //    orderService.FinalizeNonAcceptedOrder(order);
            //}
            foreach (var orderNumber in orderIds)
            {
                orderService.CancelOrder(orderNumber);
            }

            return orderIds;
        }
    }
}










