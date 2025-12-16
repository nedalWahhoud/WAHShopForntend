using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class ActivateRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsLoading { get; set; } = true;
        [JsonIgnore]
        public string StatusMessage { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsActivated { get; set; } = false;
    }
}
