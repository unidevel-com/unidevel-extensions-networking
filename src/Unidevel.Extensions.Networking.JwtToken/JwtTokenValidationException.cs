using System.Security;

namespace Unidevel.Extensions.Networking
{
    public class JwtTokenValidationException : SecurityException
    {
        public JwtTokenValidationException(string message) : base(message)
        {
        }
    }
}
