using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal abstract class WaterPassCore
    {

        internal abstract string PassName { get; }

        public virtual void Execute(WaterPass.WaterPassContext waterContext)
        {
        }

        public virtual void ExecuteBeforeCameraRendering(Camera cam)
        {
        }

        public virtual void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
        }

        public abstract void Release();
    }

}