using System;

namespace SimpleAuth.Shared.Enums
{
    [Flags]
    public enum Verb : byte
    {
        None = 0,
        Add = 1,
        View = 2,
        Edit = 4,
        Delete = 8,
        Crud = Add | View | Edit | Delete,
        CurrentMax = Crud,
        Full = 255
    }

    public static class PermissionExtensions
    {
        public static Verb Grant(this Verb verb, params Verb[] verbs)
        {
            var result = verb;
            foreach (var p in verbs)
                result |= p;
            return result;
        }

        public static Verb Revoke(this Verb verb, params Verb[] revokeVerbs)
        {
            var result = verb;
            foreach (var p in revokeVerbs)
            {
                if (p == Verb.None)
                    continue;
                if (result == Verb.Full)
                    result = Verb.CurrentMax;
                result &= ~p;
            }

            return result;
        }

        public static string Serialize(this Verb verb)
        {
            if (verb == Verb.Full)
                return Constants.WildCard;
            return ((byte) verb).ToString();
        }

        public static Verb Deserialize(this string verb)
        {
            if (Constants.WildCard.Equals(verb))
                return Verb.Full;
            return (Verb) byte.Parse(verb);
        }
    }
}