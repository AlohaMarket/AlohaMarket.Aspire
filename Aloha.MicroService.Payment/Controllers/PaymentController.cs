
using Aloha.ServiceDefaults.Meta;

namespace Aloha.MicroService.Payment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(PaymentService paymentService, IVNPayService vnPayService, IMapper mapper) : ControllerBase
    {
        private readonly PaymentService _paymentService = paymentService;
        private readonly IVNPayService _vnPayService = vnPayService;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto paymentDto)
        {
            var payment = _mapper.Map<Payments>(paymentDto);
            payment.CreatedAt = DateTime.UtcNow;

            await _paymentService.CreateAsync(payment);

            var response = ApiResponseBuilder.BuildResponse("Payment created successfully!", payment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(string id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
                return NotFound(ApiResponseBuilder.BuildResponse<string>("Payment not found!", null));

            var response = ApiResponseBuilder.BuildResponse("Payment retrieved successfully!", payment);
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUserId(string userId)
        {
            var payments = await _paymentService.GetByUserIdAsync(userId);
            var response = ApiResponseBuilder.BuildResponse("Payments retrieved successfully!", payments);
            return Ok(response);
        }

        [HttpPost("payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (url == null)
                return BadRequest(ApiResponseBuilder.BuildResponse<string>("Failed to create VNPay URL.", null));

            return Ok(ApiResponseBuilder.BuildResponse("VNPay URL created successfully!", url));
        }

        [HttpGet("ipn")]
        public IActionResult InpHandle()
        {
            var result = _vnPayService.PaymentExecuteIpn(Request.Query);
            if (result == null)
                return BadRequest(ApiResponseBuilder.BuildResponse<string>("VNPay IPN failed.", null));

            return Ok(ApiResponseBuilder.BuildResponse("VNPay IPN handled successfully!", result));
        }

        [HttpGet("callback")]
        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Redirect(response.Success
                ? "http://localhost:5173/order-success"
                : "http://localhost:5173/payment-callback");
        }
    }
}