// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("webapi"),
                new ApiScope("webdemo"),
                new ApiScope("scope1"),
                new ApiScope("scope2"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "client-hybrid",
                    ClientName = "ARXivar OpenID",
                    ClientSecrets = {new Secret("EA4A6E59-4350-4423-B971-859C287D8F11".Sha256())},
                    RequirePkce = false,
                    AllowPlainTextPkce = false,
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.Hybrid,  // code + token_id
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = {"https://localhost/ARXivarNextAuthentication/OAuth/OpenIdAuthorize"},
                    FrontChannelLogoutUri = "https://localhost/ARXivarNextWebPortal",
                    PostLogoutRedirectUris = {"https://localhost/ARXivarNextWebPortal"},
                    RequireConsent = true,
                    AllowedScopes = {IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile,IdentityServerConstants.StandardScopes.Email , "webdemo"},
                },
                new Client
                {
                    ClientId = "client-password",
                    ClientName = "ARXivar Password Provider",
                    ClientSecrets = {new Secret("6FCA53C1-8D1D-4694-9EDD-37B0364F7472".Sha256())},
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = {IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile,IdentityServerConstants.StandardScopes.Email , "webdemo"},
                },
                new Client
                {
                    ClientId = "client-hybrid-mvc",
                    ClientName = "Resource server MVC ",
                    ClientSecrets = {new Secret("2C59A271-87C0-4B7D-8583-9B7CA7954E6F".Sha256())},
                    RequirePkce = false,
                    AllowPlainTextPkce = false,
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = {"https://localhost:44392/signin-oidc"},
                    FrontChannelLogoutUri = "https://localhost:44392/signout-oidc",
                    PostLogoutRedirectUris = {"https://localhost:44392/signout-callback-oidc"},
                    RequireConsent = true,
                    AllowedScopes = {IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "scope1", "scope2", "webapi", IdentityServerConstants.StandardScopes.Email, "webdemo"},
                },
                #region Development
                new Client
                {
                    ClientId = "client-hybrid-development",
                    ClientName = "ARXivar OpenID Development",
                    ClientSecrets = {new Secret("AEE7C064-46CF-45CA-A0EF-C2A875B35EF2".Sha256())},
                    RequirePkce = false,
                    AllowPlainTextPkce = false,
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.Hybrid,  // code + token_id
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = {"https://localhost/ARXivarAuthorizationServer/OAuth/OpenIdAuthorize"},
                    FrontChannelLogoutUri = "https://localhost/Abletech.Arxivar.Client.Web.Portal",
                    PostLogoutRedirectUris = {"https://localhost/Abletech.Arxivar.Client.Web.Portal"},
                    RequireConsent = true,
                    AllowedScopes = {IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile,IdentityServerConstants.StandardScopes.Email , "webdemo"},
                },
                #endregion
            };
    }
}