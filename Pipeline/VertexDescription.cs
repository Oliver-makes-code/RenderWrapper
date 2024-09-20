namespace RenderWrapper.Pipeline;

public readonly record struct VertexDescription(
    // The number of bytes between values
    uint Stride,
    // The number of vertices to step through before the next value
    uint InstanceStepRate = 1
);
