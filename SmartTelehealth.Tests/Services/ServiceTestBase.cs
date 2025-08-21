using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;
using Xunit;

namespace SmartTelehealth.Tests.Services
{
    public abstract class ServiceTestBase
    {
        protected readonly TokenModel TestToken;

        protected ServiceTestBase()
        {
            TestToken = new TokenModel
            {
                UserID = 1,
                RoleID = 1
            };
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

        protected void AssertJsonModelHasData(JsonModel result)
        {
            Assert.NotNull(result.data);
        }

        protected void AssertJsonModelHasNoData(JsonModel result)
        {
            Assert.Null(result.data);
        }
    }
}
