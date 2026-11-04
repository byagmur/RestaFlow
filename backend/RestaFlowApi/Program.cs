using RestaFlowApi.Repos;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTPS
builder.WebHost.UseKestrel(options =>
{
    options.ListenLocalhost(7253, listenOptions =>
    {
        listenOptions.UseHttps(); // Enable HTTPS
    });
});


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNuxtApp",
        policy => policy.WithOrigins("http://localhost:3000")
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials()
                         .AllowAnyMethod());
});

// Add controllers
builder.Services.AddControllers();
builder.Services.AddScoped<OrderRepository>();


// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use HTTPS redirection
app.UseHttpsRedirection();

// Use CORS
 app.UseCors("AllowNuxtApp");

app.UseAuthentication();
app.UseAuthorization();


// Use logging middleware
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Client : {ClientIp}", context.Connection.RemoteIpAddress);
    logger.LogInformation("Application : {ApplicationName}", context.Request.Headers["User-Agent"]);
    logger.LogInformation("{Tarih} >> Handling request: {Method} {Path}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), context.Request.Method, context.Request.Path);
    await next.Invoke();
    logger.LogInformation("Finished handling request.");
});


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restoran API v1");
    c.RoutePrefix = string.Empty; // Show Swagger at the root
});

app.MapControllers();

app.Run();
