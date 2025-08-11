using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Pipopolam.Net.Http;

public record Request(Uri Uri,
    Dictionary<string, string> Headers,
    HttpContent Content);
