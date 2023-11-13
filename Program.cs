using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HealthChecks;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddHealthChecks().AddCheck<EventhubHealthCheck>("Eventhub",HealthStatus.Unhealthy);
string eventhubName = builder.Configuration.GetValue<string>("EventhubName");
string eventhubConnectionStringIdentifier = builder.Configuration.GetValue<string>("EventhubConnectionStringIdentifier");
builder.Services.AddHealthChecks().AddTypeActivatedCheck<EventhubHealthCheck>("Eventhub",HealthStatus.Unhealthy,new object[]{eventhubConnectionStringIdentifier,eventhubName});

var secretClient = new SecretClient(new Uri(builder.Configuration.GetValue<string>("AzureKeyVaultURI")),       
                                              new InteractiveBrowserCredential());

string ServiceBusConnectionStringIdentifier =  builder.Configuration.GetValue<string>("ServiceBusConnectionStringIdentifier");

string queueName = builder.Configuration.GetValue<string>("queueName");

string serviceBusConnectionString = secretClient.GetSecret(ServiceBusConnectionStringIdentifier).Value.Value;

builder.Services.AddHealthChecks().AddAzureServiceBusQueue(serviceBusConnectionString,queueName);

string sqlDBConnectionStringIdentifier = builder.Configuration.GetValue<string>("SQLDBConnectionStringIdentifier");

string SQLDBConnectionString = secretClient.GetSecret(sqlDBConnectionStringIdentifier).Value.Value;

builder.Services.AddHealthChecks().AddSqlServer(SQLDBConnectionString);
// builder.Services.AddHealthChecks()


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/Healthz");

app.Run();
