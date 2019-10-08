namespace Unidevel.Extensions.Networking
{
    public interface IJwtCodecService<T>: IJwtEncoder<T>, IJwtDecoder<T> where T : JwtTokenBase
    {
    }
}
