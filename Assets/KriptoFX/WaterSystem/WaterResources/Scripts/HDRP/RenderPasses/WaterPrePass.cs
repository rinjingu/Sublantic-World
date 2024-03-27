using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class WaterPrePass : WaterPass
    {
        WaterPrePassCore _pass;

        public WaterPrePass()
        {
            _pass                           =  new WaterPrePassCore();
            _pass.OnInitializedRenderTarget += OnInitializedRenderTarget;
            name                            =  _pass.PassName;
        }
        private void OnInitializedRenderTarget(WaterPassContext waterContext, RTHandle rt1, RTHandle rt2, RTHandle rt3)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(rt1, rt2), rt3, ClearFlag.All, Color.clear);
        }


        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }


        public override void Release()
        {
            _pass.OnInitializedRenderTarget -= OnInitializedRenderTarget;
            _pass.Release();
        }

    }
}
