using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ HttpClient برای Auth با Client Certificate
builder.Services.AddHttpClient("auth-mtls")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(
            new X509Certificate2(
                builder.Configuration["Certificates:AuthClient:PfxPath"],
                builder.Configuration["Certificates:AuthClient:Password"],
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.EphemeralKeySet
            )
        );
        return handler;
    });

// 2️⃣ YARP
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 3️⃣ JWT Validation (Client Credentials)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://auth.internal.local"; // IdentityServer یا Auth Service
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidAudience = "internal-services"
        };
    });

// 4️⃣ Policy فقط برای Investment
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ServiceOnly", policy =>
    {
        policy.RequireClaim("client_id", "investment-service");
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 5️⃣ Map Reverse Proxy و اعمال Policy
app.MapReverseProxy()
   .RequireAuthorization("ServiceOnly");

app.Run();
