using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{

    internal class KWS_WaterPassHandler 
    {
        HashSet<WaterPass> _waterPasses = new HashSet<WaterPass>();

        FftWavesPass                                           _fftWavesPass;
        BuoyancyPass                                           _buoyancyPass;
        private FlowPass                                       _flowPass;
        DynamicWavesPass                                       _dynamicWavesPass;
        ShorelineWavesPass                                     _shorelineWavesPass;
        WaterPrePass                                           _waterPrePass;
        CausticPass                                            _causticPass;
        ReflectionFinalPass                                    _reflectionFinalPass;
        ScreenSpaceReflectionPass                              _ssrPass;
        VolumetricLightingPass                                 _volumetricLightingPass;
        DrawMeshPass                                           _drawMeshPass;
        ShorelineFoamPass                                      _shorelineFoamPass;
        UnderwaterPass                                         _underwaterPass;
        DrawToPosteffectsDepthPass                             _drawToDepthPass;

        Dictionary<CustomPassInjectionPoint, CustomPassVolume> _volumes = new Dictionary<CustomPassInjectionPoint, CustomPassVolume>();

        OrthoDepthPass _orthoDepthPass;

        internal KWS_WaterPassHandler()
        {
            ////////////////////////////////////////////////// update passes /////////////////////////////////////////////////////
            _orthoDepthPass   = new OrthoDepthPass();
            _fftWavesPass     = new FftWavesPass();
            _buoyancyPass     = new BuoyancyPass();
            _flowPass         = new FlowPass();
            _dynamicWavesPass = new DynamicWavesPass();

            CreateVolume(CustomPassInjectionPoint.BeforePreRefraction);
            CreateVolume(CustomPassInjectionPoint.BeforeTransparent);
        }

        public void Release()
        {
            if (_waterPasses != null)
            {
                foreach (var waterPass in _waterPasses)
                {
                    waterPass?.Release();
                }
            }
            _orthoDepthPass?.Release();
            _fftWavesPass?.Release();
            _buoyancyPass?.Release();
            _flowPass?.Release();
            _dynamicWavesPass?.Release();
        }

        internal void OnBeforeFrameRendering(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            _orthoDepthPass.ExecutePerFrame(cameras, fixedUpdates);
            _fftWavesPass.ExecutePerFrame(cameras, fixedUpdates);
            _buoyancyPass.ExecutePerFrame(cameras, fixedUpdates);
            _flowPass.ExecutePerFrame(cameras, fixedUpdates);
            _dynamicWavesPass.ExecutePerFrame(cameras, fixedUpdates);
        }


        internal void OnBeforeCameraRendering(Camera cam, ScriptableRenderContext ctx)
        {
            //"volume.customCamera" and "volumePass.enabled" just ignored for other cameras in the current frame... Typical unity HDRiP rendering.
            try
            {
                var data = cam.GetComponent<HDAdditionalCameraData>();
                //if (data == null) return;

                var cameraSize = KWS_CoreUtils.GetScreenSizeLimited(KWS_CoreUtils.SinglePassStereoEnabled);
                KWS_CoreUtils.RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y);

                WaterPass.WaterPassContext waterContext = default;
                waterContext.cam = cam;

                waterContext.RenderContext        = ctx;
                waterContext.AdditionalCameraData = data;

               
                _orthoDepthPass.Execute(waterContext);

                InitializePass(ref _shorelineWavesPass, waterContext, CustomPassInjectionPoint.BeforePreRefraction);
                InitializePass(ref _waterPrePass,       waterContext, CustomPassInjectionPoint.BeforePreRefraction);
                InitializePass(ref _causticPass,        waterContext, CustomPassInjectionPoint.BeforePreRefraction);

                InitializePass(ref _ssrPass,                       waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _reflectionFinalPass,           waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _volumetricLightingPass,        waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _drawMeshPass,      waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _shorelineFoamPass, waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _underwaterPass,    waterContext, CustomPassInjectionPoint.BeforeTransparent);
                InitializePass(ref _drawToDepthPass,   waterContext, CustomPassInjectionPoint.BeforeTransparent);

                _drawMeshPass.ExecuteBeforeCameraRendering(cam);
                _shorelineFoamPass.ExecuteBeforeCameraRendering(cam);
                _underwaterPass.ExecuteBeforeCameraRendering(cam);

            }
            catch (Exception e)
            {
                Debug.LogError("Water rendering error: " + e.InnerException);
            }
        }

        void CreateVolume(CustomPassInjectionPoint injectionPoint)
        {
            var tempGO = new GameObject("WaterVolume_" + injectionPoint) { hideFlags = HideFlags.DontSave };
            tempGO.transform.parent = WaterSystem.UpdateManagerObject.transform;
            var volume = tempGO.AddComponent<CustomPassVolume>();
            volume.injectionPoint = injectionPoint;
            _volumes.Add(injectionPoint, volume);
        }


        void InitializePass<T>(ref T pass, WaterPass.WaterPassContext waterContext, CustomPassInjectionPoint injectionPoint) where T : WaterPass
        {
            if (pass == null)
            {
                var volumePass = _volumes[injectionPoint];
                pass = (T)volumePass.AddPassOfType<T>();
                _waterPasses.Add(pass);
            }
            pass.SetWaterContext(waterContext);
        }

    }
}