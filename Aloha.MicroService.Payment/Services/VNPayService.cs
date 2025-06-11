namespace Aloha.MicroService.Payment.Services
{
    public interface VNPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
        ErrorViewModel PaymentExecuteIpn(IQueryCollection collections);
    }
    public class VNPayService : IVNPayService
    {

        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


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
