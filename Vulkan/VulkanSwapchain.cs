using RenderWrapper.Resource;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace RenderWrapper.Vulkan;

public sealed record VulkanSwapchain(
    KhrSwapchain KhrSwapchainExt,
    SwapchainKHR SwapchainKhr,
    DeviceImage.View[] SwapchainImages,
    Extent2D SwapchainExtent,
    SurfaceCapabilitiesKHR Capabilities
) {
    public static VulkanSwapchain Create(Vk vk, Instance vulkanInstance, VulkanDevice device, VulkanSurface surface, IWindow nativeWindow) {
        unsafe {
            var swapchainSupport = QuerySwapchainSupport(device.PhysicalDevice, surface);

            var surfaceFormat = ChooseSwapSurfaceFormat(swapchainSupport.Formats);
            var presentMode = ChoosePresentMode(swapchainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapchainSupport.Capabilities, nativeWindow);

            var imageCount = swapchainSupport.Capabilities.MinImageCount + 1;
            if (swapchainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapchainSupport.Capabilities.MaxImageCount)
                imageCount = swapchainSupport.Capabilities.MaxImageCount;

            SwapchainCreateInfoKHR createInfo = new() {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = surface.SurfaceKhr,

                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            var indices = device.Families;
            var queueFamilyIndices = stackalloc[] { indices.Graphics!.Value, indices.Present!.Value };

            if (indices.Graphics != indices.Present) {
                createInfo = createInfo with {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            } else {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            createInfo = createInfo with {
                PreTransform = swapchainSupport.Capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,

                OldSwapchain = default
            };

            if (!vk.TryGetDeviceExtension<KhrSwapchain>(vulkanInstance, device.LogicalDevice, out var khrSwapchainExt))
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");

            if (khrSwapchainExt.CreateSwapchain(device.LogicalDevice, in createInfo, null, out var swapchainKhr) != Result.Success)
                throw new Exception("Failed to create swapchain.");

            khrSwapchainExt.GetSwapchainImages(device.LogicalDevice, swapchainKhr, ref imageCount, null);
            var swapchainImages = new Image[imageCount];
            fixed (Image* swapchainImagesPtr = swapchainImages) {
                khrSwapchainExt.GetSwapchainImages(device.LogicalDevice, swapchainKhr, ref imageCount, swapchainImagesPtr);
            }

            var swapchainImageFormat = surfaceFormat.Format;
            var swapchainExtent = extent;

            var deviceImages = new DeviceImage.View[imageCount];
            for (int i = 0; i < imageCount; i++)
                deviceImages[i] = new(new(swapchainImages[i], swapchainImageFormat), vk, device);

            return new(khrSwapchainExt, swapchainKhr, deviceImages, swapchainExtent, swapchainSupport.Capabilities);
        }
    }

    private static SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats) {
        foreach (var availableFormat in availableFormats) {
            if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr) {
                return availableFormat;
            }
        }

        return availableFormats[0];
    }

    private static PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes) {
        foreach (var availablePresentMode in availablePresentModes) {
            if (availablePresentMode == PresentModeKHR.MailboxKhr) {
                return availablePresentMode;
            }
        }

        return PresentModeKHR.FifoKhr;
    }

    private static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities, IWindow nativeWindow) {
        if (capabilities.CurrentExtent.Width != uint.MaxValue) {
            return capabilities.CurrentExtent;
        } else {
            var framebufferSize = nativeWindow.FramebufferSize;

            Extent2D actualExtent = new() {
                Width = (uint)framebufferSize.X,
                Height = (uint)framebufferSize.Y
            };

            actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
            actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

            return actualExtent;
        }
    }

    public static SwapchainSupportDetails QuerySwapchainSupport(PhysicalDevice physicalDevice, VulkanSurface Surface) {
        unsafe {
            var details = new SwapchainSupportDetails();

            Surface.KhrSurfaceExt.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, Surface.SurfaceKhr, out details.Capabilities);

            uint formatCount = 0;
            Surface.KhrSurfaceExt.GetPhysicalDeviceSurfaceFormats(physicalDevice, Surface.SurfaceKhr, ref formatCount, null);

            if (formatCount != 0) {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats) {
                    Surface.KhrSurfaceExt.GetPhysicalDeviceSurfaceFormats(physicalDevice, Surface.SurfaceKhr, ref formatCount, formatsPtr);
                }
            } else {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            Surface.KhrSurfaceExt.GetPhysicalDeviceSurfacePresentModes(physicalDevice, Surface.SurfaceKhr, ref presentModeCount, null);

            if (presentModeCount != 0) {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes) {
                    Surface.KhrSurfaceExt.GetPhysicalDeviceSurfacePresentModes(physicalDevice, Surface.SurfaceKhr, ref presentModeCount, formatsPtr);
                }

            } else {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }
    }

    public struct SwapchainSupportDetails {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }
}
