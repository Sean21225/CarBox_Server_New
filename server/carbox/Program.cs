using carbox.Date;
using carbox.Repositories;
using carbox.Services;

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // React
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the CORS policy
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
