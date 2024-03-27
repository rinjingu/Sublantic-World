using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class DynamicWavesPass : WaterPass
    {
        DynamicWavesPassCore _pass;

        public DynamicWavesPass()
        {
            _pass                   =  new DynamicWavesPassCore();
            _pass.OnSetRenderTarget += OnSetRenderTarget;
            name                    =  _pass.PassName;
        }

        private void OnSetRenderTarget(CommandBuffer passCommandBuffer, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(passCommandBuffer, rt, ClearFlag.Color, Color.black);
        }

        protected override void Execute(CustomPassContext ctx)
        {
            ExecutePassCore(ctx, _pass);
        }
        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            _pass.ExecutePerFrame(cameras, fixedUpdates);
        }


        public override void Release()
        {
            _pass.OnSetRenderTarget -= OnSetRenderTarget;
            _pass.Release();
        }
    }
}