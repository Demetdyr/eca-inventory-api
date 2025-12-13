using System.Reflection;
using System.Security.Claims;
using DbUp;
using DotNetEnv;
using EcaIncentoryApi.Service;
using EcaInventoryApi.Config;
using EcaInventoryApi.Consumer;
using EcaInventoryApi.Data;
using EcaInventoryApi.Middleware;
using EcaInventoryApi.Model;
using EcaInventoryApi.Publisher;
using EcaInventoryApi.Repository;
using EcaInventoryApi.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var keycloakUrl = Environment.GetEnvironmentVariable("KEYCLOAK_URL")
                       ?? throw new Exception("KEYCLOAK_URL not set in .env");
var keycloakRealm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM")
                       ?? throw new Exception("KEYCLOAK_REALM not set in .env");

var connectionString = Environment.GetEnvironmentVariable("DATASTORE_URL")
                       ?? throw new Exception("DATASTORE_URL not set in .env");
var inventoryClientId = Environment.GetEnvironmentVariable("INVENTORY_CLIENT_ID")
                       ?? throw new Exception("INVENTORY_CLIENT_ID not set in .env");
var inventoryClientSecret = Environment.GetEnvironmentVariable("INVENTORY_CLIENT_SECRET")
                       ?? throw new Exception("INVENTORY_CLIENT_SECRET not set in .env");

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IConnection>(sp =>
{
	var opts = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

	return RabbitMqConnectionFactory
		.CreateConnectionAsync(opts)
		.GetAwaiter()
		.GetResult(); 
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = $"{keycloakUrl}/realms/{keycloakRealm}";
        options.Audience = inventoryClientId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccess))
                {
                    var roles = System.Text.Json.JsonDocument.Parse(realmAccess)
                        .RootElement.GetProperty("roles")
                        .EnumerateArray()
                        .Select(r => r.GetString());

                    var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                    foreach (var role in roles)
                        claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role!));
                }
                return Task.CompletedTask;
            }
        };
    });

EnsureDatabaseMigration(connectionString);


builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    connectionString,
    o => o.MapEnum<ReservationStatus>("reservation_status")));

builder.Services.AddScoped<IStockItemRepository, StockItemRepository>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

builder.Services.AddHostedService<OrderCreatedConsumer>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Keycloak için OAuth2 yapılandırması
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Description = "Keycloak Login with OAuth2",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",

        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakUrl}/realms/{keycloakRealm}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{keycloakUrl}/realms/{keycloakRealm}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect scope" },
                    { "profile", "User profile" }
                }
            }
        }
    });

    // Swagger dokümanında auth zorunlu endpointleri otomatik etiketle
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile" }
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");

        options.OAuthClientId(inventoryClientId);
        options.OAuthClientSecret(inventoryClientSecret);
        options.OAuthUsePkce();
        options.OAuthScopes("openid", "profile");
        options.OAuthAppName("Keycloak + Swagger Demo");
    });

}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();


app.UseHttpsRedirection();
app.MapControllers();

app.Run();

static void EnsureDatabaseMigration(string connectionString)
{
    var upgrader =
        DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();

    var result = upgrader.PerformUpgrade();

    if (!result.Successful)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Database migration failed: {result.Error}");
        Console.ResetColor();
        throw new Exception("Database migration failed, aborting startup.");
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Database migrations applied successfully.");
    Console.ResetColor();
}

