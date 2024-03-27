using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;


namespace KWS
{
    internal class VolumetricLightingPassCore : WaterPassCore
    {
        public Action<WaterPass.WaterPassContext, RTHandle> OnSetRenderTarget;

        Material _volumeMaterial;

        internal      Dictionary<Camera, VolumetricData>                             _volumetricDatas  = new Dictionary<Camera, VolumetricData>();
        private const int                                                            MaxVolumetricDataCameras = 5;
        private       WaterSystemScriptableData.VolumetricLightResolutionQualityEnum _lastResolutionSetting;

        internal override string PassName => "Water.VolumetricLightingPass";

        public VolumetricLightingPassCore()
        {
            _volumeMaterial        = CreateMaterial(ShaderNames.VolumetricLightingShaderName);
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }


        internal class VolumetricData
        {
            public RTHandle[] VolumetricLightRT = new RTHandle[2];
            public static RTHandle   _tempRT;

            public  int        Frame;

            public Matrix4x4   PrevVPMatrix;
            public Matrix4x4[] PrevVPMatrixStereo = new Matrix4x4[2];

            internal void InitializeTextures()
            {
                var resolutionDownsample = (int)WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality / 100f;
                var maxSize = KWS_CoreUtils.GetScreenSizeLimited(false);
                var height = (int)(maxSize.y * resolutionDownsample);
                var width = (int)(height * 2); // typical resolution ratio is 16x9 (or 2x1), for better pixel filling we use [2 * width] x [height], instead of square [width] * [height].
                for (int idx = 0; idx < 2; idx++) VolumetricLightRT[idx] = KWS_CoreUtils.RTHandleAllocVR(width, height, name: "_volumeLightRT" + idx, colorFormat: GraphicsFormat.R16G16B16A16_SFloat);
                
                if(_tempRT == null) _tempRT = KWS_CoreUtils.RTHandleAllocVR(width, height, name: "_volumeLightTempRT", colorFormat: GraphicsFormat.R16G16B16A16_SFloat);

                this.WaterLog(VolumetricLightRT[0]);
            }

            internal void Update()
            {
                Frame++;
                if (Frame > int.MaxValue - 1) Frame = 0;

                if (KWS_CoreUtils.SinglePassStereoEnabled) PrevVPMatrixStereo = WaterSystem.CurrentVPMatrixStereo;
                else PrevVPMatrix                                             = WaterSystem.CurrentVPMatrix;
            }

            internal void ReleaseTextures()
            {
                VolumetricLightRT[0]?.Release();
                VolumetricLightRT[1]?.Release();
                _tempRT?.Release();

                VolumetricLightRT[0] = VolumetricLightRT[1] = _tempRT = null;
            }

            internal void Release()
            {
                ReleaseTextures();
                this.WaterLog("", KW_Extensions.WaterLogMessageType.Release);
            }
        }

        public bool RequireReinitialize()
        {
            if (_lastResolutionSetting == WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality) return false;
            _lastResolutionSetting = WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality;
            return true;

        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if (changedTabs.HasTab(WaterSystem.WaterTab.VolumetricLighting))
            {
                if (RequireReinitialize()) ReleaseVolumetricDatas();
            }
        }


        void ReleaseVolumetricDatas()
        {
            foreach (var data in _volumetricDatas)
            {
                data.Value?.Release();
            }
            _volumetricDatas.Clear();
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseVolumetricDatas();
            KW_Extensions.SafeDestroy(_volumeMaterial);
            

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }
        public override void Execute(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseVolumetricLighting) return;

            var cmd = waterContext.cmd;
            var cam = waterContext.cam;
            var settings = WaterSharedResources.GlobalSettings;

            if (_volumetricDatas.Count > MaxVolumetricDataCameras) _volumetricDatas.Clear();
            if (!_volumetricDatas.ContainsKey(cam))
            {
                _volumetricDatas.Add(cam, new VolumetricData());
            }
            var data = _volumetricDatas[cam];
            if (data.VolumetricLightRT[0] == null) data.InitializeTextures();


            var targetRT     = data.Frame % 2 == 0 ? data.VolumetricLightRT[0] : data.VolumetricLightRT[1];
            var lastTargetRT = data.Frame % 2 == 0 ? data.VolumetricLightRT[1] : data.VolumetricLightRT[0];
            UpdateShaderParams(cmd, data, targetRT, lastTargetRT);

            OnSetRenderTarget?.Invoke(waterContext, targetRT);
            cmd.BlitTriangleRTHandle(targetRT, _volumeMaterial, ClearFlag.None, Color.clear, 0);
           
            data.Update();
            WaterSharedResources.VolumetricLightingRT = targetRT;
            //Shader.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, WaterSharedResources.VolumetricLightingRT.rtHandleProperties.rtHandleScale);
            //Shader.SetGlobalTexture(VolumetricLightConstantsID.KWS_VolumetricLightRT, WaterSharedResources.VolumetricLightingRT);
        }


        Vector4 ComputeMieVector(float mieG)
        {
            return new Vector4(1 - (mieG * mieG), 1 + (mieG * mieG), 2 * mieG, 1.0f / (4.0f * Mathf.PI));
        }

        private void UpdateShaderParams(CommandBuffer cmd, VolumetricData data, RTHandle targetRT, RTHandle lastTargetRT)
        {
            var anisoMie = ComputeMieVector(0.05f);

            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_LightAnisotropy, anisoMie);
            cmd.SetGlobalFloat(VolumetricLightConstantsID.KWS_VolumetricLightTemporalAccumulationFactor, WaterSharedResources.GlobalSettings.VolumetricLightTemporalAccumulationFactor);
           
            cmd.SetGlobalInteger(VolumetricLightConstantsID.KWS_RayMarchSteps, WaterSharedResources.GlobalSettings.VolumetricLightIteration);

            var useCaustic = WaterSharedResources.IsAnyWaterUseCaustic && !WaterSharedResources.IsAnyWaterUseVolumetricLightAdditionalCaustic;
            var useAdditionalCaustic = WaterSharedResources.IsAnyWaterUseCaustic && WaterSharedResources.IsAnyWaterUseVolumetricLightAdditionalCaustic;

            if (WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality == WaterSystemScriptableData.VolumetricLightResolutionQualityEnum.Low ||
                WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality == WaterSystemScriptableData.VolumetricLightResolutionQualityEnum.VeryLow)
            {
                useCaustic = false;
                useAdditionalCaustic = false;
            }

            cmd.SetKeyword(WaterKeywords.USE_CAUSTIC, useCaustic);
            cmd.SetKeyword(WaterKeywords.USE_ADDITIONAL_CAUSTIC, useAdditionalCaustic);

            if (KWS_CoreUtils.SinglePassStereoEnabled) cmd.SetGlobalMatrixArray(KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP_STEREO, data.PrevVPMatrixStereo);
            else cmd.SetGlobalMatrix(KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP, data.PrevVPMatrix);

            cmd.SetGlobalTexture(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT_Last, lastTargetRT);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLightRT_Last_RTHandleScale, lastTargetRT.rtHandleProperties.rtHandleScale);
            cmd.SetGlobalInt(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_Frame, data.Frame);


            cmd.SetGlobalTexture(VolumetricLightConstantsID.KWS_VolumetricLightRT, targetRT);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, targetRT.rtHandleProperties.rtHandleScale);


            //var currentSize                        = targetRT.GetScaledSize(targetRT.rtHandleProperties.currentViewportSize);
            //var targetSize                         = targetRT.rtHandleProperties.currentViewportSize;
            //var downscaleFactor = new Vector2((float)targetSize.x / currentSize.x, (float)targetSize.y / currentSize.y);
            //cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLightDownscaleFactor, downscaleFactor);
        }



    }
}