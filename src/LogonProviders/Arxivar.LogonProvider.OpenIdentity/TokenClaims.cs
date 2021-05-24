namespace Arxivar.LogonProvider.OpenIdentity
{
    public class TokenClaims
    {
        // Parametri di discover del servizio
        // https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration

        // https://docs.microsoft.com/it-it/azure/active-directory/develop/id-tokens
        // Payload
        public const string Code = "code"; 
        public const string Issuer = "iss";
        public const string Nonce = "nonce";
        public const string ClientId = "aud";
        public const string Email = "email";
        public const string Name = "name";
        public const string PreferredUsername = "preferred_username";

        //Header
        public const string HeaderAlgorithm = "alg";
        public const string HeaderTokenType = "typ";
        public const string HeaderSignKeyId = "kid";
    }
}