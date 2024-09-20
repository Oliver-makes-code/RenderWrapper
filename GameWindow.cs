using RenderWrapper.Vulkan;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;

namespace RenderWrapper;

public sealed class GameWindow {
    public readonly IWindow NativeWindow;
    public readonly VulkanContext VulkanContext;

    public GameWindow(Action<GameWindow, double> render, Action<GameWindow, double> update, bool vsync = true, string appName = "Window", int updatesPerSecond = 20) {
        SdlWindowing.Use();
        
        var options = WindowOptions.DefaultVulkan with {
            Size = new Vector2D<int>(320, 240),
            Title = appName,
            UpdatesPerSecond = updatesPerSecond,
            VSync = vsync,
        };

        NativeWindow = Window.Create(options);
        NativeWindow.Initialize();

        if (NativeWindow.VkSurface is null)
            throw new Exception("Windowing platform doesn't support Vulkan");

        NativeWindow.Render += delta => render(this, delta);
        NativeWindow.Update += delta => update(this, delta);

        VulkanContext = new(NativeWindow, appName);
    }

    public void Start() {
        NativeWindow.Run();
    }
}
