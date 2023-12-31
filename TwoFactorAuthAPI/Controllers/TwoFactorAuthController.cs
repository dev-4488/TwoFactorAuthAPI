﻿using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TwoFactorAuthAPI.Models;

namespace TwoFactorAuthAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TwoFactorAuthController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ITwoFactorAuthConfig _twoFactorAuthConfig;

        public TwoFactorAuthController(IConfiguration configuration, IConnectionMultiplexer redis, IOptions<TwoFactorAuthConfig> twoFactorAuthConfig)
        {
            _redis = redis;
            _twoFactorAuthConfig = twoFactorAuthConfig.Value;
        }

        [Route("api/2fa/send-code")]
        [Authorize]
        [HttpPost]
        public IActionResult SendConfirmationCode([FromBody] PhoneNumberRequest request)
        {
            var db = _redis.GetDatabase();
            var phoneCodesJson = db.StringGet(request.Phone);

            // Check if the number of codes for the phone number exceeds the limit
            if (!phoneCodesJson.IsNullOrEmpty)
            {
                var phoneCodes = JsonSerializer.Deserialize<List<ConfirmationCode>>(phoneCodesJson);
                if (phoneCodes.Count >= _twoFactorAuthConfig.ConcurrentCodesPerPhone)
                {
                    return BadRequest("Too many active codes for this phone number.");
                }
            }

            // Generate a confirmation code
            string code = GenerateConfirmationCode();
            LogCode(code);

            var newCode = new ConfirmationCode
            {
                Code = code,
                CreationTime = DateTime.UtcNow
            };

            // Save the code to Redis
            var codes = new List<ConfirmationCode>();
            if (!phoneCodesJson.IsNullOrEmpty)
            {
                codes = JsonSerializer.Deserialize<List<ConfirmationCode>>(phoneCodesJson);
            }

            codes.Add(newCode);
            db.StringSet(request.Phone, JsonSerializer.Serialize(codes));

            var response = new ConfirmationResponse { Sent = true };
            return Ok(response);
        }

        [Route("api/2fa/check-code")]
        [HttpPost]
        public IActionResult CheckConfirmationCode([FromBody] PhoneNumberRequest request)
        {
            var db = _redis.GetDatabase();
            var phoneCodesJson = db.StringGet(request.Phone);

            if (!phoneCodesJson.IsNullOrEmpty)
            {
                var phoneCodes = JsonSerializer.Deserialize<List<ConfirmationCode>>(phoneCodesJson);



                var activeCodeExists = phoneCodes.Any(c =>
                    DateTime.UtcNow.Subtract(c.CreationTime).TotalMinutes <=
                    _twoFactorAuthConfig.CodeLifetimeMinutes);

                if (activeCodeExists)
                {
                    return Ok(new ConfirmationResponse { Sent = true });
                }
            }

            return BadRequest("Invalid or expired code.");
        }


        private string GenerateConfirmationCode()
        {
            int codeLength = 6;
            // Generate a random code of specified length
            byte[] randomBytes = new byte[codeLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert bytes to a numeric string
            int value = BitConverter.ToInt32(randomBytes, 0);
            value = Math.Abs(value); 
            string code = value.ToString().Substring(0, codeLength);

            return code.PadLeft(codeLength, '0');
        }

        private void LogCode(string code)
        {
            Console.WriteLine($"Confirmation code: {code}");
        }
    }
}
