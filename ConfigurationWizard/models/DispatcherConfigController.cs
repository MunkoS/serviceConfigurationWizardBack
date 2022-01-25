namespace ConfigurationWizard.models
{
    public record Customer(string Name);

    public record ConnectionStrings(string ScadaDb);

    public record Auth(bool enable);

    public record Kestrel(EndPoints EndPoints);

    public record EndPoints(HttpInfo Http);
    public record HttpInfo(string Url);

    public class DispatherInfo
    {
        public Customer Customer { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public Auth Auth { get; set; }

        public Kestrel Kestrel { get; set; }
    }

}
