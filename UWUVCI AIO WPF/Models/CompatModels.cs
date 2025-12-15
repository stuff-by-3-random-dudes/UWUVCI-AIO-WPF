using Newtonsoft.Json;
using System.Collections.Generic;

namespace UWUVCI_AIO_WPF.Models
{
    public class CompatFile<T> where T : GameCompatEntry
    {
        [JsonProperty("compatibility")]
        public List<T> Compatibility { get; set; } = new List<T>();
    }

    public class GameCompatEntry
    {
        [JsonProperty("game_name")]
        public string GameName { get; set; }

        [JsonProperty("game_region")]
        public string GameRegion { get; set; }

        [JsonProperty("base_name")]
        public string BaseName { get; set; }

        [JsonProperty("base_region")]
        public string BaseRegion { get; set; }

        /// <summary>0 = Doesn't Work, 1 = Issues, 2 = Works</summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }

    public class WiiCompatEntry : GameCompatEntry
    {
        /// <summary>0 = Doesn't Work, 1 = Works, 2 = Issues</summary>
        [JsonProperty("gamepad")]
        public int Gamepad { get; set; }
    }

    public class NdsCompatEntry : GameCompatEntry
    {
        /// <summary>Examples: "1x", "2x", free text</summary>
        [JsonProperty("rendersize")]
        public string RenderSize { get; set; }
    }

    public enum CompatStatus
    {
        DoesNotWork = 0,
        Issues = 1,
        Works = 2
    }
}
