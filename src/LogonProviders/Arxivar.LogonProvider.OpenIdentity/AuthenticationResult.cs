using System;
using System.Runtime.Serialization;
namespace Arxivar.LogonProvider.OpenIdentity
{
    /// <summary>
    /// AuthenticationResult
    /// </summary>
    [DataContract]
    public class AuthenticationResult
    {
        /// <summary>
        /// Error
        /// </summary>
        [DataMember(Name = "error", EmitDefaultValue = false)]
        public string Error { get; set; }

        /// <summary>
        /// ErrorDescription
        /// </summary>
        [DataMember(Name = "error_description", EmitDefaultValue = false)]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// ErrorUri
        /// </summary>
        [DataMember(Name = "error_uri", EmitDefaultValue = false)]
        public string ErrorUri { get; set; }

        /// <summary>
        /// AccessToken
        /// </summary>
        [DataMember(Name = "access_token", EmitDefaultValue = false)]
        public string AccessToken { get; set; }

        /// <summary>
        /// TokenType
        /// </summary>
        [DataMember(Name = "token_type", EmitDefaultValue = false)]
        public string TokenType { get; set; }

        /// <summary>
        /// ExpiresIn
        /// </summary>
        [DataMember(Name = "expires_in", EmitDefaultValue = false)]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        [DataMember(Name = "refresh_token", EmitDefaultValue = false)]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Issued
        /// </summary>
        [DataMember(Name = ".issued", EmitDefaultValue = false)]
        public DateTime Issued { get; set; }

        /// <summary>
        /// Expires
        /// </summary>
        [DataMember(Name = ".expires", EmitDefaultValue = false)]
        public DateTime Expires { get; set; }
    }
}