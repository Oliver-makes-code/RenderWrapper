using RenderWrapper;
using RenderWrapper.Resource;

var window = new GameWindow(delta => {
    // Render
}, delta => {
    // Tick
});

var vertShaderCode = File.ReadAllBytes("vert.spv");
var fragShaderCode = File.ReadAllBytes("frag.spv");

var vertShaderModule = new Shader(vertShaderCode, window.VulkanContext);
var fragShaderModule = new Shader(fragShaderCode, window.VulkanContext);



window.Start();
