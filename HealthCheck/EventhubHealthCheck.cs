using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks;


public class EventhubHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly string eventhubConnectionStringIdentifier;
    private readonly string eventhubName;
    private readonly SecretClient _secretClient;

    public EventhubHealthCheck(IConfiguration configuration, string eventhubConnectionStringIdentifier, string eventhubName)
    {
        _configuration = configuration;
        this.eventhubConnectionStringIdentifier = eventhubConnectionStringIdentifier;
        this.eventhubName = eventhubName;
        this._secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("AzureKeyVaultURI")),       
                                              new InteractiveBrowserCredential());
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {        
        string eventhubConnectionString = _secretClient.GetSecret(eventhubConnectionStringIdentifier).Value.Value;

        EventHubProducerClient evenhubClient = new(eventhubConnectionString, eventhubName);        

        return HealthCheckResult.Healthy();

    }
}