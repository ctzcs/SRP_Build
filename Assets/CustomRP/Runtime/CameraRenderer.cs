
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 这个类可以让每个相机负责自己的渲染，交给RP管理
/// 这样可以让每个相机支持不同的渲染方法
/// </summary>
public partial class CameraRenderer
{
    /// <summary>
    /// 定义自定义RenderPipeline时，可以使用ScriptableRenderContext来安排状态更新和绘图命令，并将其提交到GPU。
    /// RenderPipeline.Render方法实现通常会剔除渲染管道不需要为每个Camera渲染的对象（请参见CullingResults），
    /// 然后对ScriptableRenderContext.DrawRenderers进行一系列调用，并与ScriptableRenderContext.ExecuteCommandBuffer调用混合。
    /// 这些调用可设置全局着色器属性、更改渲染目标、分派计算着色器以及其他渲染任务。
    /// 要实际执行渲染循环，请调用ScriptableRenderContext.Submit。
    /// </summary>
    private ScriptableRenderContext context;
    private Camera camera;
    //虽然天空盒可以通过已经声明的方法绘制，但是其他的命令必须通东单独的cb间接绘制，所以我们需要cb来绘制其他的几何体
    private const string bufferName = "Render Camera";
    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };
    //剔除的结果
    private CullingResults cullingResults;
    //指定允许那种shader pass。目前只支持unlit，所以得获取SRPDefaultUnlit Pass的id，缓存到静态字段
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    

    public void Render(ScriptableRenderContext context,Camera camera)
    {
        this.context = context;
        this.camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        //在开始渲染之前进行剔除
        if(!Cull())return;
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        //Gizmo应该在最后画出来
        DrawGizmos();
        Submit();
    }
    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
    /// <summary>
    /// 设置view-projection矩阵
    /// 用于shader中的投影空间转换
    /// </summary>
    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        //清除缓冲区中的数据，深度，颜色，用处清除的颜色(就是把这块缓冲区设置成什么颜色)
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
            camera.backgroundColor.linear : Color.clear);
        //开始采样的命令
        //使用commandBuffer来注入分析器采样
        //我们可以使用命令缓冲区注入分析器样本，这些样本将同时显示在分析器和帧调试器中。这是通过在适当的点调用 BeginSample 和 EndSample 来完成的
        buffer.BeginSample(SampleName);
        //先执行已经有的内容，并清除
        ExecuteBuffer();
        
    }
    /// <summary>
    /// 画到缓冲区中
    /// </summary>
    void DrawVisibleGeometry()
    {
        
        //排序设置
        var sortingSetting = new SortingSettings(camera)
        {
            //可以强制自定义的排序
            criteria = SortingCriteria.CommonOpaque
        };
        //渲染设置
        var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSetting);
        //过滤设置，指出哪些渲染队列是允许的
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        //将剔除结果传入Renderers并提供绘画设置，和filter设置就能进行绘制几何体
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);
        //画天空盒
        context.DrawSkybox(camera);
        
        
        //渲染透明队列
        //透明队列是从后往前渲染，因为混合是和背景混合
        //由于透明对象不写入深度缓冲区，因此从前到后对它们进行排序没有性能优势。但是，当透明对象在视觉上彼此落后时，它们必须从后到前绘制以正确混合。
        sortingSetting.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSetting;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults,ref drawingSettings,ref filteringSettings);
    }

    
    /// <summary>
    /// 提交到实际的渲染队列
    /// </summary>
    void Submit()
    {
        //结束采样的命令
        buffer.EndSample(SampleName);
        //提交的时候也执行已经有的内容，并清除
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        //要执行buffer，就必须在上下文中调用ExecuteCommandBuffer
        //这个行为从缓冲区复制命令但是不清除缓冲区，所以我们需要手动清除
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }


    
}
