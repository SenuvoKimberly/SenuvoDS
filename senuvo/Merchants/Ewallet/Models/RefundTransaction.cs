using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace senuvo.Merchants.Ewallet.Models
{
    public class RefundTransaction
    {
        public string Id { get; set; }

        public RefundData RefundData { get; set; }
    }

    public class RefundData
    {
        public string Currency { get; set; }

        public double Amount { get; set; }

        public double PartialAmount { get; set; }

    }
}
