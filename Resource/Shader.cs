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

public sealed class SpecializedShader(Shader Shader, ShaderStage Stage, string Entrypoint = "main") : IDisposable {
    public readonly Shader Shader = Shader;
    public readonly ShaderStage Stage = Stage;
    public readonly string Entrypoint = Entrypoint;

    private unsafe PipelineShaderStageCreateInfo *info;

    public void PopulateCreateInfo() {
        unsafe {
            if (info != null)
                return;
            info = Mem.Alloc<PipelineShaderStageCreateInfo>();
            *info = new() {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = Stage.ToFlags(),
                Module = Shader.NativeShader,
                PName = (byte*)SilkMarshal.StringToPtr(Entrypoint)
            };
        }
    }

    public void Dispose() {
        unsafe {
            if (info == null)
                return;
            
            SilkMarshal.FreeString((nint)info->PName);
            Mem.Free(info);
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
