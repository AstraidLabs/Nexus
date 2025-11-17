using System;

namespace Nexus;

/// <summary>
/// Výjimka sjednocující selhání při práci s <see cref="NexusAPI"/>.
/// </summary>
public sealed class NexusApiException : Exception
{
    public NexusApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
