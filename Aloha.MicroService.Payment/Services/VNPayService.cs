namespace Aloha.MicroService.Payment.Services
{
    public interface IVNPayService
    {
        /// <summary>
/// Generates a VNPay payment URL based on the provided payment information and HTTP context.
/// </summary>
/// <param name="model">The payment information used to construct the VNPay request.</param>
/// <param name="context">The HTTP context containing client request details.</param>
/// <returns>A signed VNPay payment URL for redirecting the user to complete the transaction.</returns>
string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        /// <summary>
/// Processes a VNPay payment response from the provided query parameters.
/// </summary>
/// <param name="collections">The query parameters received from VNPay after payment completion.</param>
/// <returns>A model containing the parsed and validated payment response data.</returns>
PaymentResponseModel PaymentExecute(IQueryCollection collections);
        /// <summary>
/// Processes VNPay Instant Payment Notification (IPN) data from the provided query collection and returns the result.
/// </summary>
/// <param name="collections">The query parameters received from the VNPay IPN callback.</param>
/// <returns>An <see cref="ErrorViewModel"/> representing the outcome of the IPN processing.</returns>
ErrorViewModel PaymentExecuteIpn(IQueryCollection collections);
    }
    public class VNPayService : IVNPayService
    {

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the VNPayService class with the specified configuration settings.
        /// </summary>
        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Generates a VNPay payment URL based on the provided payment information and HTTP context.
        /// </summary>
        /// <param name="model">The payment information used to construct the VNPay request.</param>
        /// <param name="context">The HTTP context containing client request details.</param>
        /// <returns>A signed VNPay payment URL for redirecting the user to complete payment.</returns>
        /// <remarks>
        /// Throws a generic exception with a user-friendly message if payment URL generation fails.
        /// </remarks>
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VNPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);
            try
            {
                var paymentUrl = pay
                    .CreateRequestUrl(
                    _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

                return paymentUrl;
            }
            catch
            {
                throw new Exception("Đã xảy ra lối trong qua trình thanh toán. Vui lòng thanh toán lại sau!");
            }
        }

        /// <summary>
        /// Processes the VNPay payment response from the provided query parameters and returns the payment result.
        /// </summary>
        /// <param name="collections">The query parameters received from VNPay after payment completion.</param>
        /// <returns>A <see cref="PaymentResponseModel"/> containing the details of the payment response.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during payment processing.</exception>
        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            try
            {
                var pay = new VNPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                return response;
            }
            catch
            {
                throw new Exception("Đã xảy ra lối trong qua trình thanh toán. Vui lòng thanh toán lại sau!");
            }
        }

        /// <summary>
        /// Processes VNPay Instant Payment Notification (IPN) data from the provided query collection and returns the result.
        /// </summary>
        /// <param name="collections">The query parameters received from the VNPay IPN callback.</param>
        /// <returns>An <see cref="ErrorViewModel"/> representing the outcome of the IPN processing.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during IPN processing.</exception>
        public ErrorViewModel PaymentExecuteIpn(IQueryCollection collections)
        {
            try
            {
                var pay = new VNPayLibrary();
                var response = pay.IpnHandler(collections, _configuration["Vnpay:HashSecret"]);

                return response;
            }
            catch
            {
                throw new Exception("Đã xảy ra lối trong qua trình thanh toán. Vui lòng thanh toán lại sau!");
            }
        }
    }
}
