using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Test.Shared.Extensions
{
    public static class AssertE
    {
        public static T IsAssignableFrom<T>(object obj)
        {
            try
            {
                return (T) obj;
            }
            catch (InvalidCastException)
            {
                Assert.Fail($"{obj.GetType().Name} can not assign to {typeof(T).Name}");
                return (T)(object)null;
            }
        }
        
        public static void Ex<TException>(ActualValueDelegate<object> avd) where TException : Exception
        {
            Assert.That(avd, Throws.TypeOf<TException>());
        }
    }
}