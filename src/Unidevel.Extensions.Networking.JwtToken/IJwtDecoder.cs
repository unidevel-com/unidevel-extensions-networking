namespace Unidevel.Extensions.Networking
{
    /// <summary>
    /// Provides decoding of JWT Tokens. 
    /// </summary>
    /// <typeparam name="T">Token type. Must inherit JwtTokenBase.</typeparam>
    public interface IJwtDecoder<T> where T : JwtTokenBase
    {
        T Decode(string jwtToken);
    }
}
