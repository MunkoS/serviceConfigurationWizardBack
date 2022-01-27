using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ConfigurationWizard.models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServicesName
    {
        [Description("Mir.Journal.Service")]
        MirJournalService,
        [Description("DAService")]
        DAService,
        [Description("Energy")]
        Energy,
        [Description("Mir.Scada.Dispatcher.Api")]
        Dispatcher,
        [Description("Mir.Scada.Editor.Api")]
        Editor
    }

}
