using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal class ProbePlanarReflection: ReflectionPass
    {
        private PlanarReflectionProbe _probe;
        private GameObject _probeGO;
        private Transform _probeTransform;
        private Material _filteringMaterial;
        private CommandBuffer _cmdAnisoFiltering;
        RenderTexture _currentPlanarRT;
        RenderTexture _planarMipFilteredRT;
        private WaterSystem _waterInstance;

        private readonly Dictionary<WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum, PlanarReflectionAtlasResolution> _planarResolutions
            = new Dictionary<WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum, PlanarReflectionAtlasResolution>()

            {
                {WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum.Ultra, PlanarReflectionAtlasResolution.Resolution1024},
                {WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum.High, PlanarReflectionAtlasResolution.Resolution1024},
                {WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum.Medium, PlanarReflectionAtlasResolution.Resolution512},
                {WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum.Low, PlanarReflectionAtlasResolution.Resolution512},
                {WaterSystemScriptableData.PlanarReflectionResolutionQualityEnum.VeryLow, PlanarReflectionAtlasResolution.Resolution256}
            };

        public override void RenderReflection(WaterSystem waterInstance, Camera currentCamera)
        {
            _waterInstance = waterInstance;
            if (!_waterInstance.Settings.UsePlanarReflection)
            {
                return;
            }
            if (!_waterInstance.Settings.EnabledMeshRendering) return;

            if (_probeGO == null) CreateProbe(waterInstance);

            var cameraPos = currentCamera.GetCameraPositionFast();
            _probeTransform.position = new Vector3(cameraPos.x, waterInstance.WaterRelativeWorldPosition.y, cameraPos.z);

            UpdateRT();
        }

        public override void OnEnable()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnWaterSettingsChanged;
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasTab(WaterSystem.WaterTab.Reflection)) return;

            if (!waterInstance.Settings.UsePlanarReflection)
            {
                if (_probeGO != null) KW_Extensions.SafeDestroy(_probeGO);
                return;
            }
            else
            {
               
                UpdateProbeSettings(waterInstance);
                UpdateRT();
            }

        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnWaterSettingsChanged;

            KW_Extensions.SafeDestroy(_probeGO, _filteringMaterial);
        }

        void UpdateRT()
        {
            if (_probe == null || _probeGO == null) return;

            _currentPlanarRT = _probe.realtimeTexture;
            if (_currentPlanarRT == null) return;
            CreateTargetTexture(_currentPlanarRT.width, _currentPlanarRT.graphicsFormat);

            WaterSharedResources.PlanarReflection = _currentPlanarRT;
        }

        void CreateProbe(WaterSystem waterInstance)
        {
            _probeGO               = new GameObject("PlanarReflectionProbe");
            _probeGO.layer         = KWS_Settings.Water.WaterLayer;
            _probeTransform        = _probeGO.transform;
            _probeTransform.parent = WaterSystem.UpdateManagerObject.transform;
            _probe                 = _probeGO.AddComponent<PlanarReflectionProbe>();
            
            _probe.mode                    = ProbeSettings.Mode.Realtime;
            _probe.realtimeMode            = ProbeSettings.RealtimeMode.EveryFrame;
            _probe.influenceVolume.boxSize = new Vector3(100000, float.MinValue, 100000);

            UpdateProbeSettings(waterInstance);
        }

        void UpdateProbeSettings(WaterSystem waterInstance)
        {
            if (_probe == null || _probeGO == null) return;

            _probe.DisableAllCameraFrameSettings();
           
            _probe.SetFrameSetting(FrameSettingsField.OpaqueObjects,      true);
            _probe.SetFrameSetting(FrameSettingsField.TransparentObjects, true);

            _probe.SetFrameSetting(FrameSettingsField.VolumetricClouds, waterInstance.Settings.RenderPlanarClouds);
            _probe.SetFrameSetting(FrameSettingsField.AtmosphericScattering, waterInstance.Settings.RenderPlanarVolumetricsAndFog);
            _probe.SetFrameSetting(FrameSettingsField.Volumetrics, waterInstance.Settings.RenderPlanarVolumetricsAndFog);
            _probe.SetFrameSetting(FrameSettingsField.ShadowMaps,            waterInstance.Settings.RenderPlanarShadows);

            _probe.settingsRaw.roughReflections                       = false;
            _probe.settings.resolutionScalable.useOverride         = true;
            _probe.settings.resolutionScalable.@override           = _planarResolutions[waterInstance.Settings.PlanarReflectionResolutionQuality];
            _probe.settingsRaw.cameraSettings.culling.cullingMask     = waterInstance.Settings.PlanarCullingMask;
            _probe.settingsRaw.cameraSettings.customRenderingSettings = true;

            _probeGO.SetActive(false);
            _probeGO.SetActive(true);
        }

        void CreateTargetTexture(int size, GraphicsFormat graphicsFormat)
        {
            if (_planarMipFilteredRT != null && (_planarMipFilteredRT.width != size || _planarMipFilteredRT.graphicsFormat != graphicsFormat))
            {
                _planarMipFilteredRT.Release();
                _planarMipFilteredRT = null;
            }

            if(_planarMipFilteredRT == null) _planarMipFilteredRT = new RenderTexture(size, size, 0, graphicsFormat) { name = "_planarMipFilteredRT", autoGenerateMips = false, useMipMap = false };
        }
    }
}