namespace ConfigurationWizard.models
{

    public record GetDatabasesParams(string UserId, string DataSource, string Password);


    public record Databases(string Value, string Title);

}
