using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    internal class DrawMeshPassCore: WaterPassCore
    {
        internal override string                PassName => "Water.DrawMeshPass";

        public DrawMeshPassCore()
        {
        }

        public override void Release()
        {
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }


        public override void Execute(WaterPass.WaterPassContext waterContext)
        {
        }

        public override void ExecuteBeforeCameraRendering(Camera cam)
        {
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (cam == null) return;
                var settings = waterInstance.Settings;
                var mat      = waterInstance.InstanceData.GetCurrentWaterMaterial();
                mat.SetFloat(DynamicWaterParams.KWS_ScaledTime, KW_Extensions.TotalTime() * settings.CurrentTimeScale);

                UpdateMaterialParams(mat, waterInstance);

                switch (settings.WaterMeshType)
                {
                    case WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean:
                    case WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox:
                        DrawInstancedQuadTree(cam, waterInstance, waterInstance.InstanceData.GetCurrentWaterMaterial(), false);
                        break;
                    case WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh:
                        DrawCustomMesh(waterInstance, cam, mat);
                        break;
                     case WaterSystemScriptableData.WaterMeshTypeEnum.River:
                        DrawRiverSpline(waterInstance, cam, mat);
                         break;
                }
            }
        }


        public static void DrawInstancedQuadTree(Camera cam, WaterSystem waterInstance, Material mat, bool isPrePass)
        {
            waterInstance._meshQuadTree.UpdateQuadTree(cam, waterInstance);
            var isFastMode = isPrePass && !waterInstance.IsCameraUnderwaterForInstance;
            if (!waterInstance._meshQuadTree.TryGetRenderingContext(cam, isFastMode, out var context)) return;

            mat.SetBuffer(StructuredBuffers.InstancedMeshData, context.visibleChunksComputeBuffer);
          
            Graphics.DrawMeshInstancedIndirect(context.chunkInstance, 0, mat, waterInstance.WorldSpaceBounds, context.visibleChunksArgs, camera: cam);
        }


        private void DrawCustomMesh(WaterSystem waterInstance, Camera cam, Material mat)
        {
            var mesh = waterInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, waterInstance.WaterPivotWorldRotation, waterInstance.WaterSize);
            Graphics.DrawMesh(mesh, matrix, mat, 0, camera: cam);
        }

        private void DrawRiverSpline(WaterSystem waterInstance, Camera cam, Material mat)
        {
            var mesh = waterInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix        = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(mesh, matrix, mat, 0, camera: cam);
        }
            
        private void UpdateMaterialParams(Material mat, WaterSystem waterInstance)
        {
            var settings = waterInstance.Settings;

            //cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, waterInstance.WaterRelativeWorldPosition);
            //cmd.SetGlobalFloat(DynamicWaterParams.KW_Time, KW_Extensions.TotalTime());
            //cmd.SetGlobalFloat(DynamicWaterParams.KWS_ScaledTime, KW_Extensions.TotalTime() * settings.CurrentTimeScale);

            //cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            waterInstance.InstanceData.UpdateDynamicShaderParams(mat, WaterInstanceResources.DynamicPassEnum.FFT);

           // if (settings.UsePlanarReflection) waterInstance.InstanceData.UpdateDynamicShaderParams(mat, WaterInstanceResources.DynamicPassEnum.PlanarReflection);
            if (settings.UseFlowMap) waterInstance.InstanceData.UpdateDynamicShaderParams(mat,          WaterInstanceResources.DynamicPassEnum.FlowMap);
            if (settings.UseFluidsSimulation) waterInstance.InstanceData.UpdateDynamicShaderParams(mat, WaterInstanceResources.DynamicPassEnum.FluidsSimulation);

            //mat.SetKeyword(WaterKeywords.KW_FLOW_MAP,        waterInstance.Settings.UseFlowMap && !waterInstance.Settings.UseFluidsSimulation);
            //mat.SetKeyword(WaterKeywords.KW_FLOW_MAP_FLUIDS, waterInstance.Settings.UseFlowMap && waterInstance.Settings.UseFluidsSimulation);
            //mat.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean 
            //                                                || waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox);
        }
    }
}