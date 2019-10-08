namespace Unidevel.Extensions.Networking
{
    /// <summary>
    /// Provides coding of JWT Tokens. 
    /// </summary>
    /// <typeparam name="T">Token type. Must inherit JwtTokenBase.</typeparam>
    public interface IJwtEncoder<T> where T : JwtTokenBase
    {
        string Encode(T payload);
    }
}
