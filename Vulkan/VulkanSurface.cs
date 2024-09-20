using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace RenderWrapper.Vulkan;

public sealed record VulkanSurface(KhrSurface KhrSurfaceExt, SurfaceKHR SurfaceKhr) {
    internal static VulkanSurface Create(Vk vk, IVkSurface vkSurface, Instance vulkanInstance) {
        if (!vk.TryGetInstanceExtension<KhrSurface>(vulkanInstance, out var khrSurface))
            throw new NotSupportedException("KHR_surface extension not found.");

        unsafe {
            return new(khrSurface, vkSurface.Create<AllocationCallbacks>(vulkanInstance.ToHandle(), null).ToSurface());
        }
    }
}
