using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using TwoFactorAuthAPI.Controllers;
using TwoFactorAuthAPI.Models;

namespace TwoFactorAuthAPI.Tests
{
    [TestClass]
    public class TwoFactorAuthControllerTests
    {


        [TestMethod]
        public void SendConfirmationCode_TooManyCodes_ReturnsBadRequest()
        {
            // Arrange
            var phoneNumberRequest = new PhoneNumberRequest { Phone = "1234567890" };
            var mockDatabase = new Mock<IDatabase>();
            mockDatabase.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(JsonSerializer.Serialize(new List<ConfirmationCode>() { new ConfirmationCode() }));
            var mockRedis = new Mock<IConnectionMultiplexer>();
            mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
            var mockTwoFactorAuthConfig = new Mock<IOptions<TwoFactorAuthConfig>>();
            mockTwoFactorAuthConfig.SetupGet(c => c.Value).Returns(new TwoFactorAuthConfig { ConcurrentCodesPerPhone = 1 });
            var controller = new TwoFactorAuthController(null, mockRedis.Object, mockTwoFactorAuthConfig.Object);

            // Act
            var result = controller.SendConfirmationCode(phoneNumberRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void CheckConfirmationCode_ValidCode_ReturnsOk()
        {
            // Arrange
            var phoneNumberRequest = new PhoneNumberRequest { Phone = "1234567890" };
            var mockDatabase = new Mock<IDatabase>();
            mockDatabase.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(JsonSerializer.Serialize(new List<ConfirmationCode>()
                {
                    new ConfirmationCode
                    {
                        Code = "123456",
                        CreationTime = DateTime.UtcNow
                    }
                }));
            var mockRedis = new Mock<IConnectionMultiplexer>();
            mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
            var mockTwoFactorAuthConfig = new Mock<IOptions<TwoFactorAuthConfig>>();
            mockTwoFactorAuthConfig.SetupGet(c => c.Value).Returns(new TwoFactorAuthConfig { CodeLifetimeMinutes = 5 });
            var controller = new TwoFactorAuthController(null, mockRedis.Object, mockTwoFactorAuthConfig.Object);

            // Act
            var result = controller.CheckConfirmationCode(phoneNumberRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }
}