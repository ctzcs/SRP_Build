
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class CustomRenderPipeline:RenderPipeline
    {
        private CameraRenderer renderer = new CameraRenderer();
        /// <summary>
        /// 自定义SRP的入口点，每帧Unity在RP实例上调用Render
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cameras">RP负责渲染所有相机的画面</param>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }
        
        
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                renderer.Render(context,cameras[i]);
            }
        }
    }
