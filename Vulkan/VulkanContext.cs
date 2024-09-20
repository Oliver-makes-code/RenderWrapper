using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace RenderWrapper.Vulkan;

public sealed class VulkanContext {
    public const string EngineName = "Foxel Engine";
    
    public readonly Vk Vk;
    public readonly Instance VulkanInstance;
    public readonly IVkSurface VkSurface;
    public readonly VulkanDevice Device;
    public readonly VulkanQueues Queues;
    public readonly VulkanSurface Surface;
    public readonly VulkanSwapchain Swapchain;

    public VulkanContext(IWindow window, string appName) {
        Vk = Vk.GetApi();

        VkSurface = window.VkSurface!;

        VulkanInstance = CreateInstance(appName, Vk, VkSurface);

        Surface = VulkanSurface.Create(Vk, VkSurface, VulkanInstance);
        
        Device = VulkanDevice.Create(Vk, VulkanInstance, Surface);

        Queues = Device.GetQueues(Vk, Surface);

        Swapchain = VulkanSwapchain.Create(Vk, VulkanInstance, Device, Surface, window);
    }

    private static Instance CreateInstance(string appName, Vk vk, IVkSurface vkSurface) {
        unsafe {
            ApplicationInfo appInfo = new() {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(appName),
                ApplicationVersion = new Version32(0, 1, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Foxel Engine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version11
            };

            InstanceCreateInfo createInfo = new() {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            var extensions = vkSurface.GetRequiredExtensions(out var extensionCount);

            createInfo.EnabledExtensionCount = extensionCount;
            createInfo.PpEnabledExtensionNames = extensions;
            createInfo.EnabledLayerCount = 0;

            if (vk.CreateInstance(ref createInfo, null, out var vulkanInstance) != Result.Success)
                throw new Exception("Failed to create instance");

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);

            return vulkanInstance;
        }
    }
}
