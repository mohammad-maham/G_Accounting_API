using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface ISMTP
    {
        void SendEmail(SMTPModel smtp);
        void SendAsanakSMS(SMSModel sms);
        void SendGoldOTPSMS(SMSModel sms);
    }
}
