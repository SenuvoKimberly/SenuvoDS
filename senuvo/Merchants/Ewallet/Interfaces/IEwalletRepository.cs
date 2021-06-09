using senuvo.Merchants.Ewallet.Models;


namespace senuvo.Merchants.Ewallet.Interfaces
{
    public interface IEwalletRepository
    {
        EwalletSettings GetSettings();
        void UpdateSettings(EwalletSettingsRequest settings);
        void ResetSettings();
    }
}
