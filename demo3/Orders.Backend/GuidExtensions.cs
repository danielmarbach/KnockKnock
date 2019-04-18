using System;

namespace Orders.Backend
{
    public static class GuidExtensions
    {
        public static string Short(this Guid guid)
        {
            var guidAsString = guid.ToString();
            return guidAsString.Substring(guidAsString.Length - 7, 7);
        }
    }
}