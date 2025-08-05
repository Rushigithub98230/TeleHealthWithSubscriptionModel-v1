using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Newtonsoft.Json;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Core.Entities;
using Xunit;
using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.QuestionnaireTests
{
    public class QuestionnaireWebApplicationFactory : WebApplicationFactory<SmartTelehealth.API.Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove unrelated service registrations to isolate questionnaire system
                services.RemoveAll<IAppointmentService>();
                services.RemoveAll<IProviderService>();
                services.RemoveAll<IJwtService>();
                services.RemoveAll<IAnalyticsService>();
                services.RemoveAll<IVideoCallService>();
                services.RemoveAll<ISubscriptionService>();
                services.RemoveAll<IBillingService>();
                services.RemoveAll<IAuditService>();
                services.RemoveAll<IHomeMedService>();
                services.RemoveAll<IHealthAssessmentService>();
                services.RemoveAll<IPaymentSecurityService>();
                services.RemoveAll<IOpenTokService>();
                services.RemoveAll<IMessagingService>();
                services.RemoveAll<IChatStorageService>();
                services.RemoveAll<IUserService>();
                services.RemoveAll<INotificationService>();
                services.RemoveAll<IConsultationService>();
                services.RemoveAll<IMedicationDeliveryService>();
                services.RemoveAll<IPrivilegeRepository>();
                services.RemoveAll<IStripeService>();
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<ICategoryRepository>();
                services.RemoveAll<ISubscriptionRepository>();
                services.RemoveAll<ISubscriptionPlanRepository>();
                services.RemoveAll<ISubscriptionPlanPrivilegeRepository>();
                services.RemoveAll<IUserSubscriptionPrivilegeUsageRepository>();
                services.RemoveAll<IBillingAdjustmentRepository>();
                services.RemoveAll<ISubscriptionStatusHistoryRepository>();
                services.RemoveAll<INotificationRepository>();
                services.RemoveAll<IConsultationRepository>();
                services.RemoveAll<IMedicationDeliveryRepository>();
                services.RemoveAll<IVideoCallRepository>();

                // Replace SQL Server with In-Memory Database for testing
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("QuestionnaireTestDb");
                });

                // Add mocks for unrelated services
                services.TryAddScoped(_ => new Mock<IAppointmentService>().Object);
                services.TryAddScoped(_ => new Mock<IProviderService>().Object);
                services.TryAddScoped(_ => new Mock<IJwtService>().Object);
                services.TryAddScoped(_ => new Mock<IAnalyticsService>().Object);
                services.TryAddScoped(_ => new Mock<IVideoCallService>().Object);
                services.TryAddScoped(_ => new Mock<ISubscriptionService>().Object);
                services.TryAddScoped(_ => new Mock<IBillingService>().Object);
                services.TryAddScoped(_ => new Mock<IAuditService>().Object);
                services.TryAddScoped(_ => new Mock<IHomeMedService>().Object);
                services.TryAddScoped(_ => new Mock<IHealthAssessmentService>().Object);
                services.TryAddScoped(_ => new Mock<IPaymentSecurityService>().Object);
                services.TryAddScoped(_ => new Mock<IOpenTokService>().Object);
                services.TryAddScoped(_ => new Mock<IMessagingService>().Object);
                services.TryAddScoped(_ => new Mock<IChatStorageService>().Object);
                services.TryAddScoped(_ => new Mock<IUserService>().Object);
                services.TryAddScoped(_ => new Mock<INotificationService>().Object);
                services.TryAddScoped(_ => new Mock<IConsultationService>().Object);
                services.TryAddScoped(_ => new Mock<IMedicationDeliveryService>().Object);
                services.TryAddScoped(_ => new Mock<IPrivilegeRepository>().Object);
                services.TryAddScoped(_ => new Mock<IStripeService>().Object);
                services.TryAddScoped(_ => new Mock<IUserRepository>().Object);
                services.TryAddScoped(_ => new Mock<ICategoryRepository>().Object);
                services.TryAddScoped(_ => new Mock<ISubscriptionRepository>().Object);
                services.TryAddScoped(_ => new Mock<ISubscriptionPlanRepository>().Object);
                services.TryAddScoped(_ => new Mock<ISubscriptionPlanPrivilegeRepository>().Object);
                services.TryAddScoped(_ => new Mock<IUserSubscriptionPrivilegeUsageRepository>().Object);
                services.TryAddScoped(_ => new Mock<IBillingAdjustmentRepository>().Object);
                services.TryAddScoped(_ => new Mock<ISubscriptionStatusHistoryRepository>().Object);
                services.TryAddScoped(_ => new Mock<INotificationRepository>().Object);
                services.TryAddScoped(_ => new Mock<IConsultationRepository>().Object);
                services.TryAddScoped(_ => new Mock<IMedicationDeliveryRepository>().Object);
                services.TryAddScoped(_ => new Mock<IVideoCallRepository>().Object);
            });
        }
    }

    public class QuestionnaireSystemTests : IClassFixture<QuestionnaireWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public QuestionnaireSystemTests(QuestionnaireWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Can_Create_QuestionnaireTemplate()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Health Assessment Template",
                Description = "Comprehensive health assessment questionnaire",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your current age?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Do you have any chronic conditions?",
                        Type = QuestionType.Radio,
                        IsRequired = false,
                        Order = 2,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Yes", Value = "yes", Order = 1 },
                            new CreateQuestionOptionDto { Text = "No", Value = "no", Order = 2 }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Can_Get_QuestionnaireTemplate_By_Id()
        {
            // Arrange: Create a template first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Get Template Test",
                Description = "Template for get test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Act
            var response = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("Get Template Test");
        }

        [Fact]
        public async Task Can_Get_All_QuestionnaireTemplates()
        {
            // Act
            var response = await _client.GetAsync("/api/questionnaire/templates");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<QuestionnaireTemplateDto>>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_Update_QuestionnaireTemplate()
        {
            // Arrange: Create a template first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Update Template Test",
                Description = "Template for update test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Original question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Update the template
            var updateDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Updated Template Name",
                Description = "Updated description",
                CategoryId = createDto.CategoryId,
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Updated question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/questionnaire/templates/{templateId}", updateDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_Delete_QuestionnaireTemplate()
        {
            // Arrange: Create a template first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Delete Template Test",
                Description = "Template for delete test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Act
            var response = await _client.DeleteAsync($"/api/questionnaire/templates/{templateId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Can_Submit_UserResponse()
        {
            // Arrange: Create a template first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "User Response Test Template",
                Description = "Template for user response test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your name?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit user response
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId, // Use the actual question ID
                        AnswerText = "John Doe",
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Can_Get_UserResponse_By_Id()
        {
            // Arrange: Create template and submit response first
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Get Response Test Template",
                Description = "Template for get response test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId, // Use the actual question ID
                        AnswerText = "Test answer",
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            var submitResponse = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            var submitResult = await submitResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var responseId = submitResult!.Data;

            // Act
            var response = await _client.GetAsync($"/api/questionnaire/responses/{responseId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_Get_UserResponses_By_User_Id()
        {
            // Arrange: Create template and submit response first
            var userId = Guid.NewGuid();
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "User Responses Test Template",
                Description = "Template for user responses test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        AnswerText = "Test answer",
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);

            // Act
            var response = await _client.GetAsync($"/api/questionnaire/responses/user/{userId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserResponseDto>>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_Upload_File_With_Questionnaire()
        {
            // Arrange: Create a template with file upload
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "File Upload Test Template",
                Description = "Template for file upload test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Upload a document",
                        Type = QuestionType.Text, // Changed from "file" since file type doesn't exist in enum
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            // Create multipart form data
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(JsonConvert.SerializeObject(createDto)), "templateJson");

            // Add a test file
            var fileContent = new StringContent("Test file content");
            formData.Add(fileContent, "files", "test.txt");

            // Act
            var response = await _client.PostAsync("/api/questionnaire/templates/with-files", formData);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Returns_404_For_NonExistent_Template()
        {
            // Act
            var response = await _client.GetAsync($"/api/questionnaire/templates/{Guid.NewGuid()}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_404_For_NonExistent_UserResponse()
        {
            // Act
            var response = await _client.GetAsync($"/api/questionnaire/responses/{Guid.NewGuid()}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Can_Submit_UserResponse_With_Date_Question()
        {
            // Arrange: Create a template with date question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Date Test Template",
                Description = "Template for date question test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your birth date?",
                        Type = QuestionType.Date,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            var userId = Guid.NewGuid();
            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        DateTimeValue = new DateTime(1990, 5, 15), // Birth date
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Can_Submit_UserResponse_With_DateTime_Question()
        {
            // Arrange: Create a template with datetime question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "DateTime Test Template",
                Description = "Template for datetime question test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "When is your next appointment?",
                        Type = QuestionType.DateTime,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            var userId = Guid.NewGuid();
            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        DateTimeValue = new DateTime(2024, 12, 25, 14, 30, 0), // Appointment date and time
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Can_Submit_UserResponse_With_Time_Question()
        {
            // Arrange: Create a template with time question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Time Test Template",
                Description = "Template for time question test",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What time do you usually take your medication?",
                        Type = QuestionType.Time,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            var userId = Guid.NewGuid();
            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        DateTimeValue = new DateTime(2024, 1, 1, 8, 30, 0), // 8:30 AM
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        // ========== COMPREHENSIVE QUESTION TYPE TESTS ==========

        [Fact]
        public async Task Can_Create_Template_With_All_Question_Types()
        {
            // Arrange: Create a comprehensive template with all question types
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Comprehensive Health Assessment",
                Description = "Template testing all question types",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    // Text question
                    new CreateQuestionDto
                    {
                        Text = "What is your full name?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    // TextArea question
                    new CreateQuestionDto
                    {
                        Text = "Describe your current symptoms in detail",
                        Type = QuestionType.TextArea,
                        IsRequired = true,
                        Order = 2
                    },
                    // Radio question
                    new CreateQuestionDto
                    {
                        Text = "What is your gender?",
                        Type = QuestionType.Radio,
                        IsRequired = true,
                        Order = 3,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Male", Value = "male", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Female", Value = "female", Order = 2 },
                            new CreateQuestionOptionDto { Text = "Other", Value = "other", Order = 3 }
                        }
                    },
                    // Checkbox question
                    new CreateQuestionDto
                    {
                        Text = "Which symptoms do you experience? (Select all that apply)",
                        Type = QuestionType.Checkbox,
                        IsRequired = false,
                        Order = 4,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Headache", Value = "headache", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Fever", Value = "fever", Order = 2 },
                            new CreateQuestionOptionDto { Text = "Cough", Value = "cough", Order = 3 },
                            new CreateQuestionOptionDto { Text = "Fatigue", Value = "fatigue", Order = 4 }
                        }
                    },
                    // Dropdown question
                    new CreateQuestionDto
                    {
                        Text = "What is your blood type?",
                        Type = QuestionType.Dropdown,
                        IsRequired = true,
                        Order = 5,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "A+", Value = "A+", Order = 1 },
                            new CreateQuestionOptionDto { Text = "A-", Value = "A-", Order = 2 },
                            new CreateQuestionOptionDto { Text = "B+", Value = "B+", Order = 3 },
                            new CreateQuestionOptionDto { Text = "B-", Value = "B-", Order = 4 },
                            new CreateQuestionOptionDto { Text = "AB+", Value = "AB+", Order = 5 },
                            new CreateQuestionOptionDto { Text = "AB-", Value = "AB-", Order = 6 },
                            new CreateQuestionOptionDto { Text = "O+", Value = "O+", Order = 7 },
                            new CreateQuestionOptionDto { Text = "O-", Value = "O-", Order = 8 }
                        }
                    },
                    // Range question
                    new CreateQuestionDto
                    {
                        Text = "Rate your pain level (1-10)",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 6,
                        MinValue = 1,
                        MaxValue = 10,
                        StepValue = 1
                    },
                    // Date question
                    new CreateQuestionDto
                    {
                        Text = "What is your date of birth?",
                        Type = QuestionType.Date,
                        IsRequired = true,
                        Order = 7
                    },
                    // DateTime question
                    new CreateQuestionDto
                    {
                        Text = "When is your next appointment?",
                        Type = QuestionType.DateTime,
                        IsRequired = true,
                        Order = 8
                    },
                    // Time question
                    new CreateQuestionDto
                    {
                        Text = "What time do you usually take your medication?",
                        Type = QuestionType.Time,
                        IsRequired = true,
                        Order = 9
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();

            // Verify template was created with all questions
            var getResponse = await _client.GetAsync($"/api/questionnaire/templates/{result.Data}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var template = await getResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            template!.Data!.Questions.Should().HaveCount(9);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Text);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.TextArea);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Radio);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Checkbox);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Dropdown);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Range);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Date);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.DateTime);
            template.Data.Questions.Should().Contain(q => q.Type == QuestionType.Time);
        }

        [Fact]
        public async Task Can_Submit_Response_With_All_Question_Types()
        {
            // Arrange: Create template with all question types
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "All Types Response Test",
                Description = "Testing responses for all question types",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Your name?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Describe symptoms",
                        Type = QuestionType.TextArea,
                        IsRequired = true,
                        Order = 2
                    },
                    new CreateQuestionDto
                    {
                        Text = "Gender?",
                        Type = QuestionType.Radio,
                        IsRequired = true,
                        Order = 3,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Male", Value = "male", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Female", Value = "female", Order = 2 }
                        }
                    },
                    new CreateQuestionDto
                    {
                        Text = "Symptoms?",
                        Type = QuestionType.Checkbox,
                        IsRequired = false,
                        Order = 4,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Headache", Value = "headache", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Fever", Value = "fever", Order = 2 }
                        }
                    },
                    new CreateQuestionDto
                    {
                        Text = "Blood type?",
                        Type = QuestionType.Dropdown,
                        IsRequired = true,
                        Order = 5,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "A+", Value = "A+", Order = 1 },
                            new CreateQuestionOptionDto { Text = "O+", Value = "O+", Order = 2 }
                        }
                    },
                    new CreateQuestionDto
                    {
                        Text = "Pain level?",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 6,
                        MinValue = 1,
                        MaxValue = 10,
                        StepValue = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Birth date?",
                        Type = QuestionType.Date,
                        IsRequired = true,
                        Order = 7
                    },
                    new CreateQuestionDto
                    {
                        Text = "Next appointment?",
                        Type = QuestionType.DateTime,
                        IsRequired = true,
                        Order = 8
                    },
                    new CreateQuestionDto
                    {
                        Text = "Medication time?",
                        Type = QuestionType.Time,
                        IsRequired = true,
                        Order = 9
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get template to get question IDs
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questions = templateResult!.Data!.Questions;

            // Submit comprehensive response
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[0].Id, // Text
                        AnswerText = "John Doe",
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[1].Id, // TextArea
                        AnswerText = "Experiencing severe headache and fatigue for the past 3 days",
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[2].Id, // Radio
                        AnswerText = null,
                        SelectedOptionIds = new List<Guid> { questions[2].Options[0].Id } // Male
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[3].Id, // Checkbox
                        AnswerText = null,
                        SelectedOptionIds = new List<Guid> { questions[3].Options[0].Id, questions[3].Options[1].Id } // Headache + Fever
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[4].Id, // Dropdown
                        AnswerText = null,
                        SelectedOptionIds = new List<Guid> { questions[4].Options[1].Id } // O+
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[5].Id, // Range
                        AnswerText = null,
                        NumericValue = 7.5m,
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[6].Id, // Date
                        AnswerText = null,
                        DateTimeValue = new DateTime(1990, 5, 15),
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[7].Id, // DateTime
                        AnswerText = null,
                        DateTimeValue = new DateTime(2024, 12, 25, 14, 30, 0),
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[8].Id, // Time
                        AnswerText = null,
                        DateTimeValue = new DateTime(2024, 1, 1, 8, 30, 0),
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();

            // Verify response was saved correctly
            var getResponseResponse = await _client.GetAsync($"/api/questionnaire/responses/{result.Data}");
            getResponseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var savedResponse = await getResponseResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>();
            savedResponse!.Data!.Answers.Should().HaveCount(9);

            // Verify each answer type
            var textAnswer = savedResponse.Data.Answers.FirstOrDefault(a => !string.IsNullOrEmpty(a.AnswerText));
            textAnswer.Should().NotBeNull();
            textAnswer!.AnswerText.Should().Be("John Doe");

            var textAreaAnswer = savedResponse.Data.Answers.FirstOrDefault(a => a.AnswerText?.Length > 50);
            textAreaAnswer.Should().NotBeNull();
            textAreaAnswer!.AnswerText.Should().Contain("severe headache");

            var radioAnswer = savedResponse.Data.Answers.FirstOrDefault(a => a.SelectedOptionIds.Count == 1);
            radioAnswer.Should().NotBeNull();

            var checkboxAnswer = savedResponse.Data.Answers.FirstOrDefault(a => a.SelectedOptionIds.Count > 1);
            checkboxAnswer.Should().NotBeNull();
            checkboxAnswer!.SelectedOptionIds.Should().HaveCount(2);

            var rangeAnswer = savedResponse.Data.Answers.FirstOrDefault(a => a.NumericValue.HasValue);
            rangeAnswer.Should().NotBeNull();
            rangeAnswer!.NumericValue.Should().Be(7.5m);

            var dateAnswers = savedResponse.Data.Answers.Where(a => a.DateTimeValue.HasValue).ToList();
            dateAnswers.Should().HaveCount(3);
        }

        // ========== EDGE CASES AND VALIDATION TESTS ==========

        [Fact]
        public async Task Should_Reject_Invalid_Question_Type()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Invalid Type Test",
                Description = "Testing invalid question type",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question",
                        Type = (QuestionType)999, // Invalid type
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Multiple_Choice_Without_Options()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "No Options Test",
                Description = "Testing multiple choice without options",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Select your preference",
                        Type = QuestionType.Radio,
                        IsRequired = true,
                        Order = 1,
                        Options = new List<CreateQuestionOptionDto>() // Empty options
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Range_With_Invalid_Bounds()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Invalid Range Test",
                Description = "Testing range with invalid bounds",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Rate your pain",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 1,
                        MinValue = 10, // Min > Max
                        MaxValue = 5
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Empty_Template_Name()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "", // Empty name
                Description = "Test description",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Template_Without_Questions()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "No Questions Test",
                Description = "Testing template without questions",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>() // Empty questions
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Empty_Question_Text()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Empty Question Text Test",
                Description = "Testing question with empty text",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "", // Empty text
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Duplicate_Question_Orders()
        {
            // Arrange
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Duplicate Order Test",
                Description = "Testing duplicate question orders",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "First question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Second question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1 // Duplicate order
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Handle_Large_Text_Answers()
        {
            // Arrange: Create template with textarea
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Large Text Test",
                Description = "Testing large text answers",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Describe your medical history in detail",
                        Type = QuestionType.TextArea,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Create large text answer
            var largeText = new string('A', 3000); // 3000 character answer
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        AnswerText = largeText,
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Handle_Multiple_Checkbox_Selections()
        {
            // Arrange: Create template with checkbox
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Multiple Checkbox Test",
                Description = "Testing multiple checkbox selections",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Select all that apply",
                        Type = QuestionType.Checkbox,
                        IsRequired = true,
                        Order = 1,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "Option 1", Value = "opt1", Order = 1 },
                            new CreateQuestionOptionDto { Text = "Option 2", Value = "opt2", Order = 2 },
                            new CreateQuestionOptionDto { Text = "Option 3", Value = "opt3", Order = 3 },
                            new CreateQuestionOptionDto { Text = "Option 4", Value = "opt4", Order = 4 }
                        }
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get template and question IDs
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var question = templateResult!.Data!.Questions[0];

            // Submit response with multiple selections
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = question.Id,
                        AnswerText = null,
                        SelectedOptionIds = new List<Guid> 
                        { 
                            question.Options[0].Id, 
                            question.Options[2].Id, 
                            question.Options[3].Id 
                        } // Select 3 options
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();

            // Verify multiple selections were saved
            var getResponseResponse = await _client.GetAsync($"/api/questionnaire/responses/{result.Data}");
            var savedResponse = await getResponseResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>();
            var answer = savedResponse!.Data!.Answers.First();
            answer.SelectedOptionIds.Should().HaveCount(3);
        }

        [Fact]
        public async Task Should_Handle_Range_Values_With_Decimals()
        {
            // Arrange: Create template with range question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Decimal Range Test",
                Description = "Testing range values with decimals",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Rate your satisfaction (0.0 to 10.0)",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 1,
                        MinValue = 0.0m,
                        MaxValue = 10.0m,
                        StepValue = 0.5m
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit response with decimal value
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        AnswerText = null,
                        NumericValue = 7.5m,
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();

            // Verify decimal value was saved correctly
            var getResponseResponse = await _client.GetAsync($"/api/questionnaire/responses/{result.Data}");
            var savedResponse = await getResponseResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>();
            var answer = savedResponse!.Data!.Answers.First();
            answer.NumericValue.Should().Be(7.5m);
        }

        [Fact]
        public async Task Should_Handle_Response_Status_Changes()
        {
            // Arrange: Create template
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Status Change Test",
                Description = "Testing response status changes",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Test question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get question ID
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit response with different statuses
            var statuses = new[] { ResponseStatus.Draft, ResponseStatus.InProgress, ResponseStatus.Completed, ResponseStatus.Submitted };

            foreach (var status in statuses)
            {
                var responseDto = new CreateUserResponseDto
                {
                    UserId = Guid.NewGuid(),
                    CategoryId = createDto.CategoryId,
                    TemplateId = templateId,
                    Status = status,
                    Answers = new List<CreateUserAnswerDto>
                    {
                        new CreateUserAnswerDto
                        {
                            QuestionId = questionId,
                            AnswerText = $"Answer for {status}",
                            SelectedOptionIds = new List<Guid>()
                        }
                    }
                };

                // Act
                var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
                
                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
                result.Should().NotBeNull();
                result!.Success.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Should_Handle_Required_Field_Validation()
        {
            // Arrange: Create template with required questions
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Required Fields Test",
                Description = "Testing required field validation",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Required text question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Optional text question",
                        Type = QuestionType.Text,
                        IsRequired = false,
                        Order = 2
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get question IDs
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questions = templateResult!.Data!.Questions;

            // Submit response with only required field
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[0].Id, // Required question
                        AnswerText = "Required answer",
                        SelectedOptionIds = new List<Guid>()
                    }
                    // Note: Not answering optional question
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Handle_Media_Urls_In_Questions()
        {
            // Arrange: Create template with media URLs
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Media URLs Test",
                Description = "Testing questions with media URLs",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Question with image",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1,
                        MediaUrl = "https://example.com/image.jpg"
                    },
                    new CreateQuestionDto
                    {
                        Text = "Question with video",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 2,
                        MediaUrl = "https://example.com/video.mp4"
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();

            // Verify media URLs were saved
            var getResponse = await _client.GetAsync($"/api/questionnaire/templates/{result.Data}");
            var template = await getResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            template!.Data!.Questions.Should().Contain(q => !string.IsNullOrEmpty(q.MediaUrl));
            template.Data.Questions.Should().Contain(q => q.MediaUrl == "https://example.com/image.jpg");
            template.Data.Questions.Should().Contain(q => q.MediaUrl == "https://example.com/video.mp4");
        }

        [Fact]
        public async Task Should_Handle_Help_Text_In_Questions()
        {
            // Arrange: Create template with help text
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Help Text Test",
                Description = "Testing questions with help text",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your age?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1,
                        HelpText = "Please enter your age in years"
                    },
                    new CreateQuestionDto
                    {
                        Text = "Rate your pain",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 2,
                        HelpText = "1 = No pain, 10 = Worst pain imaginable",
                        MinValue = 1,
                        MaxValue = 10,
                        StepValue = 1
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();

            // Verify help text was saved
            var getResponse = await _client.GetAsync($"/api/questionnaire/templates/{result.Data}");
            var template = await getResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            template!.Data!.Questions.Should().Contain(q => !string.IsNullOrEmpty(q.HelpText));
            template.Data.Questions.Should().Contain(q => q.HelpText == "Please enter your age in years");
            template.Data.Questions.Should().Contain(q => q.HelpText == "1 = No pain, 10 = Worst pain imaginable");
        }

        [Fact]
        public async Task Can_Get_UserResponse_With_DateTime_Values()
        {
            // Arrange: Create a template with mixed question types including date/time
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Mixed DateTime Test Template",
                Description = "Template with mixed question types including date/time",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "What is your name?",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "What is your birth date?",
                        Type = QuestionType.Date,
                        IsRequired = true,
                        Order = 2
                    },
                    new CreateQuestionDto
                    {
                        Text = "When is your next appointment?",
                        Type = QuestionType.DateTime,
                        IsRequired = true,
                        Order = 3
                    },
                    new CreateQuestionDto
                    {
                        Text = "What time do you take your medication?",
                        Type = QuestionType.Time,
                        IsRequired = true,
                        Order = 4
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;

            // Get the template to get the actual question IDs
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questions = templateResult!.Data!.Questions;

            var userId = Guid.NewGuid();
            var responseDto = new CreateUserResponseDto
            {
                UserId = userId,
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[0].Id, // Text question
                        AnswerText = "John Doe",
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[1].Id, // Date question
                        DateTimeValue = new DateTime(1990, 5, 15),
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[2].Id, // DateTime question
                        DateTimeValue = new DateTime(2024, 12, 25, 14, 30, 0),
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[3].Id, // Time question
                        DateTimeValue = new DateTime(2024, 1, 1, 8, 30, 0),
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };

            // Submit the response
            var submitResponse = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            var submitResult = await submitResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var responseId = submitResult!.Data;

            // Act: Get the response back
            var getResponse = await _client.GetAsync($"/api/questionnaire/responses/{responseId}");
            
            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await getResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponseDto>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Answers.Should().HaveCount(4);
            
            // Verify the different answer types - find answers by question type instead of assuming order
            var textAnswer = result.Data.Answers.FirstOrDefault(a => !string.IsNullOrEmpty(a.AnswerText));
            textAnswer.Should().NotBeNull();
            textAnswer!.AnswerText.Should().Be("John Doe");
            
            var dateAnswers = result.Data.Answers.Where(a => a.DateTimeValue.HasValue).ToList();
            dateAnswers.Should().HaveCount(3);
            
            // Verify we have the expected date values (order doesn't matter)
            var expectedDates = new[]
            {
                new DateTime(1990, 5, 15),
                new DateTime(2024, 12, 25, 14, 30, 0),
                new DateTime(2024, 1, 1, 8, 30, 0)
            };
            
            dateAnswers.Select(a => a.DateTimeValue!.Value.Date).Should().Contain(expectedDates.Select(d => d.Date));
            dateAnswers.Select(a => a.DateTimeValue!.Value.TimeOfDay).Should().Contain(expectedDates.Select(d => d.TimeOfDay));
        }

        [Fact]
        public async Task Should_Reject_Response_With_Missing_Required_Answers()
        {
            // Arrange: Create template with required and optional questions
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Missing Required Answer Test",
                Description = "Test missing required answers",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Required question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new CreateQuestionDto
                    {
                        Text = "Optional question",
                        Type = QuestionType.Text,
                        IsRequired = false,
                        Order = 2
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questions = templateResult!.Data!.Questions;

            // Submit response missing required answer
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    // Only answer the optional question
                    new CreateUserAnswerDto
                    {
                        QuestionId = questions[1].Id,
                        AnswerText = "Optional answer",
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Response_With_Extra_Answers_Not_In_Template()
        {
            // Arrange: Create template with one question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Extra Answers Test",
                Description = "Test extra answers",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Valid question",
                        Type = QuestionType.Text,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit response with an extra answer (random GUID)
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        AnswerText = "Valid answer",
                        SelectedOptionIds = new List<Guid>()
                    },
                    new CreateUserAnswerDto
                    {
                        QuestionId = Guid.NewGuid(), // Not in template
                        AnswerText = "Extra answer",
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Response_With_Invalid_Option_Ids()
        {
            // Arrange: Create template with radio question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Invalid Option IDs Test",
                Description = "Test invalid option IDs",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Choose one",
                        Type = QuestionType.Radio,
                        IsRequired = true,
                        Order = 1,
                        Options = new List<CreateQuestionOptionDto>
                        {
                            new CreateQuestionOptionDto { Text = "A", Value = "A", Order = 1 },
                            new CreateQuestionOptionDto { Text = "B", Value = "B", Order = 2 }
                        }
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var question = templateResult!.Data!.Questions[0];

            // Submit response with invalid option ID
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = question.Id,
                        SelectedOptionIds = new List<Guid> { Guid.NewGuid() } // Not a valid option
                    }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Response_With_OutOfRange_Range_Value()
        {
            // Arrange: Create template with range question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "OutOfRange Range Test",
                Description = "Test out-of-range range value",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Rate 1-10",
                        Type = QuestionType.Range,
                        IsRequired = true,
                        Order = 1,
                        MinValue = 1,
                        MaxValue = 10,
                        StepValue = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit response with out-of-range value
            var responseDto = new CreateUserResponseDto
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new List<CreateUserAnswerDto>
                {
                    new CreateUserAnswerDto
                    {
                        QuestionId = questionId,
                        NumericValue = 20, // Out of range
                        SelectedOptionIds = new List<Guid>()
                    }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/questionnaire/responses", responseDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Reject_Response_With_Invalid_DateTime_Format()
        {
            // Arrange: Create template with date question
            var createDto = new CreateQuestionnaireTemplateDto
            {
                Name = "Invalid DateTime Format Test",
                Description = "Test invalid date/time format",
                CategoryId = Guid.NewGuid(),
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Birth date",
                        Type = QuestionType.Date,
                        IsRequired = true,
                        Order = 1
                    }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/questionnaire/templates", createDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var templateId = createResult!.Data;
            var getTemplateResponse = await _client.GetAsync($"/api/questionnaire/templates/{templateId}");
            var templateResult = await getTemplateResponse.Content.ReadFromJsonAsync<ApiResponse<QuestionnaireTemplateDto>>();
            var questionId = templateResult!.Data!.Questions[0].Id;

            // Submit response with invalid date (simulate by sending a string instead of DateTime)
            var payload = new
            {
                UserId = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                TemplateId = templateId,
                Status = ResponseStatus.Completed,
                Answers = new[]
                {
                    new
                    {
                        QuestionId = questionId,
                        DateTimeValue = "not-a-date",
                        SelectedOptionIds = new Guid[] { }
                    }
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/questionnaire/responses", content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
} 