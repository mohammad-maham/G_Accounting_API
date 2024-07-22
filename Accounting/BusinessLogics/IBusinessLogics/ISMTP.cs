using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface ISMTP
    {
        Task SendEmailAsync(SMTPModel smtp);
        Task SendEmailViaGoogleApiAsync(SMTPModel smtp);
        Task SendAsanakSMSAsync(SMSModel sms);
        Task SendGoldOTPSMSAsync(SMSModel sms);
    }
}
