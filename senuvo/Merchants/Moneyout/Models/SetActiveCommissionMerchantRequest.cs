using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace senuvo.Merchants.Moneyout.Models
{
    public class SetActiveCommissionMerchantRequest
    {
        public int AssociateId { get; set; }
        public int MerchantId { get; set; }
    }
}
