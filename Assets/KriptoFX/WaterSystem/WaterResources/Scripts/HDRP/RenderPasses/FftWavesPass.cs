using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class FftWavesPass : WaterPass
    {
        FftWavesPassCore _pass;
        public FftWavesPass()
        {
            _pass = new FftWavesPassCore();
            name  = _pass.PassName;
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
            _pass.Release();
        }

      
    }
}