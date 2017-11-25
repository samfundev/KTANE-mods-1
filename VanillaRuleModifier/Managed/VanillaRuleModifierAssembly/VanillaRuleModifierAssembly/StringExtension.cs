using System.Linq;

namespace VanillaRuleModifierAssembly
{
    public static class StringExtension
    {
        public static string Capitalize(this string data)
        {
            return char.ToUpperInvariant(data.First()) + data.Substring(1).ToLowerInvariant();
        }
    }
}