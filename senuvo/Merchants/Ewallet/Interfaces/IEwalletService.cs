using DirectScale.Disco.Extension;
using senuvo.Merchants.Ewallet.Models;
using senuvo.Merchants.Moneyout.Models;

namespace senuvo.Merchants.Ewallet.Interfaces
{
    public interface IEwalletService
    {
        EwalletSettings GetSettings();
        void UpdateCustomer(Associate req);
        string CreateCustomer(Application app, int responseAssociateId);
        dynamic CallEwallet(string requestUrl, object requestData);
        PointBalanceResponse GetPointBalance(GetPointBalanceRequest request);
        string CreatePointTransaction(CustomerPointTransactionsRequest request);

        string CreateToken();
        PaymentResponse CreditPayment(string payorId, int orderNumber, string currencyCode, double paymentAmount, double refundAmount, string cardNumber, string transactionNumber, string authorizationCode);
        int SetActiveCommissionMerchant(SetActiveCommissionMerchantRequest request);
    }
}
