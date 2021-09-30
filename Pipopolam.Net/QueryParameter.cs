using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipopolam.Net
{
    public class QueryParameter
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public QueryParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
