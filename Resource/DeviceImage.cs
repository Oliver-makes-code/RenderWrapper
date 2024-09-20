using RenderWrapper.Vulkan;
using Silk.NET.Vulkan;

namespace RenderWrapper.Resource;

public sealed class DeviceImage : IDisposable {
    public readonly Image NativeImage;
    public readonly Format Format;
    private readonly Vk Vk;
    private readonly VulkanDevice Device;

    public DeviceImage(VulkanContext ctx, Format format, uint width, uint height, uint depth = 1, int arrayLayers = 1) {
        Vk = ctx.Vk;
        Device = ctx.Device;

        unsafe {
            ImageCreateInfo info = new() {
                Format = format,
                Extent = new Extent3D(width, height, depth)
            };
            Vk.CreateImage(Device.LogicalDevice, &info, null, out NativeImage);
        }
        Format = format;
    }

    public DeviceImage(Image nativeImage, Format nativeImageFormat, Vk vk, VulkanDevice device) {
        NativeImage = nativeImage;
        Format = nativeImageFormat;
        Vk = vk;
        Device = device;
    }

    public void Dispose() {
        unsafe {
            Vk.DestroyImage(Device.LogicalDevice, NativeImage, null);
        }
    }

    public static implicit operator Image (DeviceImage img)
        => img.NativeImage;

    public static implicit operator Format (DeviceImage img)
        => img.Format;

    public sealed class View : IDisposable {
        public readonly ImageView NativeImageView;
        public readonly DeviceImage Image;

        public View(DeviceImage image, VulkanContext ctx) : this(image, ctx.Vk, ctx.Device) {}

        public View(DeviceImage image, Vk vk, VulkanDevice device) {
            Image = image;
            ImageViewCreateInfo createInfo = new() {
                SType = StructureType.ImageViewCreateInfo,
                Image = Image,
                ViewType = ImageViewType.Type2D,
                Format = Image,
                Components = {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange = {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            unsafe {
                vk.CreateImageView(device.LogicalDevice, &createInfo, null, out NativeImageView);
            }
        }

        public void Dispose() {
            unsafe {
                Image.Vk.DestroyImageView(Image.Device.LogicalDevice, NativeImageView, null);
            }
        }
    }
}
