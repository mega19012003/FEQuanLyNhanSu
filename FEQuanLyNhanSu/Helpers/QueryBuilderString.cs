using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.Helpers
{
    public static class QueryStringBuilder
    {
        public static string Build(IDictionary<string, object> parameters)
        {
            if (parameters == null || !parameters.Any())
                return "";

            var queryParams = parameters
                .Where(kv => kv.Value != null && !IsEmptyString(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value.ToString())}");

            return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        }

        private static bool IsEmptyString(object value)
        {
            return value is string str && string.IsNullOrWhiteSpace(str);
        }
    }

}
