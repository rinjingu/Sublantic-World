using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class ShorelineWavesPass : WaterPass
    {
        ShorelineWavesPassCore _pass;

        public ShorelineWavesPass()
        {
            _pass                   =  new ShorelineWavesPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }

        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }

        private void OnSetRenderTarget(WaterPassContext waterContext, RTHandle rt1, RTHandle rt2)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(rt1, rt2), rt1, ClearFlag.Color, Color.black);
        }

        public override void Release()
        {
            _pass.OnSetRenderTarget -= OnSetRenderTarget;
            _pass.Release();
        }
    }
}
