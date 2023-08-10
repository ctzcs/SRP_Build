using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RPAsset是存储句柄和渲染设置的地方。
/// </summary>
[CreateAssetMenu(menuName = "Rending/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
   

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}
