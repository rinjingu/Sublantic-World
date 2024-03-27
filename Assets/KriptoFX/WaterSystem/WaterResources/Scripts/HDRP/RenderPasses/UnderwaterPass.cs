using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class UnderwaterPass: WaterPass
    {
        UnderwaterPassCore _pass;

        public UnderwaterPass()
        {
            _pass                   =  new UnderwaterPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }
        private void OnSetRenderTarget(WaterPassContext waterContext, RTHandle rt)
        {
            if (rt == null) CoreUtils.SetRenderTarget(waterContext.cmd, waterContext.cameraColor, waterContext.cameraDepth);
            else CoreUtils.SetRenderTarget(waterContext.cmd,            rt);
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
            _pass.OnSetRenderTarget -= OnSetRenderTarget;
            _pass.Release();
        }
    }
}