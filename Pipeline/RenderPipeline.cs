using RenderWrapper.Resource;

namespace RenderWrapper.Pipeline;

public struct RenderPipeline() {
    public Topology topology;
    public VertexDescription[] Vertices;
    public SpecializedShader[] Shaders;
}
