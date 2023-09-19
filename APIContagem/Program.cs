using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using APIContagem.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    var useAspNetCoreRateLimit = Convert.ToBoolean(builder.Configuration["UseAspNetCoreRateLimit"]);
    if (useAspNetCoreRateLimit)
    {
        builder.Services.AddMemoryCache();
        RateLimitExtensions.Initialize(builder.Configuration);

        builder.Services.Configure<ClientRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.HttpStatusCode = 429;
            options.ClientIdHeader = RateLimitExtensions.ClientIdHeaderName;
            options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "GET:/contador",
                        Period = RateLimitExtensions.Period,
                        Limit = RateLimitExtensions.Limit,
                        QuotaExceededResponse = new ()
                        {
                            ContentType = "application/text",
                            Content = RateLimitExtensions.QuotaExceededMessage
                        }
                    }
                };
        });

        builder.Services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        builder.Services.AddInMemoryRateLimiting();
    }
#endif

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "APIContagem",
            Description = "Exemplo de implementacao de API para contagem de acessos",
            Version = "v1",
            Contact = new OpenApiContact()
            {
                Name = "Renato Groffe",
                Url = new Uri("https://github.com/renatogroffe"),
            },
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("http://opensource.org/licenses/MIT"),
            }
        });
    
    #if DEBUG
        c.OperationFilter<ClientIdHeaderOperationFilter>();
    #endif
});

var app = builder.Build();

#if DEBUG
    if (useAspNetCoreRateLimit)
        app.UseClientRateLimiting();
#endif

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();