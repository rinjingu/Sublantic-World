using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal class UnderwaterPassCore : WaterPassCore
    {
        public Action<WaterPass.WaterPassContext, RTHandle> OnSetRenderTarget;

        KW_PyramidBlur _pyramidBlur;

        Material _underwaterMaterial;
        Material _underwaterToScreenMaterial;
        RTHandle _underwaterRT;
        RTHandle _underwaterRTBlured;

        readonly Vector2         _rtScale = new Vector2(0.35f, 0.35f);
        RenderParams             _renderParams;

        internal override string PassName => "Water.UnderwaterPass";
        public UnderwaterPassCore()
        {

        }

        public override void Release()
        {
            _underwaterRT?.Release();
            _underwaterRTBlured?.Release();
            _pyramidBlur?.Release();

            KW_Extensions.SafeDestroy(_underwaterMaterial, _underwaterToScreenMaterial);

            KW_Extensions.WaterLog(this, "", KW_Extensions.WaterLogMessageType.Release);
        }
        void InitializeTextures()
        {
            var hdrFormat = KWS_CoreUtils.GetGraphicsFormatHDR();
            _underwaterRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_underwaterRT", colorFormat: hdrFormat);
            _underwaterRTBlured = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_underwaterRT_Blured", colorFormat: hdrFormat);

            KW_Extensions.WaterLog("UnderwaterPass", _underwaterRT, _underwaterRTBlured);
        }

        public override void Execute(WaterPass.WaterPassContext waterContext)
        {
            //execute underwater pass only with "blur" effect, in other case execute it using default Graphics and queue oreder
            if (!WaterSystem.IsCameraUnderwater) return;

            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (instance.Settings.UseUnderwaterEffect
                 && instance.Settings.UseUnderwaterBlur
                 && instance.IsCameraUnderwaterForInstance
                 && KWS_CoreUtils.IsWaterVisibleAndActive(instance)
                 && instance.Settings.EnabledMeshRendering) ExecuteInstance(waterContext, instance);
            }
        }

        public override void ExecuteBeforeCameraRendering(Camera cam)
        {
            if (!WaterSystem.IsCameraUnderwater) return;
            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (instance.Settings.UseUnderwaterEffect
                 && !instance.Settings.UseUnderwaterBlur
                 && instance.IsCameraUnderwaterForInstance
                 && KWS_CoreUtils.IsWaterVisibleAndActive(instance)
                 && instance.Settings.EnabledMeshRendering) ExecuteInstanceBeforeCameraRendering(cam, instance);
            }
        }

        void ExecuteInstanceBeforeCameraRendering(Camera cam, WaterSystem waterInstance)
        {
            InitMaterial();
            UpdateShaderParams(waterInstance);

            //Graphics.DrawProcedural(_underwaterMaterial, waterInstance.WorldSpaceBounds, MeshTopology.Triangles, 3, 1);

            _renderParams.camera           = cam;
            _renderParams.material         = _underwaterMaterial;
            Graphics.RenderPrimitives(in _renderParams, MeshTopology.Triangles, 3, 1);
        }

        private void InitMaterial()
        {
            if (_underwaterMaterial == null)
            {
                _underwaterMaterial       = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.UnderwaterShaderName, useWaterStencilMask: true);
                _renderParams             = new RenderParams(_underwaterMaterial);
                _renderParams.worldBounds = new Bounds(Vector3.zero, 10000000 * Vector3.one);
            }
        }

        private void UpdateShaderParams(WaterSystem waterInstance)
        {
            var settings                      = waterInstance.Settings;
            var usePhysicalApproximationColor = settings.UnderwaterRenderingMode == WaterSystemScriptableData.UnderwaterRenderingModeEnum.PhysicalAproximation && !settings.UseScreenSpaceReflection;
            var usePhysicalApproximationSSR   = settings.UnderwaterRenderingMode == WaterSystemScriptableData.UnderwaterRenderingModeEnum.PhysicalAproximation && settings.UseScreenSpaceReflection;

            _underwaterMaterial.SetInteger(KWS_ShaderConstants.DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            _underwaterMaterial.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_Transparent, settings.Transparent);
            _underwaterMaterial.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_Turbidity,   settings.Turbidity);
            _underwaterMaterial.SetVector(KWS_ShaderConstants.ConstantWaterParams.KW_WaterColor,     settings.WaterColor);
            _underwaterMaterial.SetVector(KWS_ShaderConstants.ConstantWaterParams.KW_TurbidityColor, settings.TurbidityColor);

            //_underwaterMaterial.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_VOLUMETRIC_LIGHT,                  settings.UseVolumetricLight);
            _underwaterMaterial.SetKeyword(KWS_ShaderConstants.UnderwaterKeywords.USE_PHYSICAL_APPROXIMATION_COLOR, usePhysicalApproximationColor);
            _underwaterMaterial.SetKeyword(KWS_ShaderConstants.UnderwaterKeywords.USE_PHYSICAL_APPROXIMATION_SSR,   usePhysicalApproximationSSR);
            _underwaterMaterial.SetKeyword(KWS_ShaderConstants.UnderwaterKeywords.USE_HALF_LINE_TENSION,            settings.UseUnderwaterHalfLineTensionEffect);
            _underwaterMaterial.SetKeyword(KWS_ShaderConstants.UnderwaterKeywords.USE_CUSTOM_SCENE_DEPTH_COPY,      settings.DrawToPosteffectsDepth);

            _underwaterMaterial.renderQueue = KWS_Settings.Water.DefaultWaterQueue + settings.TransparentSortingPriority + KWS_Settings.Water.UnderwaterQueueOffset;
        }

        void ExecuteInstance(WaterPass.WaterPassContext waterContext, WaterSystem waterInstance)
        {
            var cmd = waterContext.cmd;

            InitMaterial();
            UpdateShaderParams(waterInstance);

            //if (settings.DrawToPosteffectsDepth && WaterSharedResources.DepthCopyBeforeWaterWriting != null)
            //{
            //    cmd.SetGlobalTexture("KWS_SceneDepthCopy", WaterSharedResources.DepthCopyBeforeWaterWriting);
            //    cmd.SetGlobalVector("KWS_SceneDepthCopy_RTHandleScale", WaterSharedResources.DepthCopyBeforeWaterWriting.rtHandleProperties.rtHandleScale);
            //}

            var settings = waterInstance.Settings;
            if (settings.UseUnderwaterBlur)
            {
                if (_underwaterRT               == null) InitializeTextures();
                if (_pyramidBlur                == null) _pyramidBlur                = new KW_PyramidBlur();
                if (_underwaterToScreenMaterial == null) _underwaterToScreenMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.UnderwaterBlurToScreenShaderName, useWaterStencilMask: true);

                OnSetRenderTarget?.Invoke(waterContext, _underwaterRT);
                cmd.BlitTriangleRTHandle(_underwaterRT, _underwaterMaterial, ClearFlag.None, Color.clear, 0);

                if (settings.UnderwaterBlurRadius < 2.5f)
                    _pyramidBlur.ComputeSeparableBlur(settings.UnderwaterBlurRadius, _underwaterRT, _underwaterRTBlured, cmd, _rtScale);
                else _pyramidBlur.ComputeBlurPyramid(settings.UnderwaterBlurRadius - 3.0f, _underwaterRT, _underwaterRTBlured, cmd, _rtScale);

                cmd.SetGlobalVector(KWS_ShaderConstants.UnderwaterID.KWS_Underwater_RTHandleScale, Vector4.one);
                OnSetRenderTarget?.Invoke(waterContext, null);
                cmd.BlitTriangle(_underwaterRTBlured, _underwaterToScreenMaterial);
                
                //_underwaterToScreenMaterial.SetTexture(KWS_CoreUtils._sourceRT_id, _underwaterRTBlured);
                //_underwaterToScreenMaterial.renderQueue = KWS_Settings.Water.DefaultWaterQueue + settings.TransparentSortingPriority + KWS_Settings.Water.UnderwaterQueueOffset;

                //_renderParams.camera           = waterContext.cam;
                //_renderParams.material         = _underwaterToScreenMaterial;
                //Graphics.RenderPrimitives(in _renderParams, MeshTopology.Triangles, 3, 1);
            }
        }


    }
}