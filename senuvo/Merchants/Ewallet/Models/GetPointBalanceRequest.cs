
namespace senuvo.Merchants.Ewallet.Models
{
    public class GetPointBalanceRequest
    {
        public string ExternalCustomerId { get; set; }
        public string CompanyId { get; set; }
        public string PointAccountId { get; set; }
    }

    public class GetPointBalanceApiRequest
    {
        public string CustomerId { get; set; }
    }
}
