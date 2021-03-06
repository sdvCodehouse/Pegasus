﻿using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Pegasus.Library.JwtAuthentication.Models
{
    /// <summary>
    ///     A structure containing the various options required
    ///     to generate a valid Json Web Token
    /// </summary>
    public sealed class TokenOptions
    {
        private const int DefaultExpiryTimeInMinutes = 5;
        /// <summary>
        ///     Creates a new instance of <see cref="TokenOptions" />
        /// </summary>
        /// <param name="issuer">
        ///     Required. Issuer of the token, usually,
        ///     your web application URL but could be any string
        /// </param>
        /// <param name="audience">
        ///     Required. Audience of the token i.e. who the token is for.
        ///     Could be any string
        /// </param>
        /// <param name="signingKey">
        ///     Required. An instance of <see cref="SecurityKey" /> containing
        ///     the encoded 128-bit string.
        ///     Any string that is sufficiently long and unguessable will do.
        /// </param>
        /// <param name="tokenExpiryInMinutes">
        ///     Defaults to 5 mins
        ///     but can be longer or shorter.
        /// </param>
        public TokenOptions(string issuer, string audience, string signingKey, int tokenExpiryInMinutes = DefaultExpiryTimeInMinutes)
        {
            if (string.IsNullOrWhiteSpace(audience))
                throw new ArgumentNullException($"{nameof(Audience)} is mandatory in order to generate a JWT!");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentNullException($"{nameof(Issuer)} is mandatory in order to generate a JWT!");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentNullException($"{nameof(SigningKey)} is mandatory in order to generate a JWT!");

            Audience = audience;
            Issuer = issuer;
            SigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey));
            TokenExpiryInMinutes = tokenExpiryInMinutes;
        }

        public TokenOptions(string issuer, string audience, string signingKey, string tokenExpiryInMinutes) 
            : this(issuer, audience, signingKey)
        {
            if (!string.IsNullOrWhiteSpace(tokenExpiryInMinutes) && int.TryParse(tokenExpiryInMinutes, out var expiryTime))
            {
                TokenExpiryInMinutes = expiryTime;
            }
        }

        public SecurityKey SigningKey { get; }

        public string Issuer { get; }

        public string Audience { get; }

        public int TokenExpiryInMinutes { get; }
    }

    public struct TokenConstants
    {
        public const string TokenName = "access_token";
    }
}