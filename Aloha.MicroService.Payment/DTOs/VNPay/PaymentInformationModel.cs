namespace Aloha.MicroService.Payment.DTOs.Payment
{
    public class PaymentInformationModel
    {
        public int QuotationId { get; set; }
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
    }
}
