namespace Aloha.MicroService.Payment.Services
{
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        /// <summary>
        /// Handles VNPay Instant Payment Notification (IPN) requests by validating the signature and returning a response indicating the confirmation status.
        /// </summary>
        /// <param name="collection">The query collection containing VNPay IPN parameters.</param>
        /// <param name="hashSecret">The secret key used to validate the VNPay signature.</param>
        /// <returns>An <see cref="ErrorViewModel"/> indicating the result of the IPN confirmation, with response codes for success, already confirmed, or invalid input.</returns>
        public ErrorViewModel IpnHandler(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VNPayLibrary();

            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
            var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash =
                collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            long vnp_Amount = Convert.ToInt64(vnPay.GetResponseData("vnp_Amount"));
            string vnp_TransactionStatus = vnPay.GetResponseData("vnp_TransactionStatus");
            var checkSignature =
                vnPay.ValidateSignature(vnpSecureHash, hashSecret);

            if (checkSignature)
            {

                PaymentResponseModel result = new PaymentResponseModel()
                {
                    Success = true,
                    OrderDescription = orderInfo,
                    OrderId = orderId.ToString(),
                    PaymentId = vnPayTranId.ToString(),
                    TransactionId = vnPayTranId.ToString(),
                    Token = vnpSecureHash,
                    VnPayResponseCode = vnpResponseCode
                };
                if (result != null)
                {
                    Console.WriteLine("Thanh Cong");
                    return new ErrorViewModel()
                    {
                        RspCode = "0",
                        Message = "Confirm Success"
                    };
                }
                else
                {

                    Console.WriteLine("That bai");
                    return new ErrorViewModel()
                    {
                        RspCode = "02",
                        Message = "Order already confirmed"
                    };
                }

            }

            Console.WriteLine("That bai");
            return new ErrorViewModel()
            {
                RspCode = "99",
                Message = "Input data required"
            };
        }

        /// <summary>
        /// Extracts VNPay payment response data from the provided query collection, validates the signature, and returns a populated <see cref="PaymentResponseModel"/>.
        /// </summary>
        /// <param name="collection">The query collection containing VNPay response parameters.</param>
        /// <param name="hashSecret">The secret key used to validate the VNPay signature.</param>
        /// <returns>
        /// A <see cref="PaymentResponseModel"/> with payment details if the signature is valid; otherwise, a model with <c>Success = false</c>.
        /// </returns>
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VNPayLibrary();

            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
            var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash =
                collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            var checkSignature =
                vnPay.ValidateSignature(vnpSecureHash, hashSecret);

            if (!checkSignature)
                return new PaymentResponseModel()
                {
                    Success = false
                };

            return new PaymentResponseModel()
            {
                Success = true,
                OrderDescription = orderInfo,
                OrderId = orderId.ToString(),
                PaymentId = vnPayTranId.ToString(),
                TransactionId = vnPayTranId.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }
        /// <summary>
        /// Retrieves the client's IP address from the provided HTTP context, resolving IPv6 to IPv4 if possible.
        /// </summary>
        /// <param name="context">The HTTP context containing connection information.</param>
        /// <returns>The client's IP address as a string, or "127.0.0.1" if unavailable. Returns the exception message if an error occurs.</returns>
        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }
        /// <summary>
        /// Adds a key-value pair to the request data if the value is not null or empty.
        /// </summary>
        /// <param name="key">The key to add to the request data.</param>
        /// <param name="value">The value associated with the key.</param>
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        /// <summary>
        /// Adds a key-value pair to the response data collection if the value is not null or empty.
        /// </summary>
        /// <param name="key">The key to add to the response data.</param>
        /// <param name="value">The value associated with the key.</param>
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        /// <summary>
        /// Retrieves the value associated with the specified key from the response data.
        /// </summary>
        /// <param name="key">The key to look up in the response data.</param>
        /// <returns>The value for the specified key, or an empty string if the key is not found.</returns>
        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        /// <summary>
        /// Constructs a VNPay request URL with encoded parameters and an appended HMAC-SHA512 signature.
        /// </summary>
        /// <param name="baseUrl">The base URL to which the query parameters will be appended.</param>
        /// <param name="vnpHashSecret">The secret key used to generate the secure hash signature.</param>
        /// <returns>The full request URL containing all parameters and the computed secure hash.</returns>
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        /// <summary>
        /// Validates the provided signature by comparing it to an HMAC-SHA512 hash of the response data using the given secret key.
        /// </summary>
        /// <param name="inputHash">The signature to validate against.</param>
        /// <param name="secretKey">The secret key used for hashing.</param>
        /// <returns>True if the signature is valid; otherwise, false.</returns>
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Computes an HMAC SHA-512 hash of the input data using the specified key and returns the result as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="key">The secret key used for hashing.</param>
        /// <param name="inputData">The data to be hashed.</param>
        /// <returns>The HMAC SHA-512 hash as a lowercase hexadecimal string.</returns>
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        /// <summary>
        /// Serializes the response data into a URL-encoded query string, excluding signature-related fields.
        /// </summary>
        /// <returns>A URL-encoded string of response parameters, excluding "vnp_SecureHashType" and "vnp_SecureHash".</returns>
        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
    }
}

public class VnPayCompare : IComparer<string>
{
    /// <summary>
    /// Compares two strings using ordinal comparison with the "en-US" culture.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// 0 if the strings are equal; -1 if <paramref name="x"/> is less than <paramref name="y"/>; 1 if <paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

