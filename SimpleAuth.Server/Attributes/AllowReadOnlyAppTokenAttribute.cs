using System;

namespace SimpleAuth.Server.Attributes
{
    /// <summary>
    /// For overriding RequireAppToken, allowing ReadOnly records
    /// </summary>
    public class AllowReadOnlyAppTokenAttribute : Attribute
    {
    }
}