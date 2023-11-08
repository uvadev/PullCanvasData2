using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace PullCanvasData2; 

[PublicAPI]
public class CanvasDataAuthenticationException : CanvasDataException {
    public CanvasDataAuthenticationException() { }
    public CanvasDataAuthenticationException(string message) : base(message) { }
    public CanvasDataAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    protected CanvasDataAuthenticationException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}
