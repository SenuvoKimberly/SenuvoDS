using DirectScale.Disco.Extension;
using DirectScale.Disco.Extension.Services;
using Microsoft.Extensions.Logging;
using senuvo.Merchants.Ewallet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace senuvo.Hooks
{
    public class EwalletMoneyOutMerchant : AccountCommissionMerchant
    {
        private const string ACCOUNT_NUMBER_KEY = "ActNum1";
        private readonly IAssociateService _associateService;
        private readonly ILogger _logger;
        private readonly IEwalletService _ewalletService;
        private readonly IMoneyOutService _moneyOutService;
        private const int MERCHANT_ID = 9012;

        public EwalletMoneyOutMerchant(IAssociateService associateService, IEwalletService ewalletService, ILogger logger, IMoneyOutService moneyOutService)
           : base(new MerchantInfo(MERCHANT_ID, "Ewallet Money Out", "USD")
                 , new List<CommissionMerchantCustomField>
                 {
                     new CommissionMerchantCustomField
                     {
                         DisplayText = "Account Number",
                         Key = ACCOUNT_NUMBER_KEY,
                         IsRequired = true
                     }
                 })
        {
            _associateService = associateService;
            _ewalletService = ewalletService;
            _moneyOutService = moneyOutService;
            _logger = logger;
        }

        public override CommissionPaymentResults PayCommissions(int batchId, List<CommissionPayment> payments)
        {
            var results = new List<CommissionPaymentResult>();

            foreach (var payment in payments)
            {
                // Be careful here!
                // We process the whole batch at once, so you have to provide the error handling so the statuses are correctly saved.
                // If half of the payments succeeded, but then an exception were thrown, disco would not receive the CommissionPaymentResults
                // and would be unaware that any of the payments went out. Try - catch handling is necessary
                try
                {
                    payment.MerchantCustomFields.TryGetValue(ACCOUNT_NUMBER_KEY, out string accountNumber);

                    if (accountNumber == null)
                    {
                        // Provision associate's eWallet account
                        var res =_moneyOutService.AutoProvisionAccount(payment.AssociateId, MERCHANT_ID);

                        if (res != null && res.CustomValues != null && !res.CustomValues.ContainsKey(ACCOUNT_NUMBER_KEY))
                        {
                            results.Add(new CommissionPaymentResult()
                            {
                                PaymentUniqueId = payment.PaymentUniqueId, // This one is important that you manually set this equal to the payment you just processed
                                Status = CommissionPaymentStatus.Failed,
                                ErrorMessage = "Account not initialized"
                            });

                            continue;
                        }
                        else
                        {
                            accountNumber = res.CustomValues[ACCOUNT_NUMBER_KEY];
                        }
                    }

                    _ewalletService.CreditPayment(accountNumber, payment.Id, "USD", Convert.ToDouble(payment.Amount), Convert.ToDouble(payment.Amount), "", payment.PaymentUniqueId, "");
                    // call out to merchant to process payment
                    results.Add(new CommissionPaymentResult()
                    {
                        PaymentUniqueId = payment.PaymentUniqueId, // This one is important that you manually set this equal to the payment you just processed
                        TransactionNumber = payment.PaymentUniqueId,
                        Status = CommissionPaymentStatus.Paid,
                        DatePaid = DateTime.Now,
                        CheckNumber = 100,
                    });
                    
                }
                catch (Exception e)
                {
                    results.Add(new CommissionPaymentResult()
                    {
                        PaymentUniqueId = payment.PaymentUniqueId, // This one is important that you manually set this equal to the payment you just processed
                        Status = CommissionPaymentStatus.Failed,
                        ErrorMessage = $"Exception occurred while paying {payment.AssociateId} : {e.Message}"
                    });
                }
            }
            return new CommissionPaymentResults
            {
                Results = results
            };
        }

        public override CommissionMerchantAccountFields ProvisionAccount(int associateId)
        {
            var associateInfo = _associateService.GetAssociate(associateId);
            var app = new Application 
            { 
                FirstName = associateInfo.DisplayFirstName,
                LastName = associateInfo.DisplayLastName,
                ExternalId = associateInfo.AssociateId.ToString(),
                BackOfficeId = associateInfo.BackOfficeId
            };
            var id = _ewalletService.CreateCustomer(app, associateId);

            // Create an account for this associate, and any other custom values you need in PayCommissions
            return new CommissionMerchantAccountFields
            {
                CustomValues = new Dictionary<string, string>
               {
                   { ACCOUNT_NUMBER_KEY, id }
               }
            };
        }
    }
}
