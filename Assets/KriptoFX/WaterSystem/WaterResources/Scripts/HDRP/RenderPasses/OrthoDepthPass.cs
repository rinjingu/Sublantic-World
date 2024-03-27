using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class OrthoDepthPass
    {
        OrthoDepthPassCore _pass;

        internal OrthoDepthPass()
        {
            _pass          =  new OrthoDepthPassCore();
            _pass.OnRender += OnRender;
        }

        public void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            foreach (var camera in cameras)
            {
                if (camera != null) _pass.OnFrameUpdate(camera);
            }
        }


        public void Execute(WaterPass.WaterPassContext waterContext)
        {
            _pass.Execute(waterContext);
        }

        private void OnRender(Camera depthCamera, RenderTexture depthRT)
        {
            var data               = depthCamera.GetComponent<HDAdditionalCameraData>();
            if (data == null) data = depthCamera.gameObject.AddComponent<HDAdditionalCameraData>();
            data.DisableAllCameraFrameSettings();
            data.SetCameraFrameSetting(FrameSettingsField.OpaqueObjects, true);
            data.clearColorMode          = HDAdditionalCameraData.ClearColorMode.None;
            data.customRenderingSettings = true;


            var currentShadowDistance = QualitySettings.shadowDistance;
            var lodBias               = QualitySettings.lodBias;

            var terrains   = Terrain.activeTerrains;
            var pixelError = new float[terrains.Length];
            for (var i = 0; i < terrains.Length; i++) pixelError[i] = terrains[i].heightmapPixelError;

            try
            {
                QualitySettings.shadowDistance = 0;
                QualitySettings.lodBias        = 10;
                foreach (var terrain in terrains) terrain.heightmapPixelError = 1;

                depthCamera.targetTexture = depthRT;
                depthCamera.Render();
                this.WaterLog("Render ortho depth");
            }
            finally
            {
                for (var i = 0; i < terrains.Length; i++)
                {
                    terrains[i].heightmapPixelError = pixelError[i];
                }
                QualitySettings.shadowDistance = currentShadowDistance;
                QualitySettings.lodBias        = lodBias;
            }
        }

        public void Release()
        {
            _pass.OnRender -= OnRender;
            _pass.Release();
        }

      
    }
}

