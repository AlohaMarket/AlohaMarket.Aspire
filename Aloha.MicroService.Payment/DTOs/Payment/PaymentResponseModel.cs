namespace Aloha.MicroService.Payment.DTOs.Payment
{
    public class PaymentResponseModel
    {
        public int QuotationId { get; set; }
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
    }
}
