using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Provider.Controllers;
using Provider.Middlewares;

namespace Provider
{
    public static class Extensions
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                .PartManager
                .ApplicationParts
                .Add(new AssemblyPart(typeof(StudentsController).Assembly));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        []
                    }
                });
            });

            builder.Services.AddSingleton<IStudentRepository, StudentRepository>();
            builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

            var realConnection = builder.Configuration.GetConnectionString("EventHubs");
            var fakeConnection = "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fake";

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddEventHubProducerClient(
                    !string.IsNullOrEmpty(realConnection) ? realConnection : fakeConnection,
                    "my_event_hub");
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddAuthentication("Fake")
                    .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>("Fake", options => { });
            }
            else
            {
                builder.Services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        // Generate token here http://jwtbuilder.jamiekurtz.com/

                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.ValidateIssuer = false;
                        options.TokenValidationParameters.ValidateAudience = false;
                        options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                        options.TokenValidationParameters.ValidateLifetime = false;

                        options.TokenValidationParameters.IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("qwertyuiopasdfghjklzxcvbnm123456"));

                        options.MapInboundClaims = false;
                        options.TokenValidationParameters.NameClaimType = "name";
                        options.TokenValidationParameters.RoleClaimType = "role";
                    });
            }

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RegisteredUser", policy => policy.RequireClaim("sub"));
            });

            return builder;
        }

        public static IApplicationBuilder Configure(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
