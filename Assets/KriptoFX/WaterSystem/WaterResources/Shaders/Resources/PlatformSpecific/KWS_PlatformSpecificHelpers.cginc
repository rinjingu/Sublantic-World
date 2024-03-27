#ifndef KWS_PLATFORM_SPECIFIC_HELPERS
#define KWS_PLATFORM_SPECIFIC_HELPERS

//#define EXPANSE
//#define TIME_OF_DAY
//#define ENVIRO_FOG
//#define ENVIRO_3_FOG
//#define AZURE_FOG
//#define ATMOSPHERIC_HEIGHT_FOG
//#define VOLUMETRIC_FOG_AND_MIST
//#define CURVED_WORLDS

//ATMOSPHERIC_HEIGHT_FOG also need to change the "Queue" = "Transparent-1"      -> "Queue" = "Transparent+2"
//VOLUMETRIC_FOG_AND_MIST also need to enable "Water->Rendering->DrawToDepth"

#ifdef KWS_COMPUTE
	#undef ENVIRO_3_FOG
#endif

#define _FrustumCameraPlanes _FrustumPlanes
#define KWS_INITIALIZE_DEFAULT_MATRIXES float4x4 KWS_MATRIX_M = UNITY_MATRIX_M; float4x4 KWS_MATRIX_I_M = UNITY_MATRIX_I_M;

//------------------  unity includes   ----------------------------------------------------------------

#ifndef UNITY_COMMON_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#endif

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#endif

#ifndef UNITY_COLOR_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif

#ifndef UNITY_ATMOSPHERIC_SCATTERING_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"
#endif

#if defined(CURVED_WORLDS)
	#define CURVEDWORLD_BEND_TYPE_LITTLEPLANET_Y
	#define CURVEDWORLD_BEND_ID_1
	#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
#endif

//-------------------------------------------------------------------------------------------------------



//------------------  thid party assets  ----------------------------------------------------------------

#if defined(ENVIRO_FOG)
	#include "Assets/Third-party assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.hlsl"
#endif

#if defined(ENVIRO_3_FOG)
	#include "Assets/Enviro 3 - Sky and Weather/Resources/Shader/Includes/FogIncludeHLSL.hlsl"
#endif

#if defined(ATMOSPHERIC_HEIGHT_FOG)
	#include "Assets/Third-party assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Includes/AtmosphericHeightFog.cginc"
#endif

#if defined(VOLUMETRIC_FOG_AND_MIST)
	#include "Assets/VolumetricFog/Resources/Shaders/VolumetricFogOverlayVF.cginc"
#endif

#if defined(EXPANSE)
	#include "Assets/Third-party assets/Expanse/transparency/shaders/transparency.hlsl"
#endif

//-------------------------------------------------------------------------------------------------------

#ifndef KWS_WATER_VARIABLES
	#include "..\Common\KWS_WaterVariables.cginc"
#endif


float4 _CameraDepthTexture_TexelSize;
float4 _ColorPyramidTexture_TexelSize;

inline float4x4 UpdateCameraRelativeMatrix(float4x4 matrixM)
{
	return ApplyCameraTranslationToMatrix(matrixM);
}

inline float4 ObjectToClipPos(float4 vertex)
{
	#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
		CURVEDWORLD_TRANSFORM_VERTEX(vertex)
	#endif

	return TransformObjectToHClip(vertex.xyz);
}


inline float4 ObjectToClipPos(float4 vertex, float4x4 matrixM, float4x4 matrixIM)
{
	#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
		#if defined(USE_WATER_INSTANCING)
			unity_ObjectToWorld = matrixM;
			unity_WorldToObject = matrixIM;
		#endif
		CURVEDWORLD_TRANSFORM_VERTEX(vertex)
	#endif

	#if defined(USE_WATER_INSTANCING)
		return mul(GetWorldToHClipMatrix(), mul(matrixM, float4(vertex.xyz, 1.0)));
	#else
		return TransformObjectToHClip(vertex.xyz);
	#endif
}



inline float2 GetTriangleUVScaled(uint vertexID)
{
	#if UNITY_UV_STARTS_AT_TOP
		return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
	#else
		return float2((vertexID << 1) & 2, vertexID & 2);
	#endif
}

inline float2 GetScreenUV(float2 vertex)
{
	return vertex.xy / _ScreenSize.xy;
}

inline float2 GetNormalizedRTHandleUV(float2 screenUV)
{
	return screenUV /= _RTHandleScale.xy;
}

inline float4 GetTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
{
	float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
	return float4(uv * 2.0 - 1.0, z, 1.0);
}


inline float3 LocalToWorldPos(float3 localPos)
{
	return GetAbsolutePositionWS(mul(UNITY_MATRIX_M, float4(localPos, 1)).xyz);
}

inline float3 LocalToWorldPos(float3 localPos, float4x4 matrixM)
{
	#if defined(USE_WATER_INSTANCING)
		return GetAbsolutePositionWS(mul(matrixM, float4(localPos, 1)).xyz);
	#else
		return LocalToWorldPos(localPos);
	#endif
}

inline float3 WorldToLocalPos(float3 worldPos)
{
	return mul(UNITY_MATRIX_I_M, float4(GetCameraRelativePositionWS(worldPos), 1)).xyz;
}

inline float3 WorldToLocalPos(float3 worldPos, float4x4 matrixIM)
{
	#if defined(USE_WATER_INSTANCING)
		return mul(matrixIM, float4(GetCameraRelativePositionWS(worldPos), 1)).xyz;
	#else
		return WorldToLocalPos(worldPos);
	#endif
}

inline float3 WorldToLocalPosWithoutTranslation(float3 worldPos)
{
	return mul((float3x3)UNITY_MATRIX_I_M, worldPos).xyz;
}

inline float3 WorldToLocalPosWithoutTranslation(float3 worldPos, float4x4 matrixIM)
{
	#if defined(USE_WATER_INSTANCING)
		return mul((float3x3)matrixIM, worldPos).xyz;
	#else
		return WorldToLocalPosWithoutTranslation(worldPos).xyz;
	#endif
}


inline float3 GetCameraRelativePosition(float3 worldPos)
{
	return GetCameraRelativePositionWS(worldPos);
}

inline float3 GetCameraAbsolutePosition()
{
	//return UNITY_MATRIX_I_V._m03_m13_m23;
	//return Test4.xyz;
	return _WorldSpaceCameraPos.xyz; //cause shader error in 'Hidden/KriptoFX/KWS/VolumetricLighting': Program 'frag', error X8000: D3D11 Internal Compiler Error: Invalid Bytecode:
	//source register relative index temp register component 1 in r7 uninitialized. Opcode #61 (count is 1-based) at line 15 (on vulkan)

}

inline float3 GetWorldSpaceViewDirNorm(float3 worldPos)
{
	//return GetWorldSpaceNormalizeViewDir(GetCameraRelativePositionWS(worldPos)); //doesn't work with VR
	return normalize(_WorldSpaceCameraPos.xyz - worldPos);
}

inline float3 GetWorldSpaceNormal(float3 normal)
{
	return normalize(mul((float3x3)UNITY_MATRIX_M, normal)).xyz;
}

inline float3 GetWorldSpaceNormal(float3 normal, float4x4 matrixM)
{
	#if defined(USE_WATER_INSTANCING)
		return normalize(mul((float3x3)matrixM, normal)).xyz;
	#else
		return GetWorldSpaceNormal(normal);
	#endif
}

inline float GetWorldToCameraDistance(float3 worldPos)
{
	return length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}

inline float3 GetAbsoluteWorldSpacePos()
{
	return GetAbsolutePositionWS(UNITY_MATRIX_I_V._m03_m13_m23).xyz;
	//return _WorldSpaceCameraPos.xyz; //cause shader error in 'Hidden/KriptoFX/KWS/VolumetricLighting': Program 'frag', error X8000: D3D11 Internal Compiler Error: Invalid Bytecode:
	//source register relative index temp register component 1 in r7 uninitialized. Opcode #61 (count is 1-based) at line 15 (on vulkan)

}

float3 GetWorldSpacePositionFromDepth(float2 uv, float deviceDepth)
{
	return GetAbsolutePositionWS(ComputeWorldSpacePosition(uv, deviceDepth, UNITY_MATRIX_I_VP)); //todo UNITY_MATRIX_I_VP have bugs in VR in other renderings, but KWS_MATRIX_I_VP doesn't work with SSR, check why?

}

inline float GetSceneDepth(float2 uv)
{
	return SampleCameraDepth(clamp(uv, 0.0005, 0.9995));
}


inline float3 GetAmbientColor()
{
	return KWS_AmbientColor;
}

inline float3 GetAmbientColorExposed()
{
	float exposure = GetCurrentExposureMultiplier();
	//return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) * exposure;
	return KWS_AmbientColor * exposure;
}

inline float2 GetSceneColorNormalizedUV(float2 uv)
{
	#if defined(UNITY_STEREO_INSTANCING_ENABLED)
		return clamp(uv, 0.001, 0.999) * _RTHandleScale.xy;
	#else
		return clamp(uv, 0.001, 0.999) * _RTHandleScale.xy;
	#endif
}

inline float3 GetSceneColor(float2 uv)
{
	return LoadCameraColor(clamp(uv, 0.001, 0.999) * _ScreenSize.xy);
}

inline half3 GetSceneColorWithDispersion(float2 uv, float dispersionStrength)
{
	half3 refraction;
	refraction.r = GetSceneColor(uv - _ColorPyramidTexture_TexelSize.xy * dispersionStrength).r;
	refraction.g = GetSceneColor(uv).g;
	refraction.b = GetSceneColor(uv + _ColorPyramidTexture_TexelSize.xy * dispersionStrength).b;
	return refraction;
}

inline half3 GetSceneColorPoint(float2 uv)
{
	return GetSceneColor(uv);
}

inline float3 ScreenPosToWorldPos(float2 uv)
{
	float depth = GetSceneDepth(uv);
	float3 posWS = GetWorldSpacePositionFromDepth(uv, depth);
	return posWS;
	//return GetAbsolutePositionWS(posWS);

}

inline float4 WorldPosToScreenPos(float3 pos)
{
	pos = GetCameraRelativePositionWS(pos);
	float4 projected = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
	projected.xy = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		projected.xy.y = 1 - projected.xy.y;
	#endif
	return projected;
}

inline float2 WorldPosToScreenPosReprojectedPrevFrame(float3 pos, float2 texelSize)
{
	//pos = GetCameraRelativePositionWS(pos);
	float4 projected = mul(KWS_PREV_MATRIX_VP, float4(pos, 1.0f));
	float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		uv.y = 1 - uv.y;
	#endif
	return uv + texelSize * 0.5;
}

inline float3 GetMainLightDir()
{
	DirectionalLightData dirLight = _DirectionalLightDatas[_DirectionalShadowIndex];
	return -dirLight.forward;
}

inline float3 GetMainLightColor()
{
	DirectionalLightData dirLight = _DirectionalLightDatas[_DirectionalShadowIndex];
	return dirLight.color;
}

inline float3 KWS_GetSkyColor(float3 reflDir, float lod, float exposure)
{
	UNITY_BRANCH
	if (KWS_OverrideSkyColor == 1) return KWS_CustomSkyColor.xyz;
	else return SampleSkyTexture(reflDir, lod, 0).xyz * exposure;
}

inline float4 ComputeNonStereoScreenPos(float4 pos)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = pos.zw;
	return o;
}

inline float4 ComputeScreenPos(float4 pos)
{
	float4 o = ComputeNonStereoScreenPos(pos);
	#if defined(UNITY_SINGLE_PASS_STEREO)
		o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
	#endif
	return o;
}

inline float4 ComputeGrabScreenPos(float4 pos)
{
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0;
	#endif
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * scale) + o.w;
	#ifdef UNITY_SINGLE_PASS_STEREO
		o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
	#endif
	o.zw = pos.zw;
	return o;
}

inline float LinearEyeDepth(float z)
{
	return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}

//float4 EvaluateExpanseFogAndClouds(float linear01Depth, float2 uv, float4 color, float exposure, out float opacity)
inline void GetInternalFogVariables(float4 pos, float3 viewDir, float surfaceDepthZ, float screenPosZ, out half3 fogColor, out half3 fogOpacity)
{
	PositionInputs posInput = GetPositionInput(pos.xy, _ScreenSize.zw, pos.z, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
	EvaluateAtmosphericScattering(posInput, viewDir, fogColor, fogOpacity);
}


inline half3 ComputeInternalFog(half3 sourceColor, half3 fogColor, half3 fogOpacity)
{
	return lerp(sourceColor, fogColor, fogOpacity);
}

inline half3 ComputeThirdPartyFog(half3 sourceColor, float3 worldPos, float2 screenUV, float3 screenPosZ)
{
	#if defined(ENVIRO_FOG)
		//sourceColor = TransparentFog(half4(sourceColor, 1.0), worldPos.xyz, screenUV, screenPosZ); //todo check why it's white color?
	#elif defined(ENVIRO_3_FOG)
		sourceColor = ApplyFogAndVolumetricLights(sourceColor, screenUV, worldPos.xyz, Linear01Depth(screenPosZ, _ZBufferParams));
	#elif defined(AZURE_FOG)
		sourceColor = ApplyAzureFog(half4(sourceColor, 1.0), worldPos.xyz).xyz;
	#elif defined(ATMOSPHERIC_HEIGHT_FOG)
		float4 fogParams = GetAtmosphericHeightFog(GetCameraRelativePositionWS(worldPos));
		fogParams.a = saturate(fogParams.a * 1.5f); //by some reason max value < 0.75;
		sourceColor = ApplyAtmosphericHeightFog(half4(sourceColor, 1.0), fogParams).xyz;
	#elif defined(EXPANSE)
		PositionInputs posInput = GetPositionInput(screenUV, _ScreenSize.zw, screenPosZ, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
		float opacity = 0;
		sourceColor = EvaluateExpanseFogAndClouds(Linear01Depth(posInput.deviceDepth, _ZBufferParams), posInput.positionNDC, float4(sourceColor, 1), GetCurrentExposureMultiplier(), opacity).xyz;
	#endif

	return max(0, sourceColor);
}

inline float GetExposure()
{
	return GetCurrentExposureMultiplier();
}

float GetSurfaceDepth(float screenPosZ)
{
	#if UNITY_REVERSED_Z
		#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
			//GL with reversed z => z clip range is [near, -far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
			return max(-screenPosZ, 0);
		#else
			//D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
			//max is required to protect ourselves from near plane not being correct/meaningfull in case of oblique matrices.
			return max(((1.0 - screenPosZ / _ProjectionParams.y) * _ProjectionParams.z), 0);
		#endif
	#elif UNITY_UV_STARTS_AT_TOP
		//D3d without reversed z => z clip range is [0, far] -> nothing to do
		return screenPosZ;
	#else
		//Opengl => z clip range is [-near, far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
		return screenPosZ;
	#endif
}
#endif
