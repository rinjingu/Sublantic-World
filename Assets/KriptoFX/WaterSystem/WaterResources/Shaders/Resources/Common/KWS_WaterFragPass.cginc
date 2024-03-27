half4 fragWater(v2fWater i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	float2 uv = i.worldPos.xz / KW_FFTDomainSize; //todo domain size = 0
	float2 screenUV = i.screenPos.xy / i.screenPos.w;
	float3 viewDir = GetWorldSpaceViewDirNorm(i.worldPosRefracted);
	float surfaceDepthZ = LinearEyeDepth(i.screenPos.z / i.screenPos.w);
	float sceneZ = GetSceneDepth(screenUV);
	half surfaceMask = KWS_UseWireframeMode == 1 || i.surfaceMask.x > 0.999;
	half exposure = GetExposure();
	
	/////////////////////////////////////////////////////////////  NORMAL  ////////////////////////////////////////////////////////////////////////////////////////////////////////

	float3 wavesNormalFoam;
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE) || defined(KW_FLOW_MAP_FLUIDS)
		wavesNormalFoam = GetFftWavesNormalFoamWithFlowmap(i.worldPos);
	#else
		wavesNormalFoam = GetFftWavesNormalFoam(i.worldPos);
	#endif
	float foam = wavesNormalFoam.y * surfaceMask;
	float3 tangentNormal = float3(wavesNormalFoam.x, 1, wavesNormalFoam.z);
	
	//return float4(foam.xxx, 1);

	
	#if defined(KW_FLOW_MAP_FLUIDS)
		half fluidsFoam;
		tangentNormal = GetFluidsNormal(i.worldPos, uv, tangentNormal, fluidsFoam);
		fluidsFoam *= surfaceMask;
	#endif


	#ifdef KW_FLOW_MAP_EDIT_MODE
		return GetFlowmapEditor(i.worldPos, tangentNormal);
	#endif

	#if USE_SHORELINE
		tangentNormal = ComputeShorelineNormal(tangentNormal, i.worldPos);
		//return float4(tangentNormal.xz, 0, 1);
	#endif

	
	#if KW_DYNAMIC_WAVES
		float3 dynamicWavesNormal = GetDynamicWavesNormals(i.worldPos);
		tangentNormal = KWS_BlendNormals(tangentNormal, dynamicWavesNormal);
	#endif

	//#if USE_FILTERING
	//	tangentNormal.xz *= normalFilteringMask;
	//#endif

	tangentNormal = lerp(float3(0, 1, 0), tangentNormal, surfaceMask);
	float3 worldNormal = KWS_BlendNormals(tangentNormal, i.worldNormal);

	/////////////////////////////////////////////////////////////  end normal  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	//return float4(tangentNormal.xz, 0, 1);

	

	/////////////////////////////////////////////////////////////////////  REFRACTION  ///////////////////////////////////////////////////////////////////
	float2 refractionUV;
	half3 refraction;

	UNITY_BRANCH
	if (KWS_UseRefractionIOR > 0) refractionUV = GetRefractedUV_IOR(viewDir, worldNormal, i.worldPos, sceneZ, surfaceDepthZ);
	else refractionUV = lerp(screenUV, GetRefractedUV_Simple(screenUV, worldNormal), surfaceMask);

	UNITY_BRANCH
	if(KWS_UseRefractionDispersion > 0)	refraction = GetSceneColorWithDispersion(refractionUV, KWS_RefractionDispersionStrength);
	else refraction = GetSceneColor(refractionUV);

	/////////////////////////////////////////////////////////////  end refraction  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	


	/////////////////////////////////////////////////////////////  REFLECTION  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	float3 reflDir = reflect(-viewDir, worldNormal);
	half3 finalReflection = KWS_GetSkyColor(reflDir, KWS_SkyLodRelativeToWind, exposure);

	#if KWS_SSR_REFLECTION
		float2 refl_uv = GetScreenSpaceReflectionUV(reflDir);
		half4 ssrRefl = GetScreenSpaceReflectionWithStretchingMask(refl_uv, i.worldPos);
	
		#if defined(PLANAR_REFLECTION) 
			finalReflection = ssrRefl.xyz;
		#else 
			finalReflection = lerp(finalReflection, ssrRefl.xyz, ssrRefl.a);
		#endif
	#endif

	#if !defined(KWS_SSR_REFLECTION) && defined(PLANAR_REFLECTION)
		//UNITY_BRANCH
		//if(KWS_WaterInstanceID == (uint)KWS_PlanarReflectionInstanceID) //todo unity dont update KWS_PlanarReflectionInstanceID, wtf?
		{	
			float2 refl_uv = GetScreenSpaceReflectionUV(reflDir);
			finalReflection = GetPlanarReflectionWithClipOffset(refl_uv) * exposure;
		}
	#endif

	finalReflection *= surfaceMask;
	/////////////////////////////////////////////////////////////  end reflection  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	//return float4(finalReflection, 1);
	
	
	/////////////////////////////////////////////////////////////////////  UNDERWATER  ///////////////////////////////////////////////////////////////////
	#if KWS_USE_VOLUMETRIC_LIGHT
		half4 volumeScattering = GetVolumetricLight(refractionUV);
	#else
		half4 volumeScattering = half4(GetAmbientColor() * exposure, 1.0);
	#endif
	
	float depthAngleFix = (surfaceMask < 0.5 || KWS_MeshType == KWS_MESH_TYPE_CUSTOM_MESH) ?        0.25 : saturate(GetWorldSpaceViewDirNorm(i.worldPos - float3(0, KWS_WindSpeed * 0.5, 0)).y);
	float refractedSceneZ = GetSceneDepth(refractionUV);
	float fade = GetWaterRawFade(i.worldPos, surfaceDepthZ, refractedSceneZ, surfaceMask, depthAngleFix);
	FixAboveWaterRendering(refractionUV, refractedSceneZ, i.worldPos, sceneZ, surfaceDepthZ, depthAngleFix, screenUV, surfaceMask, fade, refraction, volumeScattering);
	
	half3 underwaterColor = ComputeUnderwaterColor(refraction.xyz, volumeScattering.rgb, fade, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
	
	#if defined(KW_FLOW_MAP_FLUIDS)
		underwaterColor = GetFluidsColor(underwaterColor, volumeScattering, fluidsFoam);
	#endif
	underwaterColor += ComputeSSS(screenUV, underwaterColor, volumeScattering.a > 0.5, KW_Transparent) * 5;

	UNITY_BRANCH
	if(KWS_UseIntersectionFoam) underwaterColor = ComputeIntersectionFoam(underwaterColor, worldNormal, fade, i.worldPos, GetVolumetricLight(screenUV).a, KW_WaterColor.xyz, exposure);
	
	UNITY_BRANCH
	if(KWS_UseOceanFoam) underwaterColor = ComputeOceanFoam(foam, worldNormal, underwaterColor, fade, i.worldPos, GetVolumetricLight(screenUV).a, KW_WaterColor.xyz, exposure);
	
	/////////////////////////////////////////////////////////////  end underwater  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	

	#if USE_SHORELINE
		finalReflection = ApplyShorelineWavesReflectionFix(reflDir, finalReflection, underwaterColor);
	#endif
	
	half waterFresnel = ComputeWaterFresnel(worldNormal, viewDir);
	waterFresnel *= surfaceMask;
	half3 finalColor = lerp(underwaterColor, finalReflection, waterFresnel);
	
	#if REFLECT_SUN
		finalColor += ComputeSunlight(worldNormal, viewDir, GetMainLightDir(), GetMainLightColor() * exposure, volumeScattering.a, surfaceDepthZ, _ProjectionParams.z, KW_Transparent);
		//finalColor += sunReflection * (1 - fogOpacity);
	#endif

	
	half3 fogColor;
	half3 fogOpacity;
	GetInternalFogVariables(i.pos, viewDir, surfaceDepthZ, i.screenPos.z, fogColor, fogOpacity);
	finalColor = ComputeInternalFog(finalColor, fogColor, fogOpacity);
	finalColor = ComputeThirdPartyFog(finalColor, i.worldPos, screenUV, i.screenPos.z);

	if (KWS_UseWireframeMode) finalColor = ComputeWireframe(i.surfaceMask.xyz, finalColor);
	
	half surfaceTensionFade = GetSurfaceTension(sceneZ, surfaceDepthZ);
	return float4(finalColor, surfaceTensionFade);
}

struct FragmentOutput
{
	half4 dest0 : SV_Target0;
	half dest1 : SV_Target1;
};

FragmentOutput fragDepth(v2fDepth i, float facing : VFACE)
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	FragmentOutput o;

	float facingColor = 0.75 - facing * 0.25;
	i.screenPos.xyz /= i.screenPos.w;
	float sceneDepth = GetSceneDepth(i.screenPos.xy);

	#ifndef DISABLE_DEPTH_TEST
		if (KWS_UnderwaterVisible == 0 && LinearEyeDepth(sceneDepth) < LinearEyeDepth(i.screenPos.z))
		{
			//o.dest0 = float4(facing > 0.5 ? 0.0 : facingColor, 0, 0, 0);
			//o.dest1 = facing > 0.5 ? 0 : KWS_WaterInstanceID * KWS_WATER_MASK_DECODING_VALUE;
			o.dest0 = float4(0, 0, 0, 0);
			o.dest1 = 0;
			return o;
		}
	#endif

	float3 worldPos = i.worldPos_LocalHeight.xyz;
	float waveLocalHeight = i.worldPos_LocalHeight.w;
	float2 uv = worldPos.xz / KW_FFTDomainSize;
	

	float3 wavesNormalFoam;
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
		wavesNormalFoam = GetFftWavesNormalFoamWithFlowmap(worldPos);
	#else
		wavesNormalFoam = GetFftWavesNormalFoam(worldPos);
	#endif
	float3 tangentNormal = float3(wavesNormalFoam.x, 1, wavesNormalFoam.z); 

	float3 tangentNormalScatter = GetFftWavesNormalLod(worldPos, KWS_WATER_SSR_NORMAL_LOD);

	#if KW_DYNAMIC_WAVES
		float3 dynamicWavesNormal = GetDynamicWavesNormals(worldPos);
		tangentNormal = KWS_BlendNormals(tangentNormal, dynamicWavesNormal);
	#endif
	
	tangentNormal.xz *= i.surfaceMask.x > 0.999 ?       1 : 0;

	float3 worldNormal = KWS_BlendNormals(tangentNormal, i.worldNormal);
	float3 worldNormalScatter = KWS_BlendNormals(tangentNormalScatter, i.worldNormal);


	int idx;
	half sss = 0;
	half windLimit = clamp((KWS_WindSpeed - 1), 0, 1);
	windLimit = lerp(windLimit, windLimit * 0.25, saturate(KWS_WindSpeed / 15.0));

	float3 viewDir = GetWorldSpaceViewDirNorm(worldPos);
	float3 lightDir = GetMainLightDir();
	float distanceToCamera = 1 - saturate(GetWorldToCameraDistance(worldPos) * 0.002);
	
	half zeroScattering = saturate(dot(viewDir, - (lightDir + float3(0, 1, 0))));

	float3 H = (lightDir + worldNormalScatter * float3(-1, 1, -1));
	float scattering = pow(saturate(dot(viewDir, -H)), 3);
	sss += windLimit * (scattering - zeroScattering * 0.95);


	float underwaterMask = saturate(-facing);

	if(KWS_MeshType == KWS_MESH_TYPE_RIVER && i.surfaceMask.x < 0.999) underwaterMask = 1;
	o.dest0 = half4(underwaterMask, worldNormal.xz * 0.5 + 0.5, saturate(scattering * waveLocalHeight * distanceToCamera * windLimit));
	o.dest1 = (KWS_WaterInstanceID * KWS_WATER_MASK_DECODING_VALUE);
	
	return o;
}