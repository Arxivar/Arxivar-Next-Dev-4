using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.Dto;
using Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.Interface;
using IdentityModel.Client;
using Newtonsoft.Json;

namespace Arxivar.LogonProvider.Credentials
{
    /// <summary>
    /// Logon Provider che valida le credenziali passate da ARXivar.
    /// Si appoggia a Identity Server mediante il flusso Open Outh2 Password Grant https://www.oauth.com/oauth2-servers/access-tokens/password-grant/  
    /// </summary>
    public class CredentialsProvider : Abletech.Arxivar.Server.Auth.LogonProvider.Contracts.LogonProviderBase
    {
        private readonly string _description;

        // LogonProvider Id
        public const string LogonProviderId = "3E00BFC06B514FADA869382811917EE8";

        // Costanti nome parametri
        const string ProviderDescriptionParam = "ProviderDescription";
        const string ClientIdParam = "ClientId";
        const string ClientSecretParam = "ClientSecret";
        const string IdentityServerTokenUrlParam = "IdentityServerTokenUrl";


        public CredentialsProvider(ILogonProviderConfigurationManager configurationManager) : base(configurationManager)
        {
            var config = this.GetProviderConfiguration();
            _description = config.FirstOrDefault(x => x.Name == "ProviderDescription")?.Value as string;
        }

        public override IEnumerable<IProviderConfigurationItemDto> GetDefaulConfiguration()
        {
            return new List<IProviderConfigurationItemDto>
            {
                new ProviderConfigurationItemDto {Id = "1FD4935A-52D4-4F44-899F-3ED89D8EF3CD", Name = ProviderDescriptionParam, Value = "Credentials", Description = "Logon provider description", Category = "Logon", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "B705CA4D-669C-44A5-867E-C518D63D250A", Name = ClientIdParam, Value = "", Description = "Application Id", Category = "Logon", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "C6A9AA92-118B-4221-B913-6F33BCC2E617", Name = ClientSecretParam, Value = "", Description = "Application Secret", Category = "Logon", Type = ConfigurationItemTypeEnum.String},
                new ProviderConfigurationItemDto {Id = "8D62F8DC-3C43-409E-BC33-8C7273D9150A", Name = IdentityServerTokenUrlParam, Value = "https://localhost:8081/connect/token", Description = "IdentityServer Login Token Url", Category = "Logon", Type = ConfigurationItemTypeEnum.String},
            };
        }

        public override IAuthenticationUserResponseDto LogonUser(AuthenticationUserRequestDto authenticationUserRequest)
        {
            IAuthenticationUserResponseDto response = new AuthenticationUserResponseDto();

            try
            {
                if (authenticationUserRequest == null) throw new ArgumentNullException(nameof(authenticationUserRequest));

                if (string.IsNullOrWhiteSpace(authenticationUserRequest.Username))
                    throw new ArgumentNullException(nameof(authenticationUserRequest.Username));

                if (string.IsNullOrWhiteSpace(authenticationUserRequest.Password))
                    throw new ArgumentNullException(nameof(authenticationUserRequest.Password));

                // Validazione parametri
                Logger.Debug($"Logon Provider {_description} -> LogonUser: {authenticationUserRequest.Username} Password: {authenticationUserRequest.Password?.Any()} ClientId: {authenticationUserRequest.ClientId} ClientId: {authenticationUserRequest.ClientId} ClientVersion: {authenticationUserRequest.ClientVersion} MachineKey: {authenticationUserRequest.MachineKey} ClientIpAddress: {authenticationUserRequest.ClientIpAddress}");

                // Recupero parametri logon provider
                var logonProviderConfiguration = GetProviderConfiguration();

                var tokenUrl = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(IdentityServerTokenUrlParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(tokenUrl))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", IdentityServerTokenUrlParam));
                }

                var clientId = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ClientIdParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", ClientIdParam));
                }

                var secret = logonProviderConfiguration.FirstOrDefault(x => x.Name.Equals(ClientSecretParam))?.Value as string;
                if (string.IsNullOrWhiteSpace(secret))
                {
                    throw new Exception(string.Format("Wrong logon provider configuration parameter: {0}", ClientSecretParam));
                }

                // https://identitymodel.readthedocs.io/en/latest/client/token.html
                // http://www.bubblecode.net/en/2016/01/22/understanding-oauth2/#Resource_Owner_Password_Credentials_Grant

                using (var client = new HttpClient())
                {
                    Task.Run(() =>
                        {
                            var authenticationResultTask = client.RequestPasswordTokenAsync(new PasswordTokenRequest
                            {
                                Address = tokenUrl,
                                ClientSecret = secret,
                                ClientId = clientId,
                                Scope = "openid profile email", // basterebbe openid, aggiungo anche le info del profilo per poter capire l'identità di chi si sta loggando
                                UserName = authenticationUserRequest.Username,
                                Password = authenticationUserRequest.Password,
                                Parameters = new Dictionary<string, string>
                                {
                                    // Posso passare dei parametri ulteriori a Identity Server 
                                    {"ClientVersion", authenticationUserRequest.ClientVersion},
                                    {"ClientIpAddress", authenticationUserRequest.ClientIpAddress},
                                    {"LanguageCultureName", authenticationUserRequest.LanguageCultureName},
                                }
                            });

                            return authenticationResultTask;
                        }).ContinueWith(task =>
                        {
                            var authenticationResult = task.Result;

                            if (authenticationResult.IsError)
                            {
                                // Loggo tutto l'oggetto
                                Logger.Error(JsonConvert.SerializeObject(authenticationResult));

                                response.LogonSucceeded = false;
                                response.FailCode = "AuthenticationFailed";
                                response.FailMessage = "Wrong username or password";
                            }
                            else
                            {
                                response.LogonSucceeded = true;
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion)
                        .Wait();
                }
            }
            // Eccezione sollevata dal Task
            catch (AggregateException aggregatedException)
            {
                Logger.Error($"{MethodBase.GetCurrentMethod().Name} Logon provider {_description}", aggregatedException.InnerException);
                response.LogonSucceeded = false;
                response.FailCode = "AuthenticationFailed";
                response.FailMessage = aggregatedException.InnerException?.ToString();
            }
            catch (Exception e)
            {
                Logger.Error($"{MethodBase.GetCurrentMethod().Name} Logon provider {_description}", e);
                response.LogonSucceeded = false;
                response.FailCode = "AuthenticationFailed";
                response.FailMessage = e.Message;
            }

            return response;
        }

        public override ILogonProviderInfoDto GetInfo()
        {
            return new LogonProviderInfoDto
            {
                Id = LogonProviderId,
                Description = "Credentials",
                IconB64 =
                    @"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
PvoFTRqSFBdHwbXg4Mdi1cHFWVcHV0EQ/ABxc3NSdJES/5cUWsR4cNyPd/ced+8Arl5WNKtjHNB0
20wl4kImuyqEXxECjx6Mok9SLGNOFJPwHV/3CLD1Lsay/M/9ObrVnKUAAYF4VjFMm3iDeHrTNhjv
E/NKUVKJz4nHTLog8SPTZY/fGBdc5lgmb6ZT88Q8sVBoY7mNlaKpEU8RR1VNp3wu47HKeIuxVq4q
zXuyF0Zy+soy02kOIYFFLEGEABlVlFCGjRitOikWUrQf9/EPun6RXDK5SlDIsYAKNEiuH+wPfndr
5ScnvKRIHAi9OM7HMBDeBRo1x/k+dpzGCRB8Bq70lr9SB2Y+Sa+1tOgR0LsNXFy3NHkPuNwBBp4M
yZRcKUiTy+eB9zP6pizQfwt0rXm9Nfdx+gCkqavkDXBwCIwUKHvd592d7b39e6bZ3w9mqHKiK1uQ
awAAD4tpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0i
VzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6
bWV0YS8iIHg6eG1wdGs9IlhNUCBDb3JlIDQuNC4wLUV4aXYyIj4KIDxyZGY6UkRGIHhtbG5zOnJk
Zj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpE
ZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOmlwdGNFeHQ9Imh0dHA6Ly9pcHRjLm9y
Zy9zdGQvSXB0YzR4bXBFeHQvMjAwOC0wMi0yOS8iCiAgICB4bWxuczp4bXBNTT0iaHR0cDovL25z
LmFkb2JlLmNvbS94YXAvMS4wL21tLyIKICAgIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUu
Y29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiCiAgICB4bWxuczpwbHVzPSJodHRwOi8v
bnMudXNlcGx1cy5vcmcvbGRmL3htcC8xLjAvIgogICAgeG1sbnM6R0lNUD0iaHR0cDovL3d3dy5n
aW1wLm9yZy94bXAvIgogICAgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8x
LjEvIgogICAgeG1sbnM6dGlmZj0iaHR0cDovL25zLmFkb2JlLmNvbS90aWZmLzEuMC8iCiAgICB4
bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iCiAgIHhtcE1NOkRvY3VtZW50
SUQ9ImdpbXA6ZG9jaWQ6Z2ltcDpiNzU3NTU0NC1iY2YwLTQ3N2EtYjAzMC05ZTk4YmYwMmM4MTEi
CiAgIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MTg1NDQwYmItOTI2OS00MzVjLWJkYTQtZjli
ZTI5ZDA2YzI1IgogICB4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6YWUwZTc0MDct
MWQzYy00NWI0LWFiNGEtMmZjZWEwMDU1MGUzIgogICBHSU1QOkFQST0iMi4wIgogICBHSU1QOlBs
YXRmb3JtPSJMaW51eCIKICAgR0lNUDpUaW1lU3RhbXA9IjE2MjE1MjE4MjM3MDE3MjEiCiAgIEdJ
TVA6VmVyc2lvbj0iMi4xMC4yMiIKICAgZGM6Rm9ybWF0PSJpbWFnZS9wbmciCiAgIHRpZmY6T3Jp
ZW50YXRpb249IjEiCiAgIHhtcDpDcmVhdG9yVG9vbD0iR0lNUCAyLjEwIj4KICAgPGlwdGNFeHQ6
TG9jYXRpb25DcmVhdGVkPgogICAgPHJkZjpCYWcvPgogICA8L2lwdGNFeHQ6TG9jYXRpb25DcmVh
dGVkPgogICA8aXB0Y0V4dDpMb2NhdGlvblNob3duPgogICAgPHJkZjpCYWcvPgogICA8L2lwdGNF
eHQ6TG9jYXRpb25TaG93bj4KICAgPGlwdGNFeHQ6QXJ0d29ya09yT2JqZWN0PgogICAgPHJkZjpC
YWcvPgogICA8L2lwdGNFeHQ6QXJ0d29ya09yT2JqZWN0PgogICA8aXB0Y0V4dDpSZWdpc3RyeUlk
PgogICAgPHJkZjpCYWcvPgogICA8L2lwdGNFeHQ6UmVnaXN0cnlJZD4KICAgPHhtcE1NOkhpc3Rv
cnk+CiAgICA8cmRmOlNlcT4KICAgICA8cmRmOmxpCiAgICAgIHN0RXZ0OmFjdGlvbj0ic2F2ZWQi
CiAgICAgIHN0RXZ0OmNoYW5nZWQ9Ii8iCiAgICAgIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6
OTdiOWMzOGUtMzU1ZS00YWZhLWEzMzEtZmI0MDU3MjBmOWIyIgogICAgICBzdEV2dDpzb2Z0d2Fy
ZUFnZW50PSJHaW1wIDIuMTAgKExpbnV4KSIKICAgICAgc3RFdnQ6d2hlbj0iKzAyOjAwIi8+CiAg
ICA8L3JkZjpTZXE+CiAgIDwveG1wTU06SGlzdG9yeT4KICAgPHBsdXM6SW1hZ2VTdXBwbGllcj4K
ICAgIDxyZGY6U2VxLz4KICAgPC9wbHVzOkltYWdlU3VwcGxpZXI+CiAgIDxwbHVzOkltYWdlQ3Jl
YXRvcj4KICAgIDxyZGY6U2VxLz4KICAgPC9wbHVzOkltYWdlQ3JlYXRvcj4KICAgPHBsdXM6Q29w
eXJpZ2h0T3duZXI+CiAgICA8cmRmOlNlcS8+CiAgIDwvcGx1czpDb3B5cmlnaHRPd25lcj4KICAg
PHBsdXM6TGljZW5zb3I+CiAgICA8cmRmOlNlcS8+CiAgIDwvcGx1czpMaWNlbnNvcj4KICA8L3Jk
ZjpEZXNjcmlwdGlvbj4KIDwvcmRmOlJERj4KPC94OnhtcG1ldGE+CiAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg
ICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAKPD94cGFja2V0IGVuZD0idyI/Prhh
QasAAAAJcEhZcwAALiMAAC4jAXilP3YAAAAHdElNRQflBRQOKytNlEYKAAAFb0lEQVRIx51WaYwU
RRT+qqqrZ3p6mAEGFpZhuWRZYlh/eKAocofg+gOFRCMYJCgY/4kQRUUUFIyERIx/NIAxWVTOmLAm
BAgJLBBYDgMaghzRZVn3vmaX6e7p6q7yx+wxM0wYsNKZrq56733zvvfq1SNSSgBCCMuyAIRCIc45
gEQiAUDXdcMwANi27bougGg0+lDyGu47lFLwpdfYQm7c0uobaNIShuGHTRIbQocPI8XD88hnT0gq
lUp/e54HQNM0QggAT3iyvZOeOscOHOZVV0ifwoAtAjGnDMsXs9nTZWyw8DwoUEoYYwB835dSEkJI
Z2cnAM55KBQCYFmWEIKm3MDpC3T9du1GJwoNL26KDauCSxYRI3gvdXkAZH0T//LbQOXpTCti6mj1
1itqwhiIWlrXiN3V/FRjpoC7ZBrb8pEcEcsFyKGI1jWwt9fxM7czlVPbVuorlpKICUhlXQZ8WMLd
c0Jf/SvJYM57Mi5/2EpKx2dRlJlFrLk9sPL9XOtblgfee8d17D+rfqs9cozIRMmsxx978TndDNq7
joTWHMqi64lR5MB3rKTYtixXCAADAE5nQv/gi0DlmSyFcRFac1CGAofeXdu24+f+9SGvzVz49RoC
3Kn4eMJlO4urV5/mO7bZUEK4AGgikUgkEp4QgVPnc6wDUG8upLHB109UZ1oH0PnLyWvHL2jhYNOy
mTdIMnNL31vjVO7TuRaNRqPRKE2vyvYuun57ngx5ZByAhouX7t2qv3AVQKSkqNpvvJ6DsXEHWjvS
c6rrOuecVp/TbubJSKUxAJ7j5MG2XSgwjUmg2m/M9IO1OPLoCSGEEIIahhEKBNnBw/h/Q/X+5GBg
1/5UV7dlWRSA19iiVV25V1ESlX4DSvU9GHgkIRJEpj/TfvRzxc/eVvWNAEgymSRnLwXnr85lYNNY
tiiOoAHD9Hq6lO+mg0Xa90DZAAgLMbNYunel0/LHycCl9QoABWaw4knKBOBUfcXmPa+5rqvVN+Tx
3aAkSoEUZIqb/WXRV45DVDokDvwOxsBMsKAGUABpP8CKJymTtrVzzikAmrQehnVy/+3+mFPbBqBF
o1FhGA9iuKfD+v34teYWb9ZMvWi4WxBDWneLEwkKwA+bD5ItR3edvH7sTM1xd9f3nu9pBf1oD3IA
VAhBYkML2pfC7/jnVnre+q90UoX/UiRerOs6tSzLLRqqSAFmqc6mvDArPZ82l5umLAgwsmyyYRgU
ABlZJOZMKqjwTEW5WTTSCNOXFlKgAIA+tTw2dgwAGgqFApFBWL64IIBICau92bkrLUsVFC5dttTx
Pdu2Keecc85mT/fiZvb1navT1XZX+UoBrW0qb8EYGFFz4rw5QgjXdfuqaWyw2LAqS+jvJFRWYCIx
8+VPX1/9SWjkiBx+SOJOVlKVb90UGR3vDV76PhBCBJcsdpc+O1ARf2rx67KKaNDUh8ej8bgKh/2s
89Fq3tw7sBKumFlWsUDjPOs+ACHECLDNH3pP9SH3QH12NVXr3P9UdzeHj37DZHOvr2xSyYzNn7OM
g0WSyaRSilKa7mfUzVq6Yq12sbc6JQP+lTcGkfEhQknvAeo+D/hp3hMNrG6/r3pov/X5u38c9uhk
ldFfkXtbR9rU5q/brO+t6cUg/hHZ1Kbs+6dNuGLWjM0bY6UTjYz+Kj8A51xZTqpyH9+4k7XYhTGi
ZvnWTWUVC1jYzGng8lDk+76UCkTpGkdrhzx2Ejv38bO3rXwY+tTyicuWls6bGxk9SuM8bwuav3XM
7KJTXd2qvpE3tdht7X853a5lacFgZFTxiMmTh40pSfm+my2f09lp/UmUM+kfvq5hQgmbUjaE82l9
bTrX9VC6yNsFYvMflJsCQd13P2UAAAAASUVORK5CYII=",
                Version = "1.0"
            };
        }
    }
}