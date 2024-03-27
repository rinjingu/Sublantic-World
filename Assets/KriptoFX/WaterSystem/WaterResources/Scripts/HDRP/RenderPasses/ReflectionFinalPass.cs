using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    class ReflectionFinalPass: WaterPass
    {
        ReflectionFinalPassCore _pass;
        public ReflectionFinalPass()
        {
            _pass                           =  new ReflectionFinalPassCore();
            _pass.OnInitializedRenderTarget += OnInitializedRenderTarget;
            name                            =  _pass.PassName;
        }

        private void OnInitializedRenderTarget(WaterPassContext waterContext, RTHandle rt)
        {
            CoreUtils.SetRenderTarget(waterContext.cmd, rt, ClearFlag.Color, Color.black);
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