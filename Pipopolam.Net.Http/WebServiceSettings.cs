using System;

namespace Pipopolam.Net.Http;

public record WebServiceSettings(
        Uri BaseUri,
        TimeSpan? DefaultTimeout = null,
        bool IsLoggingEnabled = false
    );
