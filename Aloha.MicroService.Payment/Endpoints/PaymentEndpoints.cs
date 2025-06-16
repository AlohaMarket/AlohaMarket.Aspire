namespace Aloha.MicroService.Payment.Endpoints
{
    public static class PaymentEndpoints
    {
        public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/Payment");

            group.MapPost("/momo-payment-url", async (
                MomoClientRequestModel model,
                IMomoService momoService) =>
            {
                var response = await momoService.CreatePaymentAsync(model);
                if (response == null || string.IsNullOrEmpty(response.PayUrl))
                    return Results.BadRequest(ApiResponseBuilder.BuildResponse<string>("Failed to create Momo payment URL.", null));

                return Results.Ok(ApiResponseBuilder.BuildResponse("Momo payment URL created successfully!", response.PayUrl));
            });

            group.MapGet("/momo-callback", (
                HttpRequest request,
                IMomoService momoService,
                IConfiguration configuration) =>
            {
                try
                {
                    var result = momoService.PaymentExecuteAsync(request.Query);
                    var amount = request.Query["amount"];
                    var orderId = request.Query["orderId"];
                    var errorCode = request.Query["errorCode"];
                    var successUrl = configuration["FrontendRedirect:SuccessUrl"];
                    var failedUrl = configuration["FrontendRedirect:FailedUrl"];

                    if (errorCode == "0")
                    {
                        return Results.Redirect($"{successUrl}?status=success&amount={amount}&orderId={orderId}");
                    }
                    else
                    {
                        return Results.Redirect($"{failedUrl}?status=failed&amount={amount}&orderId={orderId}");
                    }
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            return endpoints;
        }
    }
}
