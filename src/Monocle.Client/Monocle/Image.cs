using Newtonsoft.Json;

namespace Monocle
{
    public class Image
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
