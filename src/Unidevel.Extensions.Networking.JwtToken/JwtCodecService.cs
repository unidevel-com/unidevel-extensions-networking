using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Unidevel.Extensions.Networking
{
    public class JwtCodecService<T> : IJwtCodecService<T> where T: JwtTokenBase
    {
        private static readonly IDictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;

        static JwtCodecService()
        {
            HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
            {
                { JwtHashAlgorithm.HS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } }
            };
        }

        private Dictionary<string, object> _extraHeaders;
        private byte[] _key;
        private JwtHashAlgorithm _algorithm;

        public JwtCodecService(byte[] key, JwtHashAlgorithm algorithm = JwtHashAlgorithm.HS256, Dictionary<string, object> extraHeaders = null)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _algorithm = algorithm;
            _extraHeaders = extraHeaders ?? new Dictionary<string, object>(0);
        }

        public JwtCodecService(string key, JwtHashAlgorithm algorithm = JwtHashAlgorithm.HS256, Dictionary<string, object> extraHeaders = null) :
            this(Encoding.UTF8.GetBytes(key), algorithm, extraHeaders)
        {
        }

        public string Encode(T payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var header = new Dictionary<string, object>(_extraHeaders)
            {
                { "typ", "JWT" },
                { "alg", _algorithm.ToString() }
            };

            var headerString = base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
            var payloadString = base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
            var stringToSign = String.Concat(headerString, ".", payloadString);

            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
            var signature = HashAlgorithms[_algorithm](_key, bytesToSign);
            var signatureString = base64UrlEncode(signature);

            return String.Concat(stringToSign, ".", signatureString);
        }

        public T Decode(string token)
        {
            if (String.IsNullOrWhiteSpace(token)) throw new ArgumentException("Must not be null, empty or whitespace.", nameof(token));

            var parts = token.Split('.');
            if (parts.Length != 3) throw new ArgumentException("Token must consist from 3 delimited by dot parts", nameof(token));

            var header = parts[0];
            var payload = parts[1];
            var crypto = base64UrlDecode(parts[2]);

            var headerJson = Encoding.UTF8.GetString(base64UrlDecode(header));
            var payloadJson = Encoding.UTF8.GetString(base64UrlDecode(payload));

            var headerData = JsonConvert.DeserializeObject<Dictionary<string, object>>(headerJson);

            var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));
            var alg = (string)headerData["alg"];

            JwtHashAlgorithm algorithm;
            if (!Enum.TryParse(alg, out algorithm)) throw new ArgumentException($"Unknown algorithm '{alg}' used.", nameof(token));

            var signature = HashAlgorithms[algorithm](_key, bytesToSign);
            var decodedCrypto = Convert.ToBase64String(crypto);
            var decodedSignature = Convert.ToBase64String(signature);

            if (decodedCrypto != decodedSignature)
            {
#if DEBUG
                throw new JwtTokenValidationException($"Invalid signature. Expected '{decodedCrypto}' got '{decodedSignature}'.");
#else
                throw new JwtTokenValidationException($"Invalid signature. Got '{decodedSignature}' but different expected.");
#endif
            }

            T payloadData = JsonConvert.DeserializeObject<T>(payloadJson);

            if ((payloadData.ExpirationTime.HasValue) && (payloadData.ExpirationTime < DateTimeOffset.UtcNow)) throw new JwtTokenValidationException($"Token has expired.");
            if ((payloadData.NotBefore.HasValue) && (payloadData.NotBefore > DateTimeOffset.UtcNow)) throw new JwtTokenValidationException($"Token is not yet valid.");

            return payloadData;
        }

        private static string base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);

            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding

            return output;
        }

        private static byte[] base64UrlDecode(string input)
        {
            var output = input;

            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding

            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break;  // One pad char
                default: throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(output); // Standard base64 decoder
        }
    }
}
