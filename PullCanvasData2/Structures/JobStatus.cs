using JetBrains.Annotations;

namespace PullCanvasData2.Structures {
    [PublicAPI]
    public enum JobStatus {
        Waiting,
        Running,
        Complete,
        Failed
    }
}
