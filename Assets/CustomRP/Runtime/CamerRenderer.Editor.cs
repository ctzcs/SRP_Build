
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
   
    
    partial void PrepareBuffer();
    partial void PrepareForSceneWindow();
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    
#if UNITY_EDITOR
    private static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material errorMaterial;
    private string SampleName { get; set; }
    
    /// <summary>
    /// 让每个相机有自己的镜头，在编辑器模式下，让每个相机有自己的buffer
    /// </summary>
    partial void PrepareBuffer()
    {
        buffer.name = SampleName = camera.name;
    }
    /// <summary>
    /// 在scene中绘制UI
    /// </summary>
    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }
    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawSettings = new DrawingSettings(legacyShaderTagIds[0],new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial
        };
        //从第二个开始绘制多通道
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawSettings.SetShaderPassName(i,legacyShaderTagIds[i]);
        }
        var filterSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults,ref drawSettings,ref filterSettings);
    }
    partial void DrawGizmos()
    {
        //检查是否应该绘制gizmo
        if (Handles.ShouldRenderGizmos())
        {
            //相机，和绘制Gizmo的哪个子集
            context.DrawGizmos(camera,GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera,GizmoSubset.PostImageEffects);
        }
    }

#else
  const string SamepleName = bufferName;
#endif
    
}
