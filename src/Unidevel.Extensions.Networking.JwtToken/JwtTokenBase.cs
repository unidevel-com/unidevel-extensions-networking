using System;
using Newtonsoft.Json;

namespace Unidevel.Extensions.Networking
{
    /// <summary>
    /// Base class for all JwtTokens. Inherit and include your own properties. Use JsonProperty attribute
    /// to set short names when possible.
    /// </summary>
    public class JwtTokenBase
    {
        [JsonProperty("exp", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(JwtUnixTimestampJsonConverter))]
        public DateTimeOffset? ExpirationTime { get; set; }
        [JsonProperty("nbf", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(JwtUnixTimestampJsonConverter))]
        public DateTimeOffset? NotBefore { get; set; }
        [JsonProperty("iat", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(JwtUnixTimestampJsonConverter))]
        public DateTimeOffset? IssuedAt { get; set; }
        [JsonProperty("iss", NullValueHandling = NullValueHandling.Ignore)]
        public string Issuer { get; set; }
        [JsonProperty("aud", NullValueHandling = NullValueHandling.Ignore)]
        public string Audience { get; set; }
        [JsonProperty("prn", NullValueHandling = NullValueHandling.Ignore)]
        public string Principal { get; set; }
        [JsonProperty("jti", NullValueHandling = NullValueHandling.Ignore)]
        public string JwtId { get; set; }
        [JsonProperty("typ", NullValueHandling = NullValueHandling.Ignore)]
        public string Typ { get; set; }
    }
}
