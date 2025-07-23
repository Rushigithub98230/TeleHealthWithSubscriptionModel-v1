using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using SmartTelehealth.Application.DTOs;
using Xunit;

namespace SmartTelehealth.Tests.Integration
{
    public class QuestionnaireControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public QuestionnaireControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Admin_Can_Create_And_Get_QuestionnaireTemplate()
        {
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Test Template",
                Description = "Test Desc",
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your age?",
                        Type = "text",
                        IsRequired = true,
                        Order = 1,
                        Options = new List<CreateQuestionOptionDto>()
                    },
                    new CreateQuestionDto
                    {
                        Text = "Select symptoms:",
                        Type = "checkbox",
                        IsRequired = false,
                        Order = 2,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Fever", Value = "fever", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Cough", Value = "cough", Order = 2 }
                        }
                    }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            result.Should().NotBeNull();
            var templateId = result["id"];

            // Get template
            var getResp = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var template = await getResp.Content.ReadFromJsonAsync<QuestionnaireTemplateDto>();
            template.Should().NotBeNull();
            template!.Questions.Should().HaveCount(2);
        }

        [Fact]
        public async Task User_Can_Submit_And_Get_Response()
        {
            // Create template first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "User Response Template",
                Description = "Test Desc",
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "How do you feel?",
                        Type = "text",
                        IsRequired = true,
                        Order = 1,
                        Options = new List<CreateQuestionOptionDto>()
                    },
                    new CreateQuestionDto
                    {
                        Text = "Select symptoms:",
                        Type = "checkbox",
                        IsRequired = false,
                        Order = 2,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Fever", Value = "fever", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Cough", Value = "cough", Order = 2 }
                        }
                    }
                }
            };
            var createResp = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var result = await createResp.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var templateId = result["id"];

            // Submit user response
            var userId = Guid.NewGuid();
            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = Guid.Empty, // Will be set after fetching template
                        AnswerText = "Good",
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = Guid.Empty, // Will be set after fetching template
                        AnswerText = null,
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            // Fetch template to get question IDs
            var getResp = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var template = await getResp.Content.ReadFromJsonAsync<QuestionnaireTemplateDto>();
            responseDto.Answers[0].QuestionId = template!.Questions[0].Id;
            responseDto.Answers[1].QuestionId = template.Questions[1].Id;
            // Select the first option for the second question
            responseDto.Answers[1].SelectedOptionIds.Add(template.Questions[1].Options[0].Id);

            var submitResp = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            submitResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var submitResult = await submitResp.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var userResponseId = submitResult["id"];

            // Get user response
            var getUserResp = await _client.GetAsync($"/api/questionnaire/responses/{userId}/{templateId}");
            getUserResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var userResponse = await getUserResp.Content.ReadFromJsonAsync<UserResponseDto>();
            userResponse.Should().NotBeNull();
            userResponse!.Answers.Should().HaveCount(2);
            userResponse.Answers[0].AnswerText.Should().Be("Good");
            userResponse.Answers[1].SelectedOptionIds.Should().Contain(template.Questions[1].Options[0].Id);
        }
    }
} 