#ifndef KWS_SHARED_API_INCLUDED

#define KWS_SHARED_API_INCLUDED


#ifndef SHADERGRAPH_PREVIEW

	#include "../PlatformSpecific/KWS_PlatformSpecificHelpers.cginc"
	#include "../Common/KWS_WaterPassHelpers.cginc"
	#include "../Common/KWS_WaterHelpers.cginc"

#endif

float3 KWS_ParticlesPos; //particle system transform world position
float3 KWS_ParticlesScale; //particle system transform localScale


inline float3 TileWarpParticlesOffsetXZ(float3 vertex, float3 center)
{
	float3 halfScale = KWS_ParticlesScale * 0.5;
	float3 quadOffset = vertex.xyz - center.xyz;
	vertex.xz = frac((center.xz + halfScale.xz - KWS_ParticlesPos.xz) / KWS_ParticlesScale.xz) * KWS_ParticlesScale.xz; //aabb warp
	vertex.xz += KWS_ParticlesPos.xz + quadOffset.xz - halfScale.xz; //ofset relative to pivot and size
	return vertex;
}


inline float3 GetWaterSurfaceCollisionForQuadParticles(float3 vertex, float3 center)
{
	#ifdef SHADERGRAPH_PREVIEW
		return vertex;
	#else
		float4 screenPos = ComputeScreenPos(ObjectToClipPos(float4(vertex, 1)));
		float2 screenUV = screenPos.xy / screenPos.w;
		uint waterID = GetWaterID(screenUV);
		if (waterID == 0)
		{
			vertex = 0.0 / 0.0;
			return vertex;
		}

		float3 waterPos = KWS_WaterPositionArray[waterID];
		float3 waterDisplacement = GetFftWavesDisplacement(vertex);
		float3 quadOffset = vertex.xyz - center.xyz;
		float quadOffsetLength = length(quadOffset);

		if (center.y > waterPos.y - quadOffsetLength)
		{
			center.y = waterPos.y + waterDisplacement.y - quadOffsetLength;
			vertex = center.xyz + quadOffset;
		}
		
		return vertex;
	#endif
}


inline float4 GetUnderwaterColorAlbedo(float2 uv, float3 albedoColor, float fragmentZ)
{
	#ifdef SHADERGRAPH_PREVIEW
		return 1;
	#else
		
		uint waterID = GetWaterID(uv);
		if (waterID == 0) return 0;

		float waterMask = GetWaterMask(uv);
		if (waterMask < 0.01) return 0;
		
		float transparent = KWS_TransparentArray[waterID];
		float3 waterColor = KWS_WaterColorArray[waterID];
		float turbidity = KWS_TurbidityArray[waterID];
		float3 turbidityColor = KWS_TurbidityColorArray[waterID];

		float fragmentEyeDepth = LinearEyeDepth(fragmentZ);
		float fade = max(0, fragmentEyeDepth * UNDERWATER_DEPTH_SCALE);
		#if KWS_USE_VOLUMETRIC_LIGHT
			half3 volumeScattering = GetVolumetricLight(uv).xyz;
		#else
			half4 volumeScattering = half4(GetAmbientColor() * GetExposure(), 1.0);
		#endif
		float3 underwaterColor = ComputeUnderwaterColor(volumeScattering.xyz * albedoColor, volumeScattering.rgb, fade, transparent, waterColor.xyz, turbidity, turbidityColor.xyz);


		return half4(underwaterColor, 1);
	#endif
}

inline float4 GetUnderwaterColorRefraction(float2 uv, float3 albedoColor, float2 refractionNormal, float fragmentZ)
{
	#ifdef SHADERGRAPH_PREVIEW
		return 1;
	#else
		float2 underwaterUV = clamp(uv + refractionNormal, 0.01, 0.99);
		
		uint waterID = GetWaterID(uv);
		if (waterID == 0) return 0;

		float waterMask = GetWaterMask(uv);
		if (waterMask < 0.01) return 0;
		
		float linearZ = LinearEyeDepth(GetSceneDepth(underwaterUV));
		float waterDepth = GetWaterDepth(underwaterUV);
		float depthSurface = LinearEyeDepth(waterDepth);
		half waterSurfaceMask = (depthSurface - linearZ) > 0;
		
		#if KWS_USE_VOLUMETRIC_LIGHT
			half3 volumeScattering = GetVolumetricLight(underwaterUV).xyz;
		#else
			half4 volumeScattering = half4(GetAmbientColor() * GetExposure(), 1.0);
		#endif

		float fade = max(0, min(depthSurface, linearZ)) * UNDERWATER_DEPTH_SCALE;
		half2 normal = GetWaterMaskScatterNormals(underwaterUV).yz * 2 - 1;
		half3 refraction = GetSceneColor(underwaterUV);
		albedoColor *= dot(volumeScattering.rgb, 0.33);
		refraction += albedoColor;

		float transparent = KWS_TransparentArray[waterID];
		float3 waterColor = KWS_WaterColorArray[waterID];
		float turbidity = KWS_TurbidityArray[waterID];
		float3 turbidityColor = KWS_TurbidityColorArray[waterID];

		float3 underwaterColorBeforeFragment = ComputeUnderwaterColor(refraction, volumeScattering.rgb, fade, transparent, waterColor.xyz, turbidity, turbidityColor.xyz);

		float fragmentEyeDepth = LinearEyeDepth(fragmentZ);
		fade = max(0, fragmentEyeDepth * UNDERWATER_DEPTH_SCALE);
		#if KWS_USE_VOLUMETRIC_LIGHT
			volumeScattering = GetVolumetricLight(uv).xyz;
		#endif
		underwaterColorBeforeFragment += albedoColor ;
		float3 underwaterColor = ComputeUnderwaterColor(underwaterColorBeforeFragment, volumeScattering.rgb, fade, transparent, waterColor.xyz, turbidity, turbidityColor.xyz);

		return half4(underwaterColor, 1);
	#endif
}


////////////////////////////// shadergraph support /////////////////////////////////////////////////////////////////////

inline void GetDecalVertexOffset_float(float3 worldPos, float displacement, out float3 result)
{
	#ifdef SHADERGRAPH_PREVIEW
		result = 0;
	#else
		result = worldPos + GetFftWavesDisplacement(GetAbsolutePositionWS(worldPos)) * saturate(float3(displacement, 1, displacement));
	#endif
}



inline void GetDecalDepthTest_float(float4 screenPos, out float result)
{
	#ifdef SHADERGRAPH_PREVIEW
		result = 1;
	#else
		float sceneDepth = GetSceneDepth(screenPos.xy / screenPos.w);
		result = LinearEyeDepth(sceneDepth) > LinearEyeDepth(screenPos.z / screenPos.w);
	#endif
}


inline void TileWarpParticlesOffsetXZ_float(float3 vertex, float3 center, out float3 result)
{
	result = TileWarpParticlesOffsetXZ(vertex, center);
}


inline void GetWaterSurfaceCollisionForQuadParticles_float(float3 vertex, float3 center, out float3 result)
{
	result = GetWaterSurfaceCollisionForQuadParticles(vertex, center);
}


void GetUnderwaterColorRefraction_float(float2 uv, float3 albedoColor, float2 refractionNormal, float fragmentZ, out float4 result) //shadergraph function

{
	result = GetUnderwaterColorRefraction(uv, albedoColor, refractionNormal, fragmentZ);
}


void GetUnderwaterColorAlbedo_float(float2 uv, float3 albedoColor, float fragmentZ, out float4 result) //shadergraph function

{
	result = GetUnderwaterColorAlbedo(uv, albedoColor, fragmentZ);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




#endif