using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;

namespace Aloha.MicroService.Payment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(PaymentService paymentService, IVNPayService vnPayService, IMomoService momoService, IConfiguration configuration, IMapper mapper, IEventPublisher eventPublisher) : ControllerBase
    {
        private readonly PaymentService _paymentService = paymentService;
        private readonly IVNPayService _vnPayService = vnPayService;
        private readonly IMomoService _momoService = momoService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IMapper _mapper = mapper;
        private readonly IEventPublisher _eventPublisher = eventPublisher;

        /// <summary>
        /// Creates a new payment record from the provided payment data.
        /// </summary>
        /// <param name="paymentDto">The payment details to create a new payment.</param>
        /// <returns>A 201 Created response with the created payment data and a route to retrieve it by ID.</returns>
        //[HttpPost]
        //public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto paymentDto)
        //{
        //    var payment = _mapper.Map<Payments>(paymentDto);
        //    payment.CreatedAt = DateTime.UtcNow;

        //    await _paymentService.CreateAsync(payment);

        //    var response = ApiResponseBuilder.BuildResponse("Payment created successfully!", payment);
        //    return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, response);
        //}
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto paymentDto)
        {
            var payment = _mapper.Map<Payments>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.PlanId = paymentDto.PlanId;
            await _paymentService.CreateAsync(payment);

            var hardcodedUserId = Guid.Parse("5000a152-a3c9-4f47-9127-58ed71c7fc06");

            // Use a new GUID for PaymentId
            var createUserPlanCommand = new CreateUserPlanCommand
            {
                PaymentId = payment.Id, 
                UserId = hardcodedUserId,
                PlanId = int.Parse(payment.PlanId),
                Amount = payment.Price,
                PaymentDate = payment.CreatedAt
            };
            await _eventPublisher.PublishAsync(createUserPlanCommand);

            var response = ApiResponseBuilder.BuildResponse("Payment created successfully!", payment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, response);
        }

        /// <summary>
        /// Retrieves a payment by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the payment.</param>
        /// <returns>
        /// Returns a 200 OK response with the payment data if found; otherwise, returns 404 Not Found if the payment does not exist.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(string id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
                return NotFound(ApiResponseBuilder.BuildResponse<string>("Payment not found!", null));

            var response = ApiResponseBuilder.BuildResponse("Payment retrieved successfully!", payment);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves all payments associated with the specified user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose payments are to be retrieved.</param>
        /// <returns>An HTTP 200 response containing a list of the user's payments.</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUserId(string userId)
        {
            var payments = await _paymentService.GetByUserIdAsync(userId);
            var response = ApiResponseBuilder.BuildResponse("Payments retrieved successfully!", payments);
            return Ok(response);
        }

        /// <summary>
        /// Generates a VNPay payment URL based on the provided payment information.
        /// </summary>
        /// <param name="model">The payment information used to create the VNPay URL.</param>
        /// <returns>
        /// A 200 OK response with the generated VNPay URL if successful; otherwise, a 400 Bad Request response if URL creation fails.
        /// </returns>
        [HttpPost("payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (url == null)
                return BadRequest(ApiResponseBuilder.BuildResponse<string>("Failed to create VNPay URL.", null));

            return Ok(ApiResponseBuilder.BuildResponse("VNPay URL created successfully!", url));
        }

        /// <summary>
        /// Handles VNPay Instant Payment Notification (IPN) requests by processing query parameters and returning the result.
        /// </summary>
        /// <returns>An HTTP 200 response with the IPN result if successful; otherwise, a 400 Bad Request if processing fails.</returns>
        [HttpGet("ipn")]
        public IActionResult InpHandle()
        {
            var result = _vnPayService.PaymentExecuteIpn(Request.Query);
            if (result == null)
                return BadRequest(ApiResponseBuilder.BuildResponse<string>("VNPay IPN failed.", null));

            return Ok(ApiResponseBuilder.BuildResponse("VNPay IPN handled successfully!", result));
        }

        /// <summary>
        /// Handles VNPay payment callback requests, processes the payment result, and redirects the user to a success or failure page with the payment amount.
        /// </summary>
        /// <returns>A redirect to the appropriate payment result page, or a bad request if an error occurs.</returns>
        [HttpGet("callback")]
        public IActionResult PaymentCallback()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);
                var amount = Request.Query["vnp_Amount"];
                var actualAmount = int.Parse(amount) / 100;
                var successUrl = configuration["FrontendRedirect:SuccessUrl"];
                var failedUrl = configuration["FrontendRedirect:FailedUrl"];
                if (Request.Query["vnp_ResponseCode"] == "00")
                {
                    return Redirect($"{successUrl}?status=success&amount={actualAmount}");
                }
                else
                {
                    return Redirect($"{failedUrl}?status=failed&amount={actualAmount}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        ///// <summary>
        ///// Creates a Momo payment URL based on the provided payment information.
        ///// </summary>
        ///// <param name="model">The payment information used to create the Momo payment URL.</param>
        ///// <returns>A 200 OK response with the generated Momo payment URL if successful; otherwise, a 400 Bad Request response.</returns>
        //[HttpPost("momo-payment-url")]
        //public async Task<IActionResult> CreateMomoPaymentUrl([FromBody] MomoClientRequestModel model)
        //{
        //    var response = await _momoService.CreatePaymentAsync(model);
        //    if (response == null || string.IsNullOrEmpty(response.PayUrl))
        //        return BadRequest(ApiResponseBuilder.BuildResponse<string>("Failed to create Momo payment URL.", null));

        //    return Ok(ApiResponseBuilder.BuildResponse("Momo payment URL created successfully!", response.PayUrl));
        //}
        ///// <summary>
        ///// Handles Momo payment callback, processes the payment result, and redirects the user to a success or failure page.
        ///// </summary>
        ///// <returns>A redirect to the appropriate payment result page, or a bad request if an error occurs.</returns>
        //[HttpGet("momo-callback")]
        //public IActionResult MomoCallback()
        //{
        //    try
        //    {
        //        var result = _momoService.PaymentExecuteAsync(Request.Query);
        //        var amount = Request.Query["amount"];
        //        var orderId = Request.Query["orderId"];
        //        var errorCode = Request.Query["errorCode"];

        //        // Momo: errorCode == "0" là thành công
        //        if (errorCode == "0")
        //        {
        //            return Redirect($"http://localhost:3000/payment/success?status=success&amount={amount}&orderId={orderId}");
        //        }
        //        else
        //        {
        //            return Redirect($"http://localhost:3000/payment/failed?status=failed&amount={amount}&orderId={orderId}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}