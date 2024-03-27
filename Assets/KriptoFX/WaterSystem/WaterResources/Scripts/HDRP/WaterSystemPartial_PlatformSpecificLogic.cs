using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public partial class WaterSystem
    { 
        ///////////////////////////// platform specific components /////////////////////////////////////////////////
        internal ReflectionPass PlanarReflectionComponent  = new ProbePlanarReflection();
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
 

        internal static List<ThirdPartyAssetDescription> ThirdPartyFogAssetsDescriptions = new List<ThirdPartyAssetDescription>()
        {
            new ThirdPartyAssetDescription() {EditorName = "Native Unity Fog", ShaderDefine = ""},
            new ThirdPartyAssetDescription() {EditorName = "Expanse", AssetNameSearchPattern = "Expanse", ShaderDefine          = "EXPANSE", ShaderInclude     = "transparency.hlsl"},
            new ThirdPartyAssetDescription() {EditorName = "Time Of Day", AssetNameSearchPattern = "Time Of Day", ShaderDefine      = "TIME_OF_DAY", ShaderInclude = ""},
            new ThirdPartyAssetDescription(){EditorName = "Enviro", AssetNameSearchPattern = "Enviro - Sky and Weather", ShaderDefine                    = "ENVIRO_FOG", ShaderInclude              = "EnviroFogCore.hlsl"},
            new ThirdPartyAssetDescription(){EditorName = "Enviro 3", AssetNameSearchPattern = "Enviro 3 - Sky and Weather", ShaderDefine = "ENVIRO_3_FOG", ShaderInclude = "FogIncludeHLSL.hlsl"},
            new ThirdPartyAssetDescription(){EditorName = "Atmospheric height fog", AssetNameSearchPattern = "Atmospheric Height Fog", ShaderDefine    = "ATMOSPHERIC_HEIGHT_FOG", ShaderInclude  = "AtmosphericHeightFog.cginc", CustomQueueOffset = 2},
            //new ThirdPartyAssetDescription(){EditorName = "Volumetric fog and mist 2", ShaderDefine = "VOLUMETRIC_FOG_AND_MIST", ShaderInclude = "VolumetricFogOverlayVF.cginc", DrawToDepth = true},
        };

        public static void OverrideCameraRequiredSettings(Camera cam)
        {
           
        }

        void InitializeWaterPlatformSpecificResources()
        {
            isWaterPlatformSpecificResourcesInitialized = true;
        }

        void RenderPlatformSpecificFeatures(Camera cam)
        {
           
        }

        void ReleasePlatformSpecificResources()
        {
            isWaterPlatformSpecificResourcesInitialized = false;
        }

        static void SetGlobalPlatformSpecificShaderParams(Camera cam)
        {
            Shader.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_SkyTexture, ReflectionProbe.defaultTexture);

            SphericalHarmonicsL2 sh;
            LightProbes.GetInterpolatedProbe(cam.GetCameraPositionFast(), null, out sh);
            var ambient = new Vector3(sh[0, 0] - sh[0, 6], sh[1, 0] - sh[1, 6], sh[2, 0] - sh[2, 6]);
            ambient = Vector3.Max(ambient, Vector3.zero);
            Shader.SetGlobalVector(KWS_ShaderConstants.DynamicWaterParams.KWS_AmbientColor, ambient);
        }
    }
}