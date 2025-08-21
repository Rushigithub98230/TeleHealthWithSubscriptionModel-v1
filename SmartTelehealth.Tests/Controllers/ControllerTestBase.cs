using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;
using Xunit;

namespace SmartTelehealth.Tests.Controllers
{
    public abstract class ControllerTestBase
    {
        protected readonly Mock<ILogger<BaseController>> MockLogger;
        protected readonly TokenModel TestToken;

        protected ControllerTestBase()
        {
            MockLogger = new Mock<ILogger<BaseController>>();
            TestToken = new TokenModel
            {
                UserID = 1,
                RoleID = 1
            };
        }

        protected HttpContext CreateMockHttpContext()
        {
            var httpContext = new Mock<HttpContext>();
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("UserId", TestToken.UserID.ToString()),
                new System.Security.Claims.Claim("RoleId", TestToken.RoleID.ToString())
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            
            httpContext.Setup(x => x.User).Returns(principal);
            return httpContext.Object;
        }

        protected void AssertJsonModelResponse(JsonModel result, int expectedStatusCode, string expectedMessage = null)
        {
            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, result.StatusCode);
            if (expectedMessage != null)
            {
                Assert.Equal(expectedMessage, result.Message);
            }
        }

        protected void AssertJsonModelData<T>(JsonModel result, T expectedData)
        {
            Assert.NotNull(result.data);
            Assert.IsType<T>(result.data);
            Assert.Equal(expectedData, result.data);
        }
    }
}
