using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace senuvo.Merchants.Moneyout.Models
{
    public class CurrencyRate
    {
        public string CurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Date { get; set; }
    }
}
