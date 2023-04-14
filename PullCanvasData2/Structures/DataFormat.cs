using JetBrains.Annotations;

namespace PullCanvasData2.Structures {
    [PublicAPI]
    public enum DataFormat {
        Tsv,
        Csv,
        JsonLines,
        Parquet
    }
}
