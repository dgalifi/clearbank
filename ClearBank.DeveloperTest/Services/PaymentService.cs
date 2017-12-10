using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;

        public PaymentService(IAccountDataStore accountDataStore)
        {
            _accountDataStore = accountDataStore;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            Account account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            var result = new MakePaymentResult { Success = false };

            if (account == null)
                return result;

            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        result.Success = true;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments) &&
                        account.Balance >= request.Amount)
                    {
                        result.Success = true;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps) &&
                        account.Status == AccountStatus.Live)
                    {
                        result.Success = true;
                    }
                    break;
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;
                _accountDataStore.UpdateAccount(account);
            }

            return result;
        }
    }
}
