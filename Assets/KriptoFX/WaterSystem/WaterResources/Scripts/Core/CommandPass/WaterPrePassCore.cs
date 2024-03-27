using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    internal class WaterPrePassCore : WaterPassCore
    {
        public Action<WaterPass.WaterPassContext, RTHandle, RTHandle, RTHandle> OnInitializedRenderTarget;
        readonly Vector2 _rtScale = new Vector2(0.5f, 0.5f);
        readonly Vector2 _rtScaleBlur = new Vector2(0.5f, 0.5f);
        private const float maskBlurRadius = 1.5f;
        KW_PyramidBlur _pyramidBlur;
       
        internal override string PassName => "Water.PrePass";

        public WaterPrePassCore()
        {
            _pyramidBlur = new KW_PyramidBlur();
        }

        void InitializeTextures()
        {
            WaterSharedResources.WaterMaskRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterMaskRT", colorFormat: GraphicsFormat.R16G16B16A16_UNorm);
            WaterSharedResources.WaterMaskRTBlurred = KWS_CoreUtils.RTHandleAllocVR(_rtScaleBlur, name: "_waterMaskRTBlurred", colorFormat: GraphicsFormat.R8_UNorm);
            WaterSharedResources.WaterIdRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterIdRT", colorFormat: GraphicsFormat.R8_UNorm);
            WaterSharedResources.WaterDepthRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterDepthRT", depthBufferBits: DepthBits.Depth24);

            Shader.SetGlobalTexture(MaskPassID.KW_WaterDepth_ID, WaterSharedResources.WaterDepthRT);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterTexID, WaterSharedResources.WaterIdRT);
            Shader.SetGlobalTexture(MaskPassID.KW_WaterMaskScatterNormals_ID, WaterSharedResources.WaterMaskRT);


            this.WaterLog(WaterSharedResources.WaterMaskRT, WaterSharedResources.WaterIdRT, WaterSharedResources.WaterDepthRT);
        }

        void ReleaseTextures()
        {
            WaterSharedResources.WaterMaskRT?.Release();
            WaterSharedResources.WaterDepthRT?.Release();
            WaterSharedResources.WaterIdRT?.Release();
            WaterSharedResources.WaterMaskRT = WaterSharedResources.WaterDepthRT = WaterSharedResources.WaterIdRT = null;


            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }

        public override void Release()
        {
            ReleaseTextures();
            _pyramidBlur.Release();
            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        public override void Execute(WaterPass.WaterPassContext waterContext)
        {
            if (WaterSharedResources.WaterMaskRT == null) InitializeTextures();

            OnInitializedRenderTarget?.Invoke(waterContext, WaterSharedResources.WaterMaskRT, WaterSharedResources.WaterIdRT, WaterSharedResources.WaterDepthRT);

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (IsRequireRenderMask(waterInstance)) ExecuteInstance(waterContext.cam, waterContext.cmd, waterInstance);
            }

            if (WaterSystem.IsCameraUnderwater)
            {
                _pyramidBlur.ComputeBlurPyramid(maskBlurRadius, WaterSharedResources.WaterMaskRT, WaterSharedResources.WaterMaskRTBlurred, waterContext.cmd, _rtScaleBlur);
                Shader.SetGlobalTexture(MaskPassID.KWS_WaterMaskBlurred, WaterSharedResources.WaterMaskRTBlurred);
            }
            else Shader.SetGlobalTexture(MaskPassID.KWS_WaterMaskBlurred, WaterSharedResources.WaterMaskRT);

            Shader.SetGlobalVector(MaskPassID.KWS_WaterMask_RTHandleScale, WaterSharedResources.WaterMaskRT.rtHandleProperties.rtHandleScale);
        }

        public void ExecuteInstance(Camera cam, CommandBuffer cmd, WaterSystem waterInstance)
        {
            UpdateMaterialParams(cmd, waterInstance);

            switch (waterInstance.Settings.WaterMeshType)
            {
                case WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean:
                case WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox:
                    //DrawInstancedQuadTree(cam, cmd, waterInstance);
                    var shaderPass = waterInstance.CanRenderTesselation ? 1 : 0;
                    var mat = waterInstance.InstanceData.MaterialWaterPrePass;
                    DrawInstancedQuadTree(cam, cmd, waterInstance, mat, shaderPass);
                    break;
                case WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh:
                    DrawCustomMesh(cmd, waterInstance);
                    break;
                case WaterSystemScriptableData.WaterMeshTypeEnum.River:
                    DrawRiverSpline(cmd, waterInstance);
                    break;
            }
            
        }

        public static void DrawInstancedQuadTree(Camera cam, CommandBuffer cmd, WaterSystem waterInstance, Material mat, int shaderPass)
        {
            var isFastMode = !waterInstance.IsCameraUnderwaterForInstance;
            if (!waterInstance._meshQuadTree.TryGetRenderingContext(cam, isFastMode, out var context)) return;

            Vector3 size;
            Vector3 pos;
            if (waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox)
            {
                size = waterInstance.WaterSize;
                pos  = waterInstance.WaterPivotWorldPosition;
            }
            else
            {
                size = new Vector3(100000, 1, 100000);
                pos  = waterInstance.WaterRelativeWorldPosition;
                pos.y = Mathf.Min(cam.GetCameraPositionFast().y, waterInstance.WaterPivotWorldPosition.y) - 250;
            }

            cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, false);
            var matrix = Matrix4x4.TRS(pos, waterInstance.WaterPivotWorldRotation, size);
            cmd.DrawMesh(context.underwaterMesh, matrix, mat, 0, 0);

            cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, true);
            cmd.SetGlobalBuffer(StructuredBuffers.InstancedMeshData, context.visibleChunksComputeBuffer);
            cmd.DrawMeshInstancedIndirect(context.chunkInstance, 0, mat, shaderPass, context.visibleChunksArgs);

            cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, false);
        }

        private void DrawCustomMesh(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var mesh = waterInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, waterInstance.WaterPivotWorldRotation, waterInstance.WaterSize);
            var shaderPass = waterInstance.CanRenderTesselation ? 1 : 0;
            cmd.DrawMesh(mesh, matrix, waterInstance.InstanceData.MaterialWaterPrePass, 0, shaderPass);
        }

        private void DrawRiverSpline(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var mesh = waterInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix     = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);
            var shaderPass = waterInstance.CanRenderTesselation ? 1 : 0;
            cmd.DrawMesh(mesh, matrix, waterInstance.InstanceData.MaterialWaterPrePass, 0, shaderPass);
        }

        bool IsRequireRenderMask(WaterSystem waterInstance)
        {
            if (!IsWaterVisibleAndActive(waterInstance)) return false;
            if (WaterSystem.IsCameraUnderwater && waterInstance.IsCameraUnderwaterForInstance == false) return false;

            var settings = waterInstance.Settings;
            if (settings.EnabledMeshRendering == false) return settings.UseCausticEffect;
            return true;
        }

        private void UpdateMaterialParams(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var settings = waterInstance.Settings;

            //cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, waterInstance.WaterRelativeWorldPosition);
            //cmd.SetGlobalFloat(DynamicWaterParams.KW_Time, KW_Extensions.TotalTime());
            cmd.SetGlobalFloat(DynamicWaterParams.KWS_ScaledTime, KW_Extensions.TotalTime() * settings.CurrentTimeScale);
            //cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FFT);


            if (settings.UseFluidsSimulation) waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FlowMap);
            //cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean
            //                                                || waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox);

        }
    }
}