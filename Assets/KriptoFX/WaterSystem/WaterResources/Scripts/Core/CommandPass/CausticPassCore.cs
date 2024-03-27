using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;
using static KWS.WaterSystemScriptableData;

namespace KWS
{
    internal class CausticPassCore: WaterPassCore
    {
        public Action<WaterPass.WaterPassContext, RTHandle, int> OnRenderToCausticTarget;
        public Action<WaterPass.WaterPassContext> OnRenderToCameraTarget;

        private Dictionary<int, Mesh> _causticMeshes = new Dictionary<int, Mesh>();

        private Mesh _decalMesh;
        Material _decalMaterial;
        Material _causticMaterial;

        const float KWS_CAUSTIC_MULTIPLIER = 0.15f;

        Dictionary<CausticTextureResolutionQualityEnum, int> _causticQualityToMeshQuality = new Dictionary<CausticTextureResolutionQualityEnum, int>()
        {
            {CausticTextureResolutionQualityEnum.Ultra, 512},
            {CausticTextureResolutionQualityEnum.High, 384},
            {CausticTextureResolutionQualityEnum.Medium, 256},
            {CausticTextureResolutionQualityEnum.Low, 128},
        };

        Dictionary<CausticTextureResolutionQualityEnum, float> _causticQualityToDispersionStrength = new Dictionary<CausticTextureResolutionQualityEnum, float>()
        {
            {CausticTextureResolutionQualityEnum.Ultra, 1.5f},
            {CausticTextureResolutionQualityEnum.High, 1.25f},
            {CausticTextureResolutionQualityEnum.Medium, 1.0f},
            {CausticTextureResolutionQualityEnum.Low, 0.75f},
        };


        internal override string PassName => "Water.CausticPass";

        public CausticPassCore()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
            _decalMaterial                                 =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CausticDecalShaderName, useWaterStencilMask: true);
            _causticMaterial                               =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CausticComputeShaderName);
            _decalMesh                                     =  MeshUtils.CreateCubeMesh();
        }

        void InitializeTextures()
        {
            var size = WaterSharedResources.MaxCausticResolution;
            var slices = WaterSharedResources.MaxCausticArraySlices;
            WaterSharedResources.CausticRTArray = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R8_UNorm, name: "_CausticRTArray", useMipMap: true, autoGenerateMips: false, slices: slices, dimension:TextureDimension.Tex2DArray);
            Shader.SetGlobalTexture(CausticID.KWS_CausticRTArray, WaterSharedResources.CausticRTArray);

            KWS_CoreUtils.ClearRenderTexture(WaterSharedResources.CausticRTArray.rt, ClearFlag.Color, new Color(KWS_CAUSTIC_MULTIPLIER, KWS_CAUSTIC_MULTIPLIER, KWS_CAUSTIC_MULTIPLIER));
            this.WaterLog(WaterSharedResources.CausticRTArray);
        }

        void ReleaseTextures()
        {
            WaterSharedResources.CausticRTArray?.Release();
            WaterSharedResources.CausticRTArray = null;
            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
           
            ReleaseTextures();
            KW_Extensions.SafeDestroy(_decalMesh, _decalMaterial, _causticMaterial);

            foreach (var causticMesh in _causticMeshes) KW_Extensions.SafeDestroy(causticMesh.Value);
            _causticMeshes.Clear();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if(!WaterSharedResources.IsAnyWaterUseCaustic) return;

            if (changedTabs.HasTab(WaterSystem.WaterTab.Caustic))
            {
                if (WaterSharedResources.CausticRTArray == null 
                 || WaterSharedResources.CausticRTArray.rt.width != WaterSharedResources.MaxCausticResolution 
                 || WaterSharedResources.CausticRTArray.rt.volumeDepth != WaterSharedResources.MaxCausticArraySlices)
                {
                    ReleaseTextures();
                    InitializeTextures();
                }
            }
        }

        public override void Execute(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseCaustic) return;

            if(WaterSharedResources.CausticRTArray == null) InitializeTextures();

            var currentCausticID = 0;
            if (WaterSharedResources.IsAnyWaterUseGlobalWind)
            {
                ComputeCaustic(waterContext, WaterSharedResources.GlobalWaterSystem, currentCausticID);
                currentCausticID++;
            }

            
            for (var i = 0; i < WaterSharedResources.WaterInstances.Count; i++)
            {
                var instance = WaterSharedResources.WaterInstances[i];
                if (!instance.Settings.UseGlobalWind && instance.Settings.UseCausticEffect && IsWaterVisibleAndActive(instance))
                {
                    WaterSharedResources.InstanceToCausticID[i + 1] = currentCausticID;
                    ComputeCaustic(waterContext, instance, currentCausticID);
                    currentCausticID++;
                }
                else
                {
                    WaterSharedResources.InstanceToCausticID[i + 1] = -1;
                }
            }

            if (WaterSharedResources.CausticRTArray != null) waterContext.cmd.GenerateMips(WaterSharedResources.CausticRTArray);

            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (instance.Settings.UseCausticEffect && IsWaterVisibleAndActive(instance))
                {
                    DrawCausticDecal(waterContext, instance);
                }
            }

            Shader.SetGlobalFloatArray(CausticID.KWS_InstanceToCausticID, WaterSharedResources.InstanceToCausticID);
        }

        void ComputeCaustic(WaterPass.WaterPassContext waterContext, WaterSystem waterInstance, int causticID)
        {
            var cmd = waterContext.cmd;

            OnRenderToCausticTarget?.Invoke(waterContext, WaterSharedResources.CausticRTArray, causticID);
            cmd.SetGlobalFloat(CausticID.KWS_CausticDepthScale, waterInstance.Settings.GetCurrentCausticDepth);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WindSpeed, waterInstance.Settings.CurrentWindSpeed);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesCascades, waterInstance.Settings.CurrentFftWavesCascades);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, waterInstance.Settings.CurrentWavesAreaScale);

            cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.GetFftWavesDisplacementTexture(waterInstance).GetSafeTexture());

            cmd.SetKeyword(KWS_ShaderConstants.CausticKeywords.USE_CAUSTIC_FILTERING, waterInstance.Settings.GetCurrentCausticHighQualityFiltering && (int)waterInstance.Settings.CurrentFftWavesQuality <= 64);

            var mesh = GetOrCreateCausticMesh(waterInstance.Settings.GetCurrentCausticTextureResolutionQuality);
            cmd.DrawMesh(mesh, Matrix4x4.identity, _causticMaterial);
        }


        void DrawCausticDecal(WaterPass.WaterPassContext waterContext, WaterSystem waterInstance)
        {
            var cmd = waterContext.cmd;
            
            cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, waterInstance.WaterPivotWorldPosition);
            cmd.SetGlobalFloat(CausticID.KWS_CaustisStrength, waterInstance.Settings.CausticStrength);
            cmd.SetGlobalFloat(CausticID.KWS_CaustisDispersionStrength, _causticQualityToDispersionStrength[waterInstance.Settings.GetCurrentCausticTextureResolutionQuality]);
            cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            if (waterInstance.Settings.UseFlowMap) waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FlowMap);

            waterContext.cmd.SetKeyword(WaterKeywords.USE_SHORELINE,    waterInstance.Settings.UseShorelineRendering);
            waterContext.cmd.SetKeyword(WaterKeywords.KW_DYNAMIC_WAVES, waterInstance.Settings.UseDynamicWaves);
            waterContext.cmd.SetKeyword(WaterKeywords.KW_FLOW_MAP,      waterInstance.Settings.UseFlowMap && !waterInstance.Settings.UseFluidsSimulation);
            waterContext.cmd.SetKeyword(CausticKeywords.USE_DISPERSION, waterInstance.Settings.UseCausticDispersion);

            var decalSize = waterInstance.WorldSpaceBounds.size;
            var decalPos = waterInstance.WaterRelativeWorldPosition;

            if (waterInstance.Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                var farDistance = waterContext.cam.farClipPlane;
                decalSize.x = Mathf.Min(decalSize.x, farDistance);
                decalSize.z = Mathf.Min(decalSize.z, farDistance);
                decalSize.y = KWS_Settings.Caustic.CausticDecalHeight;

                decalPos.y -= KWS_Settings.Caustic.CausticDecalHeight * 0.5f - 1;
            }
            else if(waterInstance.Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            {
                decalPos.y -= decalSize.y * 0.5f;
            }
            
            var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, decalSize); //todo precompute trs matrix
           
            OnRenderToCameraTarget?.Invoke(waterContext);
            cmd.DrawMesh(_decalMesh, decalTRS, _decalMaterial);

        }

        Mesh GetOrCreateCausticMesh(CausticTextureResolutionQualityEnum quality)
        {
            var size = _causticQualityToMeshQuality[quality];
            if (!_causticMeshes.ContainsKey(size))
            {
                _causticMeshes.Add(size, MeshUtils.CreatePlaneMesh(size, 1.2f));
            }

            return _causticMeshes[size];
        }

    }
}