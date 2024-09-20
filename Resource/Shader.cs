using RenderWrapper.Vulkan;
using Silk.NET.Vulkan;

namespace RenderWrapper.Resource;

public sealed class Shader {
    public readonly ShaderModule NativeShader;

    public Shader(byte[] code, VulkanContext ctx) : this(code, ctx.Vk, ctx.Device) {}

    public Shader(byte[] code, Vk vk, VulkanDevice device) {
        ShaderModuleCreateInfo createInfo = new() {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint)code.Length,
        };

        unsafe {
            fixed (byte* codePtr = code) {
                createInfo.PCode = (uint*)codePtr;

                if (vk!.CreateShaderModule(device.LogicalDevice, in createInfo, null, out NativeShader) != Result.Success)
                    throw new Exception("Unable to create shader!");
            }
        }
    }
}

public sealed record SpecializedShader(Shader Shader, ShaderStage Stage, string Entrypoint = "main");

public enum ShaderStage {
    Compute,
    Geometry,
    Vertex,
    Fragment,
    Task,
    Mesh
}

public static class ShaderStageExtensions {
    public static ShaderStageFlags ToFlags(this ShaderStage stage)
        => stage switch {
            ShaderStage.Compute => ShaderStageFlags.ComputeBit,
            ShaderStage.Geometry => ShaderStageFlags.GeometryBit,
            ShaderStage.Vertex => ShaderStageFlags.VertexBit,
            ShaderStage.Fragment => ShaderStageFlags.FragmentBit,
            ShaderStage.Task => ShaderStageFlags.TaskBitExt,
            ShaderStage.Mesh => ShaderStageFlags.MeshBitExt,
            _ => 0
        };
}
