using System;
using Pipopolam.Net.Http.Logging;

namespace Pipopolam.Net.Http;

public record WebServiceOptions(
        TimeSpan? DefaultTimeout = null,
        IWebServiceLogger? Logger = null
    );
