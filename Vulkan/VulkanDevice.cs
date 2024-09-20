using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace RenderWrapper.Vulkan;

public sealed record VulkanQueues(Queue Graphics, Queue Present);

public sealed record VulkanDevice(PhysicalDevice PhysicalDevice, Device LogicalDevice, VulkanDevice.QueueFamilies Families, ExtMeshShader MeshShaderExt) {
    private static readonly string[] DeviceExtensions = [
        KhrSwapchain.ExtensionName,
        ExtMeshShader.ExtensionName
    ];

    public VulkanQueues GetQueues(Vk vk, VulkanSurface surface) {
        vk.GetDeviceQueue(LogicalDevice, Families.Graphics!.Value, 0, out var graphicsQueue);
        vk.GetDeviceQueue(LogicalDevice, Families.Graphics!.Value, 1, out var presentQueue);
        return new(graphicsQueue, presentQueue);
    }

    private static PhysicalDevice PickPhysicalDevice(Vk vk, Instance vulkanInstance, VulkanSurface surface) {
        var devices = vk.GetPhysicalDevices(vulkanInstance);

        PhysicalDevice physicalDevice = new();

        foreach (var device in devices) {
            if (IsDeviceSuitable(device, vk, surface)) {
                physicalDevice = device;
                break;
            }
        }

        if (physicalDevice.Handle == 0)
            throw new Exception("Failed to find a suitable GPU");

        return physicalDevice;
    }

    internal static VulkanDevice Create(Vk vk, Instance vulkanInstance, VulkanSurface surface) {
        var physicalDevice = PickPhysicalDevice(vk, vulkanInstance, surface);

        unsafe {
            var indices = FindQueueFamilies(physicalDevice, vk, surface);

            var uniqueQueueFamilies = new[] { indices.Graphics!.Value, indices.Present!.Value };
            uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

            using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            float queuePriority = 1.0f;
            for (int i = 0; i < uniqueQueueFamilies.Length; i++)
                queueCreateInfos[i] = new() {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueQueueFamilies[i],
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority
                };

            PhysicalDeviceFeatures deviceFeatures = new();

            DeviceCreateInfo createInfo = new() {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = queueCreateInfos,

                PEnabledFeatures = &deviceFeatures,

                EnabledExtensionCount = (uint)DeviceExtensions.Length,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(DeviceExtensions),
                EnabledLayerCount = 0
            };

            if (vk.CreateDevice(physicalDevice, in createInfo, null, out var logicalDevice) != Result.Success)
                throw new Exception("Failed to create logical device");

            if (!vk.TryGetDeviceExtension<ExtMeshShader>(vulkanInstance, logicalDevice, out var meshShaderExt))
                throw new NotSupportedException("EXT_mesh_shader extension not found.");

            return new(physicalDevice, logicalDevice, indices, meshShaderExt);
        }
    }

    private static bool IsDeviceSuitable(PhysicalDevice device, Vk vk, VulkanSurface surface) {
        var families = FindQueueFamilies(device, vk, surface);

        if (!CheckDeviceExtensionsSupport(device, vk))
            return false;

        var swapChainSupport = VulkanSwapchain.QuerySwapchainSupport(device, surface);
        bool swapChainAdequate = swapChainSupport.Formats.Length != 0 && swapChainSupport.PresentModes.Length != 0;

        return families.HasAll() && swapChainAdequate;
    }

    private static unsafe QueueFamilies FindQueueFamilies(PhysicalDevice device, Vk vk, VulkanSurface surface) {
        QueueFamilies value = new();

        uint queueFamilityCount = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies) {
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
        }


        uint i = 0;
        foreach (var queueFamily in queueFamilies) {
            if (!queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit)) {
                i++;
                continue;
            }

            value.Graphics = i;

            surface.KhrSurfaceExt.GetPhysicalDeviceSurfaceSupport(device, i, surface.SurfaceKhr, out var presentSupport);

            if (!presentSupport) {
                i++;
                continue;
            }

            value.Present = i;

            break;
        }

        return value;
    }

    private static bool CheckDeviceExtensionsSupport(PhysicalDevice device, Vk vk) {
        unsafe {
            uint extentionsCount = 0;
            vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, null);

            var availableExtensions = new ExtensionProperties[extentionsCount];
            fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
            {
                vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, availableExtensionsPtr);
            }

            var availableExtensionNames = availableExtensions.Select(extension => Marshal.PtrToStringAnsi((IntPtr)extension.ExtensionName)).ToHashSet();

            return DeviceExtensions.All(availableExtensionNames.Contains);
        }
    }

    public struct QueueFamilies {
        public uint? Present;
        public uint? Graphics;

        public readonly bool HasAll()
            => Present != null && Graphics != null;
    }
}
