using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Unicode;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using HealthChecks.Models;

namespace HealthChecks.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly IConfiguration _configuration;    

    private readonly SecretClient _secretClient;

    public DataController(IConfiguration configuration)
    {
        this._configuration = configuration;                

        this._secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("AzureKeyVaultURI")),       
                                              new InteractiveBrowserCredential());
    }
    [HttpPost]
    [Route("PublishToEventhub")]
    public async Task<string> PublishStringDataToEventhub (string data)
    {
        try
        {
            var value = _secretClient.GetSecret(_configuration.GetValue<string>("EventhubConnectionStringIdentifier"));
            string eventhubConnectionString = _secretClient.GetSecret(_configuration.GetValue<string>("EventhubConnectionStringIdentifier")).Value.Value;
            EventHubProducerClient producerClient = new EventHubProducerClient(eventhubConnectionString, "dataeventhub");

            List<EventData> eventDatas = new List<EventData>();

            var eventdata = new EventData(Encoding.UTF8.GetBytes(data));

            eventDatas.Add(eventdata);

            await producerClient.SendAsync(eventDatas);

            return "Successfully published the message";

        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            throw;
        }
        
    }

    [HttpPost]
    [Route("PublishToServiceBusQueue")]

    public async Task<string> PublishStringDataToServiceBusQueue(string data)
    {
        try
        {
            string output = "";

            string serviceBusConnectionString = _secretClient.GetSecret(_configuration.GetValue<string>("ServiceBusConnectionStringIdentifier")).Value.Value;

            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(_configuration.GetValue<string>("queueName"));

            ServiceBusMessage serviceBusMessage = new(data);

            await serviceBusSender.SendMessageAsync(serviceBusMessage);

            output = "Successfully published the message";

            return output;
        }
        catch (System.Exception)
        {            
            throw;
        }
        
    }

    [HttpPost]
    [Route("AddEmployeeToSQLDBTable")]

    public async Task<string> AddEmployee(Employee employee)
    {
        try
        {
            string output = "";

            System.Data.SqlClient.SqlConnection dbConnection = new(_configuration.GetValue<string>("SQLDBConnecrion"));

            System.Data.SqlClient.SqlCommand cmd = new();
            var firstNameSQLParameter = cmd.CreateParameter();
            firstNameSQLParameter.DbType = System.Data.DbType.String;
            firstNameSQLParameter.Direction = System.Data.ParameterDirection.Input;
            firstNameSQLParameter.ParameterName = "firstName";
            firstNameSQLParameter.Value = employee?.FirstName;

            var lastNameSQLParameter = cmd.CreateParameter();
            lastNameSQLParameter.DbType = System.Data.DbType.String;
            lastNameSQLParameter.Direction = System.Data.ParameterDirection.Input;
            lastNameSQLParameter.ParameterName = "lastName";
            lastNameSQLParameter.Value = employee?.LastName;

            cmd.Parameters.Add(firstNameSQLParameter);
            cmd.Parameters.Add(lastNameSQLParameter);

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "INSERT into Employee (FirstName, LastName) VALUES (@firstName, @lastName)";
            cmd.Connection = dbConnection;

            await dbConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await dbConnection.CloseAsync();

            output = "Successfully Added the employee to the employee table";

            return output;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}