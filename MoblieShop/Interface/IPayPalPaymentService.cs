using PayPal.Api;

namespace MoblieShop.Interface
{
    public interface IPayPalPaymentService
    {
        Payment CreatePayment(decimal totalAmount, string returnUrl, string cancelUrl);
        Payment ExecutePayment(string paymentId, string payerId);
    }
}
