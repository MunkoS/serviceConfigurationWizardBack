using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ConfigurationWizard.models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServicesName
    {
        [Description("MirJournalService")]
        MirJournalService,
        [Description("DAService")]
        DAService
    }

}
