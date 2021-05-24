using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security;
using Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.Dto;
using Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.Interface;
using Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.OpenIdentity;
using Newtonsoft.Json;

namespace Arxivar.LogonProvider.OpenIdentity
{
    /// <summary>
    /// Logon Provider che utilizza il flusso OpenID Connect Hybrid: code + token_id
    /// https://openid.net/specs/openid-connect-core-1_0.html
    /// https://www.oauth.com/oauth2-servers/server-side-apps/authorization-code/ 
    /// </summary>
    public class OpenIdentityProvider : Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.OpenIdentity.OpenIdentityLogonProvider
    {
        public const string LogonProviderId = "A8F667A07BB547FFB49AB55E2E83D54A";

        const string ProviderDescriptionParam = "ProviderDescription";
        const string ClientIdParam = "ClientId";
        const string ClientSecretParam = "ClientSecret";
        const string RedirectUrlParam = "RedirectUrl";
        const string ARXivarPortalUrlParam = "ARXivarPortalUrl";
        const string IdentityServerAuthorizeUrlParam = "IdentityServerAuthorizeUrl";
        const string IdentityServerTokenUrlParam = "IdentityServerTokenUrl";
        const string IdentityServerKeysDiscoveryUrlParam = "IdentityServerKeysDiscoveryUrl";
        const string UserInfoUrlParam = "UserInfoUrl";

        private readonly string _description;

        public OpenIdentityProvider(ILogonProviderConfigurationManager configurationManager) : base(configurationManager)
        {
            var config = this.GetProviderConfiguration();
            _description = config.FirstOrDefault(x => x.Name == "ProviderDescription")?.Value as string;
        }

        public override IEnumerable<IProviderConfigurationItemDto> GetDefaulConfiguration()
        {
            return new List<IProviderConfigurationItemDto>
            {
                new ProviderConfigurationItemDto {Id = "134D1522-ABF4-483A-A826-5F2531D65F08", Name = ProviderDescriptionParam, Value = "OpenID", Description = "Logon provider description", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "948ECDDA-E1D1-48BB-BB31-0DF7A7DCD6FA", Name = ClientIdParam, Value = "", Description = "Application Id", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "3A197338-C833-42E9-AEA1-84A3B5AACE22", Name = ClientSecretParam, Value = "", Description = "Application Secret", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "13572F1D-D48B-489E-B811-9C69B0BD2655", Name = RedirectUrlParam, Value = "https://localhost/ARXivarNextAuthentication/OAuth/OpenIdAuthorize", Description = "Redirect Url for IdentityServer", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "D1EFE1C4-28B8-4B2E-B65B-EF6BADF68B89", Name = ARXivarPortalUrlParam, Value = "https://localhost/ARXivarNextWebPortal", Description = "ARXivar Web Portal Url", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "9FE50051-C60B-4763-84C0-DD98CE152ED8", Name = IdentityServerAuthorizeUrlParam, Value = "https://localhost:8081/connect/authorize", Description = "IdentityServer Login Authorize Url", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "2183BBBE-760E-4581-B367-B14D54CD498A", Name = IdentityServerTokenUrlParam, Value = "https://localhost:8081/connect/token", Description = "IdentityServer Login Token Url", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "09A6512B-AC94-491E-98E4-D84F4BC86FF8", Name = IdentityServerKeysDiscoveryUrlParam, Value = "https://localhost:8081/.well-known/openid-configuration/jwks", Description = "IdentityServer Keys Discovery Url", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "C518A385-1614-43EA-BD35-392753A7156A", Name = UserInfoUrlParam, Value = "https://localhost:8081/connect/userinfo", Description = "User info Url", Category = "OpenID", Type = ConfigurationItemTypeEnum.String},
            };
        }

        /// <summary>
        /// Questo metodo base relativo al flusso Password Grant non verrà mai chiamato
        /// </summary>
        /// <param name="authenticationUserRequest"></param>
        /// <returns></returns>
        public override IAuthenticationUserResponseDto LogonUser(AuthenticationUserRequestDto authenticationUserRequest)
        {
            return new AuthenticationUserResponseDto
            {
                FailCode = "NotImplemented",
                FailMessage = "This is a ImplicitFlow logon Provider",
                LogonSucceeded = false
            };
        }

        public override ILogonProviderInfoDto GetInfo()
        {
            return new LogonProviderInfoDto
            {
                Description = _description ?? "OpenID",
                Id = LogonProviderId,
                Version = "1.0",
                IconB64 = IconB64
            };
        }

        /// <summary>
        /// 1) Prepara il redirect verso la pagina di Authorize di Identity Server
        /// </summary>
        /// <param name="redirectRequest"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override OpenIdRedirectResponse PrepareRedirect(OpenIdRedirectRequest redirectRequest)
        {
            OpenIdRedirectResponse response = new OpenIdRedirectResponse();

            try
            {
                Logger.Debug("OpenID Logon Provider PrepareRedirect");

                // Recupero parametri logon provider
                var logonProviderConfiguration = GetProviderConfiguration();

                // ClientId da passare a Identity server
                var clientId = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ClientIdParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", ClientIdParam));
                }

                // Indirizzo di ritorno con il code + id_token
                var redirectUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(RedirectUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", RedirectUrlParam));
                }

                // Indirizzo Identity Server authorize endpoint
                var identityServerAuthorizeUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(IdentityServerAuthorizeUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(identityServerAuthorizeUrl))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", IdentityServerAuthorizeUrlParam));
                }

                response.RedirectUrl = identityServerAuthorizeUrl;

                // Specifico i parametri che dovranno essere messi come query string nel redirect verso la pagina di authorize di Identity Server
                
                // https://openid.net/specs/openid-connect-core-1_0.html#code-id_tokenExample
                response.UrlQueryString.Add(new KeyValuePair<string, string>("response_type", "code id_token"));
                // https://openid.net/specs/oauth-v2-form-post-response-mode-1_0.html#FormPostResponseExample
                response.UrlQueryString.Add(new KeyValuePair<string, string>("response_mode", "form_post")); // Impongo che la risposta con code + id_token venga mandata in POST
                response.UrlQueryString.Add(new KeyValuePair<string, string>("client_id", clientId));
                // https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
                response.UrlQueryString.Add(new KeyValuePair<string, string>("redirect_uri", redirectUrl));
                // Scope OpenID: https://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims
                response.UrlQueryString.Add(new KeyValuePair<string, string>("scope", "openid profile email"));

                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("RedirectUrl: {0}", identityServerAuthorizeUrl);

                    foreach (var q in response.UrlQueryString)
                    {
                        Logger.DebugFormat("{0}: {1}", q.Key, q.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                response.Error = new OpenIdError
                {
                    ErrorCode = e.GetType().FullName,
                    ErrorDescription = e.Message,
                };
            }

            return response;
        }

        /// <summary>
        /// 2) Dopo che Identity Server ha autorizzato redirige il browser verso il servizio Auth di ARXivar con code + id_token
        /// Viene preparato il contesto autorizzativo che avverrà utilizzato nel metodo Authorize
        /// </summary>
        /// <param name="receiveTokenRequest"></param>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        public override OpenIdReceiveTokenResponse ReceiveToken(OpenIdReceiveTokenRequest receiveTokenRequest)
        {
            OpenIdReceiveTokenResponse response = new OpenIdReceiveTokenResponse
            {
                SessionState = receiveTokenRequest.SessionState,
            };

            try
            {
                Logger.Debug("OpenID Logon Provider ReceiveToken");

                var code = receiveTokenRequest.Code;
                var idToken = receiveTokenRequest.IdToken;

                // Recupero parametri logon provider
                var logonProviderConfiguration = GetProviderConfiguration();
                var identityServerKeysDiscoveryUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(IdentityServerKeysDiscoveryUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(identityServerKeysDiscoveryUrl))
                {
                    throw new System.Security.SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", IdentityServerKeysDiscoveryUrlParam));
                }

                // Decodifico il token JWT
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(idToken) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    throw new SecurityException("idToken is not a JWT token (null cast)");
                }

                if (jsonToken.ValidFrom > DateTime.UtcNow)
                {
                    throw new SecurityException(string.Format("JWT Token is not valid yet: {0}", jsonToken.ValidFrom.ToString("O")));
                }

                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    throw new SecurityException(string.Format("Expired JWT Token: {0}", jsonToken.ValidTo.ToString("O")));
                }

                // Verifica ClientId dell'applicazione
                var clientId = jsonToken.Payload.Aud.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new SecurityException(string.Format("Unable to determine the claim: {0}", "ClientId (aud)"));
                }

                // Email
                var email = jsonToken.Payload.Claims.FirstOrDefault(x => x.Type.Equals(TokenClaims.Email))?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new SecurityException(string.Format("Unable to determine the claim: {0}", TokenClaims.Email));
                }


                // PreferredUsername
                var preferreduserName = jsonToken.Payload.Claims.FirstOrDefault(x => x.Type.Equals(TokenClaims.PreferredUsername))?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new SecurityException(string.Format("Unable to determine the claim: {0}", TokenClaims.PreferredUsername));
                }

                // User name
                var userName = jsonToken.Payload.Claims.FirstOrDefault(x => x.Type.Equals(TokenClaims.Name))?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new SecurityException(string.Format("Unable to determine the claim: {0}", TokenClaims.Name));
                }

                // Issuer
                var issuer = jsonToken.Payload.Claims.FirstOrDefault(x => x.Type.Equals(TokenClaims.Issuer))?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new SecurityException(string.Format("Unable to determine the claim: {0}", TokenClaims.Issuer));
                }

                Logger.InfoFormat("OpenID Username: {0} Email: {1} ClientId: {2}  Issuer: {3} Code: {4}", email, userName, clientId, issuer, code);

                // Recupero la chiave utilizzata per firmare il token JWT
                if (!jsonToken.Header.Typ.Equals("JWT"))
                {
                    throw new SecurityException(string.Format("Token Type is not JWT: {0}", jsonToken.Header.Typ));
                }

                if (!jsonToken.Header.Alg.Equals("RS256"))
                {
                    throw new SecurityException(string.Format("Token algorithm not handled: {0}", jsonToken.Header.Alg));
                }

                var keyId = jsonToken.Header.Kid;
                if (string.IsNullOrWhiteSpace(keyId))
                {
                    throw new SecurityException("Unable to determine the signing key");
                }


                // Verifico che la firma digitale del token JWT sia coerente con la chiave pubblica RSA pubblicata
                Logger.DebugFormat("Verify JWT signature: {0} ...", identityServerKeysDiscoveryUrl);
                TokenJwtHelper.VerifyTokenJwtSignature(jsonToken, identityServerKeysDiscoveryUrl, Logger);
                Logger.DebugFormat("JWT signature OK");

                // Inserisco la lista dei parametri del giro code in session state protetto lato server
                response.SessionState.Add(new KeyValuePair<string, string>(TokenClaims.Code, code));
                response.SessionState.Add(new KeyValuePair<string, string>(TokenClaims.Issuer, issuer));
                response.SessionState.Add(new KeyValuePair<string, string>(TokenClaims.ClientId, clientId));
                response.SessionState.Add(new KeyValuePair<string, string>(TokenClaims.Email, email));
                response.SessionState.Add(new KeyValuePair<string, string>(TokenClaims.PreferredUsername, preferreduserName));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                response.Error = new OpenIdError
                {
                    ErrorCode = e.GetType().FullName,
                    ErrorDescription = e.ToString()
                };
            }

            return response;
        }


        /// <summary>
        /// 3) Viene validato l'indirizzo dove il browser farà redirect con bearer token finale di ARXivar
        /// Questo indirizzo dovrebbe essere sempre il portale di ARXivar
        /// </summary>
        /// <param name="redirectRequest"></param>
        /// <returns></returns>
        /// <exception cref="SecurityException"></exception>
        public override OpenIdCheckRedirectResponse ValidateRedirectUrl(OpenIdCheckRedirectRequest redirectRequest)
        {
            Logger.DebugFormat("OpenID Logon Provider ValidateRedirectUrl: {0}", redirectRequest.RedirectUrl);
            var response = new OpenIdCheckRedirectResponse();

            try
            {
                // Recupero parametri logon provider
                var logonProviderConfiguration = GetProviderConfiguration();
                var arxivarPortalUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ARXivarPortalUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(arxivarPortalUrl))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", ARXivarPortalUrlParam));
                }

                // Verifica indirizzo di redirect verso il portale di Arxivar
                var allowedRedirectUrlList = arxivarPortalUrl.Split(',').Select(x => x.Trim());

                // Mi aspetto che context.RedirectUri abbia come prefisso arxivarPortalUrl
                if (!allowedRedirectUrlList.Any(x => redirectRequest.RedirectUrl.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new SecurityException(string.Format("Redirect: {0} is not authorized.  Check logon parameter: {1}", redirectRequest.RedirectUrl, ARXivarPortalUrlParam));
                }

                Logger.DebugFormat("RedirectUrl: {0} VALIDATED", redirectRequest.RedirectUrl);

                response.Validated = true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                response.Validated = false;
                response.Error = new OpenIdError
                {
                    ErrorCode = e.GetType().FullName,
                    ErrorDescription = e.ToString()
                };
            }

            return response;
        }


        /// <summary>
        /// 4) Viene determinato il logonProvider username.
        /// Viene passato il contesto costruito nella metodo ReceiveToken (fase 2)
        /// </summary>
        /// <param name="authorizeRequest"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="Exception"></exception>
        public override OpenIdAuthorizeResponse Authorize(OpenIdAuthorizeRequest authorizeRequest)
        {
            Logger.Debug("OpenID Logon Provider Authorize");
            OpenIdAuthorizeResponse response = new OpenIdAuthorizeResponse();
            try
            {
                var sessionState = authorizeRequest.SessionState;

                if (!authorizeRequest.SessionState.Any())
                {
                    throw new ArgumentNullException("Void session state. Verify all url in the authorization flow are https");
                }

                // Mi prendo i valori di stato salvati alla fase 2
                var code = sessionState.FirstOrDefault(x => x.Key == TokenClaims.Code).Value;
                var authorizeClientId = sessionState.FirstOrDefault(x => x.Key == TokenClaims.ClientId).Value;
                var username = sessionState.FirstOrDefault(x => x.Key == TokenClaims.Name).Value;
                var email = sessionState.FirstOrDefault(x => x.Key == TokenClaims.Email).Value;
                var issuer = sessionState.FirstOrDefault(x => x.Key == TokenClaims.Issuer).Value;
                var preferredUsername = sessionState.FirstOrDefault(x => x.Key == TokenClaims.PreferredUsername).Value;


                Logger.InfoFormat("OpenID Authorize PreferredUserName: {0} UserName: {1} Email: {2} Issuer: {3} ClientId: {4} Code: {5}", preferredUsername, username, email, issuer, authorizeClientId, code);

                // Recupero parametri logon provider
                var logonProviderConfiguration = GetProviderConfiguration();

                var clientId = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ClientIdParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", clientId));
                }

                var clientSecret = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ClientSecretParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", ClientSecretParam));
                }

                var redirectUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(RedirectUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", redirectUrl));
                }

                var identityServerTokenUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(IdentityServerTokenUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(identityServerTokenUrl))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", IdentityServerTokenUrlParam));
                }


                var userInfoUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(UserInfoUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(userInfoUrl))
                {
                    throw new SecurityException(string.Format("Wrong logon provider configuration parameter: {0}", UserInfoUrlParam));
                }


                // Verifica azureClientId
                if (clientId != authorizeClientId)
                {
                    throw new SecurityException(string.Format("ClientId is not correct: {0}. Check logon parameter: {1}", authorizeClientId, ClientIdParam));
                }

                #region decodifica code -> bearer token

                string tokenBarer;

                try
                {
                    // Richiedo il BearetToken utilizzando il code ricevuto
                    // https://www.oauth.com/oauth2-servers/access-tokens/authorization-code-request/
                    using (var hcTokenBarer = new HttpClient())
                    {
                        var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("client_id", authorizeClientId),
                            new KeyValuePair<string, string>("client_secret", clientSecret),
                            new KeyValuePair<string, string>("code", code),
                            new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                        });

                        var tokenRequestUrl = identityServerTokenUrl;
                        Logger.DebugFormat("Requesting bearer token from code at: {0} ...", tokenRequestUrl);
                        var tokenBarerPostResult = hcTokenBarer.PostAsync(tokenRequestUrl, postContent).GetAwaiter().GetResult();
                        Logger.Debug("Bearer token response received");

                        if (!tokenBarerPostResult.IsSuccessStatusCode)
                        {
                            var errorMessage = string.Format("{0} ({1})", tokenBarerPostResult.ReasonPhrase, tokenBarerPostResult.StatusCode);
                            throw new Exception(string.Format("Barer token request error: {0}", errorMessage));
                        }

                        var tokenBarerPostResultString = tokenBarerPostResult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        Logger.Debug($"TokenBarer received: {tokenBarerPostResultString}");

                        var tokenResult = (AuthenticationResult) JsonConvert.DeserializeObject(tokenBarerPostResultString, typeof(AuthenticationResult));

                        if (tokenResult == null)
                        {
                            throw new SecurityException("Deserialization null barer token");
                        }

                        if (tokenResult.Error == null && tokenResult.ErrorDescription == null && tokenResult.AccessToken == null)
                        {
                            throw new SecurityException("Wrong logon provider configuration parameter: {0}");
                        }

                        if (!string.IsNullOrWhiteSpace(tokenResult.Error))
                        {
                            throw new SecurityException(string.Format("Getting bearer token from code failed: {0}", string.Format("{0} - {1}", tokenResult.Error, tokenResult.ErrorDescription)));
                        }

                        // Deserializzo il messaggio
                        tokenBarer = tokenResult.AccessToken;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Getting bearer token from code failed", e);
                    throw;
                }

                try
                {
                    // Verifica che il code sia corretto e mi permetta di ottenere un token barer

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenBarer);

                        // Utilizzo il bearer token per fare una chiamata autorizzata e recuperare altre informazioni necessarie
                        Logger.DebugFormat("Requesting user info at: {0} ...", userInfoUrl);
                        var userInfoResponse = httpClient.GetAsync(userInfoUrl).GetAwaiter().GetResult();
                        Logger.Debug("Graph user info response received");

                        if (!userInfoResponse.IsSuccessStatusCode)
                        {
                            throw new SecurityException(userInfoUrl + " error code: " + userInfoResponse.StatusCode.ToString());
                        }

                        string userInfoString = userInfoResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        // Mi aspetto un JSON:
                        /*
                        {
                            "name": "Alice Smith",
                            "preferred_username": "alice",
                            "given_name": "Alice",
                            "family_name": "Smith",
                            "email": "AliceSmith@email.com",
                            "email_verified": true,
                            "website": "http://alice.com",
                            "sub": "12345678"
                        }
                        */
                        
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.DebugFormat($"UserInfo result -> {userInfoString}");
                        }

                        // Attualmente queste info non vengono utilizzate
                        var userInfoDefinition = new
                        {
                            name = string.Empty,
                            preferred_username = string.Empty,
                            given_name = string.Empty,
                            family_name = string.Empty,
                            email = string.Empty,
                            email_verified = string.Empty,
                            website = string.Empty,
                            sub = string.Empty
                        };
                        
                        var userInfo = JsonConvert.DeserializeAnonymousType(userInfoString, userInfoDefinition);

                        // ... posso scrivere delle logiche sulla base dell'ogetto ottenuto con la chiamata autorizzata ...
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error calling " + userInfoUrl, e);
                    throw;
                }

                #endregion


                // L'identificativo unico in OpenID è il campo preferred_username del JWT -> lo restituisco per la verifica dell'associazione con un utente ARXivar che verrà autorizzato
                response.ProviderUserId = preferredUsername;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                response.Error = new OpenIdAuthorizeError
                {
                    ErrorCode = e.GetType().FullName,
                    ErrorDescription = e.ToString(),
                    ReturnHttpUnauthorized = false
                };
            }

            return response;
        }

        public string IconB64 = @"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
jwj6EJMUy5gTxTR8x9c9Amy9S7As/3N/jn61YClAQCCeVQzTJt4gnt60Dcb7xLxSllTic+Ixky5I
/Mh02eM3xiWXOZbJm9nMPDFPLJS6WO5ipWxqxFPEcVXTKZ/Leawy3mKsVetK+57shdGCvrLMdJox
pLCIJYgQIKOOCqqwkaBVJ8VChvaTPv5h1y+SSyZXBQo5FlCDBsn1g/3B726t4uSElxRNAqEXx/kY
AcK7QKvhON/HjtM6AYLPwJXe8deawMwn6Y2OFj8CBraBi+uOJu8BlzvA0JMhmZIrBWlyxSLwfkbf
lAcGb4HeNa+39j5OH4AsdZW+AQ4OgdESZa/7vDvS3du/Z9r9/QAgpHKGcK71TQAAD4tpVFh0WE1M
OmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6
cmVTek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1w
dGs9IlhNUCBDb3JlIDQuNC4wLUV4aXYyIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3
dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpEZXNjcmlwdGlvbiBy
ZGY6YWJvdXQ9IiIKICAgIHhtbG5zOmlwdGNFeHQ9Imh0dHA6Ly9pcHRjLm9yZy9zdGQvSXB0YzR4
bXBFeHQvMjAwOC0wMi0yOS8iCiAgICB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94
YXAvMS4wL21tLyIKICAgIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAv
c1R5cGUvUmVzb3VyY2VFdmVudCMiCiAgICB4bWxuczpwbHVzPSJodHRwOi8vbnMudXNlcGx1cy5v
cmcvbGRmL3htcC8xLjAvIgogICAgeG1sbnM6R0lNUD0iaHR0cDovL3d3dy5naW1wLm9yZy94bXAv
IgogICAgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIgogICAgeG1s
bnM6dGlmZj0iaHR0cDovL25zLmFkb2JlLmNvbS90aWZmLzEuMC8iCiAgICB4bWxuczp4bXA9Imh0
dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iCiAgIHhtcE1NOkRvY3VtZW50SUQ9ImdpbXA6ZG9j
aWQ6Z2ltcDo1MzFhNjkxZC1iMDI5LTRiNmMtYjEwYS1mMTk4OTQ2MDc1NzUiCiAgIHhtcE1NOklu
c3RhbmNlSUQ9InhtcC5paWQ6Y2VkZjZhNTctMjBlOS00YjYwLWFkMDktMzJkNzFjYTFhMDlkIgog
ICB4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6YjViOTMwNWQtMGI0Yy00ZTlhLWJk
NmMtY2EyMzI2YTZjODVjIgogICBHSU1QOkFQST0iMi4wIgogICBHSU1QOlBsYXRmb3JtPSJMaW51
eCIKICAgR0lNUDpUaW1lU3RhbXA9IjE2MjE0OTgzMjA0ODYxMzYiCiAgIEdJTVA6VmVyc2lvbj0i
Mi4xMC4yMiIKICAgZGM6Rm9ybWF0PSJpbWFnZS9wbmciCiAgIHRpZmY6T3JpZW50YXRpb249IjEi
CiAgIHhtcDpDcmVhdG9yVG9vbD0iR0lNUCAyLjEwIj4KICAgPGlwdGNFeHQ6TG9jYXRpb25DcmVh
dGVkPgogICAgPHJkZjpCYWcvPgogICA8L2lwdGNFeHQ6TG9jYXRpb25DcmVhdGVkPgogICA8aXB0
Y0V4dDpMb2NhdGlvblNob3duPgogICAgPHJkZjpCYWcvPgogICA8L2lwdGNFeHQ6TG9jYXRpb25T
aG93bj4KICAgPGlwdGNFeHQ6QXJ0d29ya09yT2JqZWN0PgogICAgPHJkZjpCYWcvPgogICA8L2lw
dGNFeHQ6QXJ0d29ya09yT2JqZWN0PgogICA8aXB0Y0V4dDpSZWdpc3RyeUlkPgogICAgPHJkZjpC
YWcvPgogICA8L2lwdGNFeHQ6UmVnaXN0cnlJZD4KICAgPHhtcE1NOkhpc3Rvcnk+CiAgICA8cmRm
OlNlcT4KICAgICA8cmRmOmxpCiAgICAgIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiCiAgICAgIHN0RXZ0
OmNoYW5nZWQ9Ii8iCiAgICAgIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6ZWUwMGRlZmEtOTJh
NC00YmZlLTkyMGEtYTg0OWU1MzY1ZWE3IgogICAgICBzdEV2dDpzb2Z0d2FyZUFnZW50PSJHaW1w
IDIuMTAgKExpbnV4KSIKICAgICAgc3RFdnQ6d2hlbj0iKzAyOjAwIi8+CiAgICA8L3JkZjpTZXE+
CiAgIDwveG1wTU06SGlzdG9yeT4KICAgPHBsdXM6SW1hZ2VTdXBwbGllcj4KICAgIDxyZGY6U2Vx
Lz4KICAgPC9wbHVzOkltYWdlU3VwcGxpZXI+CiAgIDxwbHVzOkltYWdlQ3JlYXRvcj4KICAgIDxy
ZGY6U2VxLz4KICAgPC9wbHVzOkltYWdlQ3JlYXRvcj4KICAgPHBsdXM6Q29weXJpZ2h0T3duZXI+
CiAgICA8cmRmOlNlcS8+CiAgIDwvcGx1czpDb3B5cmlnaHRPd25lcj4KICAgPHBsdXM6TGljZW5z
b3I+CiAgICA8cmRmOlNlcS8+CiAgIDwvcGx1czpMaWNlbnNvcj4KICA8L3JkZjpEZXNjcmlwdGlv
bj4KIDwvcmRmOlJERj4KPC94OnhtcG1ldGE+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
IAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAog
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAg
ICAgICAgICAgICAgICAgICAgICAgICAKPD94cGFja2V0IGVuZD0idyI/Pubvxr8AAAAGYktHRAD/
AP8A/6C9p5MAAAAJcEhZcwAACxMAAAsTAQCanBgAAAAHdElNRQflBRQIDAA/YHGdAAADiElEQVRY
w+2XX2hbZRTAf1/TtOm0jbGatpGykpvsQRoaLCTDRZS+dJTWtmJwVGoho/OhbkikglQh6ZyOYqgd
Yq3Ul/ogKBqhoFiU/glOcLONacmg9CKphRBaQTQPmTfm+tDbUuemaRO7lx74Hr577/ed3z3nfOec
D46kOPIEcBUYPWzFjwJz+hLU9od1KvDhfjcoKUB5uETw7Xtnqh7/6dJDnHFVHmiT0gIAnoy8ZOaR
44aCTFiIBagoEwX7sORuR+8RQL5BKIBTQAa4/h/fHgcuAD8Dy8ACoBzUAieAt4EUEAHezQP2tNVq
9Xd1dY3q9fqvtbWjQK323gS8Djj/zQIW4DLQ6/V6cbvdpNNpAoFAXi5rbGzE5/PR29tLIpEwzc7O
vjg9PX0O+Ah4SoPYAqK3A3ga+KC7u7uqra2NmpoaAOLxeL4uU9fX14nH49hstt3R0tJybGFh4azb
7SYejzM1NSXdbsPzQogrwWCQpqYmhBCk02nm5+eZmJgAuJEHwHQ0GrVHo9FnJUmq6+vrw+l0IkkS
kiQBsLW1BVB5K8AzOp3uSigUwmq1ApBIJBgbG2NtbW0eeBn4Pg+AJPAWcL8sy75kMonT6fx7Aquo
ALDtBXgAeD8YDO4q39jYwO/3oyjKEPDGPk/Xa4Bv549XVlYwm81UV1ej0+kwGo0A9+wFON/a2lrl
cDgAUBSFyclJFEW5eADlAC8AE7Isu8fHx93ASaCxtLSUjo6OHQvY9wJ0eTwehNjO7bIss7i4mAIu
FZBjlrUxqc0rs9msKxwO7wCZ9gI4zGbz7spUKgXwFXCziEnvd+AbbfwzEamqeldrwY+bm5u7D2tr
awFagfLDAvg8EomQy+UAsFqtNDc312jRfCgA78zMzPwai8UA0Ov19Pf3YzAYhoBXDwPgF+BcIBBA
luXtYmCxEAqFsNvtF7VCdPL/7gc+yeVyA36/n6WlJVRVpb6+nuHhYQYGBjxCiO+AH4AhwFMsAN0t
82vA8tzc3OlMJmOwWCyYTCZsNhvt7e04HI46o9HYsrq66gPE84/dy4OV2yf5RvImn0UzMSC8H4A7
dZV1wJvAc16vV7hcLhoaGigv3z4U2WyWnp4ervqN3HdMx5fLaS58/Bt/qlwGXimmi2xaM5ESQqid
nZ3q4OCgOjIyopaVlak9zXoVUIEviumWO8kpzf+fAjHgD63BcB7dTAuRvwBusA+DR7DP7gAAAABJ
RU5ErkJggg==";
    }
}