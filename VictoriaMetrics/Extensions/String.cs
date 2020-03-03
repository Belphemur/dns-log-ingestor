using System.Linq;

namespace VictoriaMetrics.Extensions
{
    public static class String
    {
        /// <summary>
        /// Also known as snake_case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUnderscoreCase(this string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}