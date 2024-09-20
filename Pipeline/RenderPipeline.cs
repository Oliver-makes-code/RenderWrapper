using RenderWrapper.Resource;
using Silk.NET.Vulkan;

namespace RenderWrapper.Pipeline;

public sealed class RenderPipeline(
    Topology Topology,
    SpecializedShader[] Shaders
) : IDisposable {
    public readonly Topology Topology = Topology;
    public readonly SpecializedShader[] Shaders = Shaders;

    private unsafe PipelineVertexInputStateCreateInfo *vertexInput;
    private unsafe PipelineInputAssemblyStateCreateInfo *inputAssembly;

    public unsafe PipelineVertexInputStateCreateInfo *GetVertexInput() {
        if (vertexInput == null) {
            vertexInput = Mem.Alloc<PipelineVertexInputStateCreateInfo>();

            *vertexInput = new() {
                SType = StructureType.PipelineVertexInputDivisorStateCreateInfoKhr,
                VertexAttributeDescriptionCount = 0,
                VertexBindingDescriptionCount = 0
            };
        }

        return vertexInput;
    }

    public unsafe PipelineInputAssemblyStateCreateInfo *GetInputAssembly() {
        if (inputAssembly == null) {
            inputAssembly = Mem.Alloc<PipelineInputAssemblyStateCreateInfo>();

            *inputAssembly = new() {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = Topology.ToPrimitiveTopology(),
                PrimitiveRestartEnable = false,
            };
        }

        return inputAssembly;
    }

    public void Dispose() {
        unsafe {
            if (vertexInput != null) {
                Mem.Free(vertexInput);
            }

            if (inputAssembly != null) {
                Mem.Free(inputAssembly);
            }
        }
    }
}
