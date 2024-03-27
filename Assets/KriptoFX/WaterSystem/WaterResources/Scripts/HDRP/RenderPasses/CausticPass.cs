using System.Collections.Generic;
using KWS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{

    internal class CausticPass: WaterPass
    {
        CausticPassCore _pass;

        public CausticPass()
        {
            _pass                         =  new CausticPassCore();
            _pass.OnRenderToCausticTarget += OnRenderToCausticTarget;
            _pass.OnRenderToCameraTarget  += OnRenderToCameraTarget;
            name                          =  _pass.PassName;
        }

        private void OnRenderToCausticTarget(WaterPassContext waterContext, RTHandle rt, int slice)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, rt, ClearFlag.Color, Color.black, depthSlice: slice);
        }

        private void OnRenderToCameraTarget(WaterPassContext waterContext)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, waterContext.cameraColor, waterContext.cameraDepth);
        }
        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }

        public override void Release()
        {
            _pass.OnRenderToCausticTarget -= OnRenderToCausticTarget;
            _pass.OnRenderToCameraTarget  -= OnRenderToCameraTarget;
            _pass.Release();
        }
    }
}
