using System;

namespace Hestify.Helpers
{
    internal static class Checks
    {
        public static void IsNotNull<T>(T value, string name = "parameter")
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }
    }
}