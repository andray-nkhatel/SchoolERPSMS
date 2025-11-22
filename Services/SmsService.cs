using System.Net.Http;
using System.Text;
using System.Net;

namespace BluebirdCore.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message);
    }

    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmsService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _senderId;

        public SmsService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            
            _baseUrl = _configuration["Sms:BaseUrl"] ?? "https://bulksms.zamtel.co.zm/api/v2.1/action/";
            _apiKey = _configuration["Sms:ApiKey"] ?? throw new InvalidOperationException("SMS API key is not configured");
            _senderId = _configuration["Sms:SenderId"] ?? "Zamtel";
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("SMS send failed: Phone number is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("SMS send failed: Message is empty");
                return false;
            }

            try
            {
                // Format phone number (ensure it starts with country code)
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                
                // URL encode the message
                var encodedMessage = WebUtility.UrlEncode(message);
                
                // Build the URL according to Zamtel API format
                // Format: send/api_key/:api_key/contacts/:contacts/senderId/:sender_id/message/:message
                // Note: Phone number should be wrapped in brackets according to API documentation
                var url = $"{_baseUrl}send/api_key/{_apiKey}/contacts/[{formattedPhone}]/senderId/{_senderId}/message/{encodedMessage}";

                _logger.LogInformation($"Sending SMS to {formattedPhone} via Zamtel API");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"SMS sent successfully to {formattedPhone}. Response: {responseContent}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"SMS send failed to {formattedPhone}. Status: {response.StatusCode}, Response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS to {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message)
        {
            if (phoneNumbers == null || !phoneNumbers.Any())
            {
                _logger.LogWarning("Bulk SMS send failed: No phone numbers provided");
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Bulk SMS send failed: Message is empty");
                return false;
            }

            var results = new List<bool>();
            
            // Send SMS to each phone number
            foreach (var phoneNumber in phoneNumbers)
            {
                var result = await SendSmsAsync(phoneNumber, message);
                results.Add(result);
                
                // Add a small delay between requests to avoid rate limiting
                await Task.Delay(100);
            }

            var successCount = results.Count(r => r);
            _logger.LogInformation($"Bulk SMS completed: {successCount}/{phoneNumbers.Count} messages sent successfully");
            
            return successCount == phoneNumbers.Count;
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove any whitespace or special characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // If it doesn't start with country code, add it (Zambia is 260)
            if (!cleaned.StartsWith("260"))
            {
                // If it starts with 0, replace with 260
                if (cleaned.StartsWith("0"))
                {
                    cleaned = "260" + cleaned.Substring(1);
                }
                else
                {
                    // Assume it's a local number and add country code
                    cleaned = "260" + cleaned;
                }
            }
            
            return cleaned;
        }
    }
}

