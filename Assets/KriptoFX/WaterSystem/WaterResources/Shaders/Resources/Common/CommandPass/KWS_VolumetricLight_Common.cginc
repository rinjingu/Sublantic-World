#define CAUSTIC_LOD 4

uint KWS_Frame;
float KWS_VolumetricLightTemporalAccumulationFactor;
float2 KWS_VolumetricLightDownscaleFactor;

half MaxDistance;
uint KWS_RayMarchSteps;
half4 KWS_LightAnisotropy;

float KWS_VolumeLightMaxDistance;
float KWS_VolumeDepthFade;


static const float ditherPattern[8][8] =
{

	{
		0.012f, 0.753f, 0.200f, 0.937f, 0.059f, 0.800f, 0.243f, 0.984f
	},
	{
		0.506f, 0.259f, 0.690f, 0.443f, 0.553f, 0.306f, 0.737f, 0.490f
	},
	{
		0.137f, 0.875f, 0.075f, 0.812f, 0.184f, 0.922f, 0.122f, 0.859f
	},
	{
		0.627f, 0.384f, 0.569f, 0.322f, 0.675f, 0.427f, 0.612f, 0.369f
	},
	{
		0.043f, 0.784f, 0.227f, 0.969f, 0.027f, 0.769f, 0.212f, 0.953f
	},
	{
		0.537f, 0.290f, 0.722f, 0.475f, 0.522f, 0.275f, 0.706f, 0.459f
	},
	{
		0.169f, 0.906f, 0.106f, 0.843f, 0.153f, 0.890f, 0.090f, 0.827f
	},
	{
		0.659f, 0.412f, 0.600f, 0.353f, 0.643f, 0.400f, 0.584f, 0.337f
	},
};

struct RaymarchData
{
	float2 uv;
	float stepSize;
	float3 step;
	float offset;
	float3 sceneWorldPos;
	float3 currentPos;
	float3 rayStart;
	float3 rayDir;
	float rayLength;
	float transparent;
	bool isUnderwater;
	float2 causticStrength;
	uint waterID;
};

struct vertexInput
{
	uint vertexID : SV_VertexID;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};


vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.vertex = GetTriangleVertexPosition(v.vertexID);
	o.uv = GetTriangleUVScaled(v.vertexID);
	return o;
}

//float GetLowResSceneDepth(float2 uv)
//{
//	float4 depth;
//	depth.x = GetSceneDepth(uv, float2(-KWS_VolumetricLightDownscaleFactor.x, 0));
//	depth.y = GetSceneDepth(uv, float2(KWS_VolumetricLightDownscaleFactor.x, 0));
//	depth.z = GetSceneDepth(uv, float2(0, KWS_VolumetricLightDownscaleFactor.y));
//	depth.w = GetSceneDepth(uv, float2(0, -KWS_VolumetricLightDownscaleFactor.y));

//	return min(depth.x, min(depth.y, min(depth.z, depth.w)));
//}


float3 FrustumRay(float2 uv, float4 frustumRays[4])
{
	float3 ray0 = lerp(frustumRays[0].xyz, frustumRays[1].xyz, uv.x);
	float3 ray1 = lerp(frustumRays[2].xyz, frustumRays[3].xyz, uv.x);
	return lerp(ray0, ray1, uv.y);
}


inline half MieScattering(float cosAngle)
{
	return KWS_LightAnisotropy.w * (KWS_LightAnisotropy.x / (KWS_LightAnisotropy.y - KWS_LightAnisotropy.z * cosAngle));
}

inline float GetMaxRayDistanceRelativeToTransparent(float transparent)
{ 
    return min(KWS_MAX_TRANSPARENT, max(0.3f, transparent * 3));
}

half RaymarchCaustic(RaymarchData raymarchData, float3 currentPos, float3 lightForward)
{
	float angle = dot(float3(0, -0.999, 0), lightForward);
	float vericalOffset = GetCameraAbsolutePosition().y * 0.5 * raymarchData.isUnderwater;
	float offsetLength = (raymarchData.rayStart.y - currentPos.y - vericalOffset) / angle;
	float2 uv = (currentPos.xz - offsetLength * lightForward.xz) / GetDomainSize(1, raymarchData.waterID);
	half caustic = GetCausticLod(uv, raymarchData.waterID, CAUSTIC_LOD) - KWS_CAUSTIC_MULTIPLIER;
	
	float causticOverScale = saturate(raymarchData.transparent * 0.2);
	caustic *= lerp(causticOverScale * raymarchData.causticStrength.x, raymarchData.causticStrength.y * 10, raymarchData.isUnderwater);

	float distanceToCamera = GetWorldToCameraDistance(currentPos);
	caustic = lerp(caustic, 0, saturate(distanceToCamera / 500));
	return caustic;
}


RaymarchData InitRaymarchData(vertexOutput i, float depthTop, float depthBot, bool isUnderwater, uint waterID)
{
	RaymarchData data;
	data.waterID = waterID;
	data.transparent = KWS_TransparentArray[waterID];
	data.causticStrength = KWS_VolumetricCausticParamsArray[waterID];
	data.rayLength = GetMaxRayDistanceRelativeToTransparent(data.transparent);
	data.isUnderwater = isUnderwater;

	float3 topPos = GetWorldSpacePositionFromDepth(i.uv, depthTop);
	float3 botPos = GetWorldSpacePositionFromDepth(i.uv, depthBot);
	
	data.sceneWorldPos = botPos;

	UNITY_BRANCH
	if (data.isUnderwater)
	{
		float3 camPos = GetCameraAbsolutePosition();
		data.rayStart = camPos;
		data.rayDir = normalize(botPos - camPos);
		data.rayLength = min(length(camPos - botPos), data.rayLength);
		data.rayLength = min(length(camPos - topPos), data.rayLength);
	}
	else
	{
		data.rayDir = normalize(botPos - topPos);
		data.rayLength = min(length(topPos - botPos), data.rayLength);
		data.rayStart = topPos;
	}
	float2 ditherScreenPos = i.vertex.xy % 8;
	
	data.stepSize = data.rayLength / KWS_RayMarchSteps;
	data.step = data.rayDir * data.stepSize;
	data.offset = InterleavedGradientNoise(i.vertex.xy, KWS_Frame);
	
	data.currentPos = data.rayStart + data.step * data.offset;
	data.uv = i.uv;
	
	return data;
}

void AddTemporalAccumulation(float3 worldPos, inout float3 color)
{
	if (KWS_Frame > 5)
	{
		float2 reprojectedUV = WorldPosToScreenPosReprojectedPrevFrame(worldPos, 0).xy;
		float3 lastColor = GetVolumetricLightLastFrame(reprojectedUV).xyz;
		color.xyz = lerp(color, lastColor, KWS_VolumetricLightTemporalAccumulationFactor);
	}
}