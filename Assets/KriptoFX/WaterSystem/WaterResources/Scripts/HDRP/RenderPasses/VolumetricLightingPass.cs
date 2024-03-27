using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class VolumetricLightingPass : WaterPass
    {
        VolumetricLightingPassCore _pass;

        public VolumetricLightingPass()
        {
            _pass                   =  new VolumetricLightingPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }
        private void OnSetRenderTarget(WaterPassContext waterContext, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, rt, ClearFlag.Color, Color.black);
        }

        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }


        public override void Release()
        {
            _pass.Release();
            _pass.OnSetRenderTarget -= OnSetRenderTarget;
        }
    }
}