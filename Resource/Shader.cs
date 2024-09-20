using RenderWrapper.Vulkan;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace RenderWrapper.Resource;

public sealed class Shader : IDisposable {
    public readonly ShaderModule NativeShader;
    private readonly Vk Vk;
    private readonly VulkanDevice Device;

    public Shader(byte[] code, VulkanContext ctx) : this(code, ctx.Vk, ctx.Device) {}

    public Shader(byte[] code, Vk vk, VulkanDevice device) {
        Vk = vk;
        Device = device;

        ShaderModuleCreateInfo createInfo = new() {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint)code.Length,
        };

        unsafe {
            fixed (byte* codePtr = code) {
                createInfo.PCode = (uint*)codePtr;

                if (Vk.CreateShaderModule(Device.LogicalDevice, in createInfo, null, out NativeShader) != Result.Success)
                    throw new Exception("Unable to create shader!");
            }
        }
    }

    public void Dispose() {
        unsafe {
            Vk.DestroyShaderModule(Device.LogicalDevice, NativeShader, null);
        }
    }
}

public sealed record SpecializedShader(Shader Shader, ShaderStage Stage, string Entrypoint = "main") : IDisposable {
    private PipelineShaderStageCreateInfo? info = null;

    public PipelineShaderStageCreateInfo GetCreateInfo() {
        if (info == null) {
            unsafe {
                info = new() {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = Stage.ToFlags(),
                    Module = Shader.NativeShader,
                    PName = (byte*)SilkMarshal.StringToPtr(Entrypoint)
                };
            }
        }

        return info!.Value;
    }

    public void Dispose() {
        if (info == null)
            return;
        
        unsafe {
            SilkMarshal.FreeString((nint)info!.Value.PName);
        }
    }
}

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
