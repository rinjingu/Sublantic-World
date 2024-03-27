using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class FlowPass : WaterPass
    {
        FlowPassCore             _pass;

        public FlowPass()
        {
            _pass                   =  new FlowPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }

        private void OnSetRenderTarget(CommandBuffer passCommandBuffer, RTHandle rt1, RTHandle rt2)
        {
            CoreUtils.SetRenderTarget(passCommandBuffer, KWS_CoreUtils.GetMrt(rt1, rt2), rt1.rt.depthBuffer, ClearFlag.None, Color.clear);
        }
        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            _pass.ExecutePerFrame(cameras, fixedUpdates);
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