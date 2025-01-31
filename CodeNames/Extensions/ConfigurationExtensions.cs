using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CodeNames.Utils;

namespace CodeNames.Extensions;

public static class ConfigurationExtensions
{
    public static void RegisterAzureConfigs(this WebApplicationBuilder appBuilder)
    {
        var keyVaultName = appBuilder.Configuration["AzureConfig:KeyVaultName"];

        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");

        var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

        var signalRConnectionString = secretClient.GetSecret(SecretNames.SignalR_ConnectionString);
        var emailConfigApiKey = secretClient.GetSecret(SecretNames.EmailConfig_ApiKey);
        var emailConfigFromEmail = secretClient.GetSecret(SecretNames.EmailConfig_FromEmail);
        var emailConfigApiUrl = secretClient.GetSecret(SecretNames.EmailConfig_ApiUrl);

        appBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "SignalRConfig:ConnectionString", signalRConnectionString.Value.Value },
            { "EmailConfig:ApiKey", emailConfigApiKey.Value.Value},
            { "EmailConfig:FromEmail", emailConfigFromEmail.Value.Value},
            { "EmailConfig:ApiUrl", emailConfigApiUrl.Value.Value},
        });

        appBuilder.Services.Configure<EmailConfig>(appBuilder.Configuration.GetSection("EmailConfig"));

        appBuilder.Services.Configure<GameParametersOptions>(appBuilder.Configuration.GetSection("GameVariables"));
    }
}
