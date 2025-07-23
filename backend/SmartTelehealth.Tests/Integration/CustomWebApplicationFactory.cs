using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace SmartTelehealth.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<SmartTelehealth.API.Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Optionally customize for test DB, etc.
            return base.CreateHost(builder);
        }
    }
} 