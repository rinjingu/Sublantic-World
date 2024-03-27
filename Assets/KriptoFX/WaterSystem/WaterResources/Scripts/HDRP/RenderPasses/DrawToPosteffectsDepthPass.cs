using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class DrawToPosteffectsDepthPass : WaterPass
    {
        DrawToPosteffectsDepthPassCore _pass;
        public DrawToPosteffectsDepthPass()
        {
            _pass                   =  new DrawToPosteffectsDepthPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }

        private void OnSetRenderTarget(WaterPassContext waterContext)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, waterContext.cameraColor, waterContext.cameraDepth);
        }

        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }

        public override void Release()
        {
            _pass.OnSetRenderTarget -= OnSetRenderTarget;
            _pass.Release();
        }
    }
}
