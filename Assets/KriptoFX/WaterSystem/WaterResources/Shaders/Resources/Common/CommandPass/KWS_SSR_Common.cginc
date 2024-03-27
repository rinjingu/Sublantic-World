#define MaxUint 4294967295u
#define MaxHalf 65504.0
#define STRETCH_THRESHOLD 0.75
#define MAX_HEIGHT_STRETCH_METERS 100

float4 _RTSize;
float KWS_AverageInstancesHeight;
uint _DepthHolesFillDistance;
uint KWS_ReprojectedFrameReady;
uint UseScreenSpaceReflectionSky;
float KWS_ScreenSpaceBordersStretching;

float KWS_ActiveSsrInstances[MAX_WATER_INSTANCES];

#if defined(STEREO_INSTANCING_ON)
	RWTexture2DArray<float4> ColorRT;
	RWTexture2DArray<float4> KWS_LastTargetRT;
	#define GetTextureID(id) uint3(id, unity_StereoEyeIndex)
	#define GetBufferOffset(stereoIdx)  _RTSize.x * _RTSize.y * stereoIdx
	#define SetStereoIndex(x) unity_StereoEyeIndex = x
#else
	RWTexture2D<float4> ColorRT;
	RWTexture2D<float4> KWS_LastTargetRT;
	#define GetTextureID(id) id
	#define GetBufferOffset(stereoIdx) 0
	#define SetStereoIndex(x)
#endif
RWStructuredBuffer<uint> HashRT;

StructuredBuffer<float> KWS_WaterSurfaceHeights;
#define DiscardIfOutOfBorder(id) uint2 rtSize = _RTSize.xy; if (id.x > rtSize.x || id.y > rtSize.y) return;

#if defined(STEREO_INSTANCING_ON)

	#define STEREO_LOOP for (int stereoIdx = 0; stereoIdx <= 1; stereoIdx++) { SetStereoIndex(stereoIdx);
	#define END_STEREO_LOOP }
	#define CONTINUTE_STEREO_LOOP continue;
#else
	#define STEREO_LOOP
	#define END_STEREO_LOOP
	#define CONTINUTE_STEREO_LOOP return;
#endif

half ComputeUVFade(float2 screenUV)
{
	UNITY_BRANCH
	if (screenUV.x <= 0.001 || screenUV.x > 0.999 || screenUV.y < 0.001 || screenUV.y > 0.999) return 0;
	else
	{
		float fringeY = 1 - screenUV.y;
		float fringeX = fringeY * (1 - abs(screenUV.x * 2 - 1)) * 300;
		fringeY = fringeY * 10;
		return saturate(fringeY) * saturate(fringeX);
	}
}

///////////////////////////////////////////////////////////////////////////////// kernels ////////////////////////////////////////////////////////////////////////////////////////


[numthreads(8, 8, 1)]
void Clear_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	SetStereoIndex(id.z);
	uint hashOffset = GetBufferOffset(id.z);

	uint val = MaxUint;
	#if defined(USE_HOLES_FILLING) && !defined(USE_UNDERWATER_REFLECTION)
		if (KWS_ReprojectedFrameReady == 1)
		{
			float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;
			float3 worldPos = GetWorldSpacePositionFromDepth(screenUV, 0);
			float3 reflectPosWS = worldPos;
			reflectPosWS.y = -reflectPosWS.y + 2 * KWS_AverageInstancesHeight;
			float2 reflectUV = WorldPosToScreenPos(reflectPosWS).xy;
			if (reflectUV.x < 0.001 || reflectUV.x > 0.999 || reflectUV.y < 0.001 || reflectUV.y > 0.999) val = MaxUint - 1;
		}
	#endif

	HashRT[id.y * _RTSize.x + id.x + hashOffset] = val;
}



[numthreads(8, 8, 1)]
void RenderHash_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	//SetStereoIndex(id.z);
	float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;
	
	uint waterID = KWS_ActiveSsrInstances[id.z];
	float waterHeight = KWS_WaterPositionArray[waterID].y;
	
	for (int stereoIdx = 0; stereoIdx <= 1; stereoIdx++)
	{
		SetStereoIndex(stereoIdx);

		float depth = GetSceneDepth(screenUV);
		float3 posWS = GetWorldSpacePositionFromDepth(screenUV, depth);
		
		#ifdef USE_UNDERWATER_REFLECTION
			half underwaterMask = GetWaterMask(screenUV);

			UNITY_BRANCH
			if (underwaterMask > 0.01)
			{
				if (posWS.y > waterHeight) CONTINUTE_STEREO_LOOP
			}
			else
			{
				if (posWS.y <= waterHeight) CONTINUTE_STEREO_LOOP
			}
			
		#else
			if (posWS.y <= waterHeight) CONTINUTE_STEREO_LOOP
		#endif

		float3 reflectPosWS = posWS;
		reflectPosWS.y = -reflectPosWS.y + 2 * waterHeight;
		float2 reflectUV = WorldPosToScreenPos(reflectPosWS).xy;

		float HeightStretch = min(posWS.y - waterHeight, MAX_HEIGHT_STRETCH_METERS);
		float AngleStretch = saturate(-KWS_CameraForward.y);
		float ScreenStretch = saturate(abs(reflectUV.x * 2 - 1) - STRETCH_THRESHOLD);
		float uvOffset = HeightStretch * AngleStretch * ScreenStretch * KWS_ScreenSpaceBordersStretching;
		reflectUV.x = reflectUV.x * (1 + uvOffset * 2) - uvOffset;

		if (reflectUV.x < 0.005 || reflectUV.x > 0.995 || reflectUV.y < 0.005 || reflectUV.y > 0.995) CONTINUTE_STEREO_LOOP
		
		uint reprojectedWaterID = GetWaterID(reflectUV);
		if (reprojectedWaterID > 0 && reprojectedWaterID != waterID) CONTINUTE_STEREO_LOOP

		uint2 reflectedScreenID = reflectUV * _RTSize.xy;
		uint hash = id.y << 16 | id.x;
		if (UseScreenSpaceReflectionSky == 0 && depth < 0.0000001) hash = MaxUint - 1;
		uint hashOffset = GetBufferOffset(stereoIdx);

		InterlockedMin(HashRT[reflectedScreenID.y * _RTSize.x + reflectedScreenID.x + hashOffset], hash);
	}
}

[numthreads(8, 8, 1)]
void RenderColorFromHash_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	SetStereoIndex(id.z);

	uint hashOffset = GetBufferOffset(id.z);
	uint hashIdx = id.y * _RTSize.x + id.x;
	
	uint left = HashRT[hashIdx + 1 + hashOffset].x;
	uint right = HashRT[hashIdx - 1 + hashOffset].x;
	uint down = HashRT[(id.y + 1) * _RTSize.x + id.x + hashOffset].x;
	uint up = HashRT[(id.y - 1) * _RTSize.x + id.x + hashOffset].x;

	uint hashData = min(left, min(right, min(up, down)));

	float4 earlyOutColor = 0;

	float exposure = GetExposure();
	float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;
	float3 worldPos = GetWorldSpacePositionFromDepth(screenUV, 0.0);
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos) * float3(-1, 1, -1);

	float3 fallBackColor = KWS_GetSkyColor(viewDir, KWS_SkyLodRelativeToWind, exposure);
	#if PLANAR_REFLECTION
		//if(GetWaterID(screenUV) == (uint)KWS_PlanarReflectionInstanceID) //todo unity dont update KWS_PlanarReflectionInstanceID, wtf?
		fallBackColor = GetPlanarReflectionRaw(screenUV) * exposure;
	#endif

	if (hashData >= MaxUint - 1)
	{
		#if defined(USE_HOLES_FILLING) && !defined(USE_UNDERWATER_REFLECTION)
			if (KWS_ReprojectedFrameReady == 1)
			{
				float2 reprojectedUV = WorldPosToScreenPosReprojectedPrevFrame(worldPos, 0).xy;
				float4 lastColor = KWS_LastTargetRT[GetTextureID(reprojectedUV * _RTSize.xy)].xyzw;
				if (hashData == MaxUint && lastColor.x + lastColor.y + lastColor.z > 0.0001) earlyOutColor = lastColor;
			}
		#endif

		earlyOutColor.xyz = lerp(fallBackColor, earlyOutColor.xyz, earlyOutColor.w);
		ColorRT[GetTextureID(id.xy)] = earlyOutColor;
		return;
	}

	uint2 sampleID = uint2(hashData & 0xFFFF, hashData >> 16);
	float2 sampleUV = (sampleID.xy) * _RTSize.zw + _RTSize.zw * 0.5;

	float fade = ComputeUVFade(sampleUV);
	half3 sampledColor = GetSceneColorPoint(sampleUV);
	half4 finalColor = half4(sampledColor, fade);
	
	finalColor.xyz = lerp(fallBackColor, finalColor.xyz, fade);
	ColorRT[GetTextureID(id.xy)] = finalColor;
}