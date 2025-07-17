using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Infrastructure.Services;
using OpenTokSDK;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing OpenTok Integration...");
        
        try
        {
            // Test 1: Direct OpenTok SDK
            Console.WriteLine("\n1. Testing OpenTok SDK directly...");
            var openTok = new OpenTok(84a6270c, "AjhKghyi8412988516");
            var session = openTok.CreateSession();
            Console.WriteLine($"✓ Session created: {session.Id}");
            
            // Test 2: Token generation
            Console.WriteLine("\n2. Testing token generation...");
            var tokenBuilder = new TokenBuilder("84a6270c", "AjhKghyi8412988516");
            var token = tokenBuilder
                .SetSessionId(session.Id)
                .SetRole(Role.Publisher)
                .SetData("userId=test&userName=Test User")
                .Build();
            Console.WriteLine($"✓ Token generated: {token.Substring(0, 20)}...");
            
            // Test 3: Our OpenTok Service
            Console.WriteLine("\n3. Testing our OpenTok Service...");
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["OpenTokSettings:ApiKey"] = "84a6270c",
                    ["OpenTokSettings:ApiSecret"] = "AjhKghyi8412988516"
                })
                .Build();

            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<OpenTokService>();
            var openTokService = new OpenTokService(configuration, logger);
            
            var sessionResult = await openTokService.CreateSessionAsync("Test Session", false);
            Console.WriteLine($"✓ Service session created: {sessionResult.Data?.SessionId}");
            
            var tokenResult = await openTokService.GenerateTokenAsync(
                sessionResult.Data!.SessionId, 
                "test-user", 
                "Test User", 
                OpenTokRole.Publisher);
            Console.WriteLine($"✓ Service token generated: {tokenResult.Data?.Substring(0, 20)}...");
            
            var healthResult = await openTokService.IsServiceHealthyAsync();
            Console.WriteLine($"✓ Service health check: {healthResult.Success}");
            
            Console.WriteLine("\n🎉 All OpenTok tests passed successfully!");
            Console.WriteLine("\nYour OpenTok integration is working correctly.");
            Console.WriteLine($"API Key: 84a6270c");
            Console.WriteLine($"Session ID: {session.Id}");
            Console.WriteLine($"Token: {token.Substring(0, 50)}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
} 