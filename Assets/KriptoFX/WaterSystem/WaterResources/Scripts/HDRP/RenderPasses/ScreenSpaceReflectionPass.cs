using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class ScreenSpaceReflectionPass: WaterPass
    {
        ScreenSpaceReflectionPassCore _pass;

        public ScreenSpaceReflectionPass()
        {
            _pass                   =  new ScreenSpaceReflectionPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }

        private void OnSetRenderTarget(WaterPassContext waterContext, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, rt, ClearFlag.Color, Color.clear);
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