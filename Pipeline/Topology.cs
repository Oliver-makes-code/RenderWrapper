using Silk.NET.Vulkan;

namespace RenderWrapper.Pipeline;

public enum Topology {
    PointList = PrimitiveTopology.PointList,
    LineList = PrimitiveTopology.LineList,
    LineStrip = PrimitiveTopology.LineStrip,
    TriangleList = PrimitiveTopology.TriangleList,
    TriangleStrip = PrimitiveTopology.TriangleStrip,
    TriangleFan = PrimitiveTopology.TriangleFan,
    LineListWithAdjacency = PrimitiveTopology.LineListWithAdjacency,
    LineStripWithAdjacency = PrimitiveTopology.LineStripWithAdjacency,
    TriangleListWithAdjacency = PrimitiveTopology.TriangleListWithAdjacency,
    TriangleStripWithAdjacency = PrimitiveTopology.TriangleStripWithAdjacency,
    PatchList = PrimitiveTopology.PatchList,
}

public static class TopologyExtensions {
    public static PrimitiveTopology ToPrimitiveTopology(this Topology topology)
        => (PrimitiveTopology) topology;
}
