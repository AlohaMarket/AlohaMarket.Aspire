using Aloha.MicroService.Payment.DTOs.Momo;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

namespace Aloha.MicroService.Payment.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoClientRequestModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }

    public class MomoService : IMomoService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public MomoService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoClientRequestModel model)
        {
            model.OrderId = DateTime.UtcNow.Ticks.ToString();
            model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;

            var partnerCode = _configuration["MomoAPI:PartnerCode"];
            var accessKey = _configuration["MomoAPI:AccessKey"];
            var secretKey = _configuration["MomoAPI:SecretKey"];
            var momoApiUrl = _configuration["MomoAPI:MomoApiUrl"];
            var returnUrl = _configuration["MomoAPI:ReturnUrl"];
            var notifyUrl = _configuration["MomoAPI:NotifyUrl"];
            var requestType = _configuration["MomoAPI:RequestType"];

            var rawData =
                $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={model.OrderId}&amount={model.Amount}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData=";

            var signature = ComputeHmacSha256(rawData, secretKey);

            var requestData = new
            {
                accessKey = accessKey,
                partnerCode = partnerCode,
                requestType = requestType,
                notifyUrl = notifyUrl,
                returnUrl = returnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.OrderInfo,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            var httpClient = _httpClientFactory.CreateClient();
            var requestJson = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(momoApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(responseContent);
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;
            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }
    }
}