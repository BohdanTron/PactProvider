using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Provider.Contract.Tests.Middlewares
{
    public class ProviderStateMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IStudentRepository _repository;
        private readonly IDictionary<string, Action> _providerStates;

        public ProviderStateMiddleware(RequestDelegate next, IStudentRepository repository)
        {
            _next = next;
            _repository = repository;

            _providerStates = new Dictionary<string, Action>
            {
                { "student with id 10 exists", Student10Exists },
                { "no auth token is provided", Student10Exists }
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Value?.StartsWith("/provider-states") ?? false)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;

            if (context.Request.Method.Equals(HttpMethod.Post.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();

                var providerState = JsonSerializer.Deserialize<ProviderState>(requestBody, 
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (!string.IsNullOrEmpty(providerState?.State))
                    _providerStates[providerState.State].Invoke();

                await context.Response.WriteAsync(string.Empty);
            }
        }


        private void Student10Exists()
        {
            var student = new Student
            {
                Id = 10,
                FirstName = "Lars",
                LastName = "Ulrich",
                Address = "1234, 56th Street, New York, USA",
                Gender = "male"
            };

            _repository.Add(student);
        }
    }
}
