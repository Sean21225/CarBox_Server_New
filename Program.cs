using CarboxBackend.Date;
using CarboxBackend.Repositories;
using CarboxBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDBService
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddScoped(provider =>
{
    var mongoDBService = provider.GetRequiredService<MongoDBService>();
    return mongoDBService.Database;
});

// Add MQTTClientService as a Background Service
builder.Services.AddHostedService<MqttService>();
builder.Services.AddScoped<CarService>();

builder.Services.AddScoped<RideOrderRepository>();
builder.Services.AddScoped<CarRepository>();
builder.Services.AddScoped<StationRepository>();
builder.Services.AddScoped<RouteRepository>();
builder.Services.AddScoped<RideService>();

// Add services to the container.
builder.Services.AddControllers();

// CORS - Allow all origins for deployment flexibility
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API testing on Render
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CARBOX API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
});

// Apply the CORS policy
app.UseCors("AllowAll");

// Skip HTTPS redirection in production (Render handles SSL termination)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();