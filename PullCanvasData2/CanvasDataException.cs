using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace PullCanvasData2;

public class CanvasDataException : Exception {
    public CanvasDataException() { }
    public CanvasDataException(string message) : base(message) { }
    public CanvasDataException(string message, Exception innerException) : base(message, innerException) { }
    protected CanvasDataException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}
