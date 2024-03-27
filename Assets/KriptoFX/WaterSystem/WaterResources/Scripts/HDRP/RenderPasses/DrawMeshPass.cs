using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class DrawMeshPass: WaterPass
    {
        DrawMeshPassCore _pass;
        public DrawMeshPass()
        {
            _pass = new DrawMeshPassCore();
            name  = _pass.PassName;
        }

        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }
        public override void ExecuteBeforeCameraRendering(Camera cam)
        {
            _pass.ExecuteBeforeCameraRendering(cam);
        }

        public override void Release()
        {
            _pass.Release();
        }
    }
}