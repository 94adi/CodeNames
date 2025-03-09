namespace CodeNames.BuildingBlocks.Helpers;

public static class EnvironmentUtils
{
    public static string GetEnvironmentVariable()
    {
        var environment = Environment.GetEnvironmentVariable(EnvironmentConstants.ENVIRONMENT_STRING)
            ?? EnvironmentConstants.DEVELOPMENT;

        return environment.ToLower();
    }
}


public static class EnvironmentConstants
{
    public const string DEVELOPMENT = "development";
    public const string DOCKER = "docker";
    public const string AZURE = "azure";
    public const string ENVIRONMENT_STRING = "ASPNETCORE_ENVIRONMENT";
}
