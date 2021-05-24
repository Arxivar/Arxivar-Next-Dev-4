using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using log4net;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Newtonsoft.Json;

namespace Arxivar.LogonProvider.OpenIdentity
{
    public class TokenJwtHelper
    {
        internal static void VerifyTokenJwtSignature(JwtSecurityToken tokenJwt, string pkiUri, ILog logger)
        {
            TokenKeys tokenKeys;
            try
            {
                TokenKey signingKey = null;

                string responseAsString;
                using (var hc = new HttpClient())
                {
                    HttpResponseMessage responseContent;

                    try
                    {
                        logger.DebugFormat("Sign key loading from: {0}", pkiUri);
                        responseContent = hc.GetAsync(pkiUri).Result;
                    }
                    catch (Exception e)
                    {
                        var errorMessage = string.Format("Verify JWT token error calling {0} -> ", pkiUri) + e.Message;
                        logger.Error(errorMessage);
                        throw new HttpException(errorMessage);
                    }

                    if (!responseContent.IsSuccessStatusCode)
                    {
                        throw new SecurityException(pkiUri + " error code: " + responseContent.StatusCode.ToString());
                    }

                    responseAsString = responseContent.Content.ReadAsStringAsync().Result;
                }

                tokenKeys = JsonConvert.DeserializeObject<TokenKeys>(responseAsString);
                if (tokenKeys.TokenKeyList != null)
                {
                    signingKey = tokenKeys?.TokenKeyList.FirstOrDefault(x => x.KeyId.Equals(tokenJwt.Header.Kid));
                    logger.DebugFormat("Sign key found: {0}", signingKey.KeyId);
                }


                if (signingKey == null)
                {
                    // Chiave non trovata!
                    throw new SecurityException("Token sign key not found: " + tokenJwt.Header.Kid);
                }

                if (!signingKey.KeyType.Equals("RSA"))
                {
                    throw new SecurityException("Token sign key is not RSA: " + signingKey.KeyType);
                }

                if (!signingKey.Use.Equals("sig"))
                {
                    throw new SecurityException("Token sign key use is not sig: " + signingKey.Use);
                }

                var jwtSignatureValid = TokenJwtHelper.VerifyTokenJwtSignatureInternal(tokenJwt.RawData, signingKey.Modolus, signingKey.PublicExponent);

                if (!jwtSignatureValid)
                {
                    throw new SecurityException("Token JWT signature is not valid, kid: " + signingKey.KeyId);
                }
            }
            catch (Exception e)
            {
                logger.Error("VerifyTokenJwtSignature error", e);
                throw;
            }
        }

        /// <summary>
        /// Verify a JWT token signed in RS256 (RSA + SHA-256)
        /// </summary>
        /// <param name="tokenJwtEncoded"></param>
        /// <returns></returns>
        private static bool VerifyTokenJwtSignatureInternal(string tokenJwtEncoded, string modulusEncoded, string exponentEncoded)
        {
            var urlTextEncoder = new Base64UrlTextEncoder();
            string[] tokenParts = tokenJwtEncoded.Split('.');

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = urlTextEncoder.Decode(modulusEncoded),
                    Exponent = urlTextEncoder.Decode(exponentEncoded)
                });

            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + '.' + tokenParts[1]));
            }

            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");

            var signatureVerified = rsaDeformatter.VerifySignature(hash, urlTextEncoder.Decode(tokenParts[2]));

            return signatureVerified;
        }
    }

    public class TokenKeys
    {
        [JsonProperty("keys")] public TokenKey[] TokenKeyList { get; set; }
    }

    /// <summary>
    /// Oggetto che descrive la chiave pubblica RSA utilizzata per firmare il token JWT
    /// </summary>
    public class TokenKey
    {
        [JsonProperty("kty")] public string KeyType { get; set; }

        [JsonProperty("use")] public string Use { get; set; }

        [JsonProperty("kid")] public string KeyId { get; set; }

        [JsonProperty("x5t")] public string Thumbprint { get; set; }

        [JsonProperty("n")] public string Modolus { get; set; }

        [JsonProperty("e")] public string PublicExponent { get; set; }

        [JsonProperty("x5c")] public string[] CertificateList { get; set; }

        [JsonProperty("issuer")] public string Issuer { get; set; }
    }
}