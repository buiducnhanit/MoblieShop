using MoblieShop.Service.MomoPayment;

namespace MoblieShop.Interface
{
    public interface IMomoPaymentService
    {
        Task<string> CreatePaymentUrl(MomoPaymentRequestModel model);
        MomoPaymentResponseModel PaymentExecute(IQueryCollection query);
    }
}
