using RenderWrapper.Vulkan;
using Silk.NET.Vulkan;

namespace RenderWrapper.Resource;

public sealed class DeviceImage {
    public readonly Image NativeImage;
    public readonly Format NativeImageFormat;

    public DeviceImage(VulkanContext ctx)
        => throw new NotImplementedException();

    public DeviceImage(Image nativeImage, Format nativeImageFormat) {
        NativeImage = nativeImage;
        NativeImageFormat = nativeImageFormat;
    }

    public static implicit operator Image (DeviceImage img)
        => img.NativeImage;

    public static implicit operator Format (DeviceImage img)
        => img.NativeImageFormat;

    public sealed class View {
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
    }
}
