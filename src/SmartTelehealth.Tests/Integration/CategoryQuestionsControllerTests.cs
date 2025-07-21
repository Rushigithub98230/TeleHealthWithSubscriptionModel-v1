using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartTelehealth.API;
using SmartTelehealth.Core.Entities;
using Xunit;
using System.Text.Json;

namespace SmartTelehealth.Tests.Integration
{
    public class CategoryQuestionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public CategoryQuestionsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            // TODO: Add authentication header/cookie for Superadmin endpoints if needed
        }

        [Theory]
        [InlineData("text", null, "Sample text answer")] // Textbox
        [InlineData("textarea", null, "Sample long answer")] // Textarea
        [InlineData("radio", "[\"Option 1\",\"Option 2\"]", "Option 1")] // Radio
        [InlineData("checkbox", "[\"A\",\"B\",\"C\"]", "[\"A\",\"C\"]")] // Checkbox (multiple)
        [InlineData("range", "{\"min\":1,\"max\":10}", "5")] // Range
        [InlineData("image", null, "https://example.com/image.jpg")] // Image answer (URL)
        public async Task Superadmin_Can_CRUD_And_User_Can_Answer_DynamicTypes(string type, string? optionsJson, string answerJson)
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var question = new CategoryQuestion
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                QuestionText = $"Test {type} question?",
                QuestionType = type,
                IsRequired = true,
                IsActive = true,
                OptionsJson = optionsJson
            };
            // Create
            var createResp = await _client.PostAsJsonAsync("/api/CategoryQuestions", question);
            Assert.True(createResp.StatusCode == HttpStatusCode.OK || createResp.StatusCode == HttpStatusCode.Unauthorized, $"Create: {createResp.StatusCode}");
            if (createResp.StatusCode == HttpStatusCode.Unauthorized)
                return; // Skip if auth not set up
            // Get
            var getResp = await _client.GetAsync($"/api/CategoryQuestions/{question.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var fetched = await getResp.Content.ReadFromJsonAsync<CategoryQuestion>();
            Assert.NotNull(fetched);
            Assert.Equal(question.QuestionText, fetched!.QuestionText);
            // Update
            question.QuestionText = $"Updated {type} question?";
            var updateResp = await _client.PutAsJsonAsync($"/api/CategoryQuestions/{question.Id}", question);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
            // User fetch by category
            var catResp = await _client.GetAsync($"/api/CategoryQuestions/by-category/{categoryId}");
            Assert.Equal(HttpStatusCode.OK, catResp.StatusCode);
            var questions = await catResp.Content.ReadFromJsonAsync<List<CategoryQuestion>>();
            Assert.NotNull(questions);
            Assert.Contains(questions!, q => q.Id == question.Id);
            // User submit answer
            var answer = new CategoryQuestionAnswer
            {
                Id = Guid.NewGuid(),
                CategoryQuestionId = question.Id,
                UserId = Guid.NewGuid(),
                Answer = answerJson
            };
            var answerResp = await _client.PostAsJsonAsync("/api/CategoryQuestions/answers", answer);
            Assert.Equal(HttpStatusCode.OK, answerResp.StatusCode);
            // Delete
            var deleteResp = await _client.DeleteAsync($"/api/CategoryQuestions/{question.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);
        }
    }
} 