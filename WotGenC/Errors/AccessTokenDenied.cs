using System;

namespace WotGenC.Errors
{
    public class AccessTokenDenied : Exception
    {
        public override string Message { get; } = "Access token is invalid";
    }
}