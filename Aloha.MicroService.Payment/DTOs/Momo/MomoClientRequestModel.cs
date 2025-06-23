namespace Aloha.MicroService.Payment.DTOs.Momo
{
    public class MomoClientRequestModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public double Amount { get; set; }
    }
}
