using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipopolam.Net
{
    public class QueryParameter
    {
        public string Key { get; }
        public string Value { get; }

        public QueryParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
