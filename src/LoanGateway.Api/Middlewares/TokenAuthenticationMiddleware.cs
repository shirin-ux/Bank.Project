using Microsoft.AspNetCore.Http;

namespace LoanService.Api.Middlewares
{
    public class TokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenAuthenticationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token) || !await ValidateTokenAsync(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("توکن معتبر نیست.");
                return;
            }

            await _next(context);
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://192.168.87.12:3000?token={token}");

            return response.IsSuccessStatusCode;
        }
    }

}
