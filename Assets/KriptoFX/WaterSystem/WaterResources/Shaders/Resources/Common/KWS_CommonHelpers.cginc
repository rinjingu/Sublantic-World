#ifndef KWS_COMMON_HELPERS
#define KWS_COMMON_HELPERS

uint Pack_R11G11B10f(float3 rgb)
{
	uint r = (f32tof16(rgb.x) << 17) & 0xFFE00000;
	uint g = (f32tof16(rgb.y) << 6) & 0x001FFC00;
	uint b = (f32tof16(rgb.z) >> 5) & 0x000003FF;
	return r | g | b;
}

float3 Unpack_R11G11B10f(uint rgb)
{
	float r = f16tof32((rgb >> 17) & 0x7FF0);
	float g = f16tof32((rgb >> 6) & 0x7FF0);
	float b = f16tof32((rgb << 5) & 0x7FE0);
	return float3(r, g, b);
}


float KWS_Noise13(float3 p3)
{
	p3  = frac(p3 * .1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return frac((p3.x + p3.y) * p3.z);
}

float InterleavedGradientNoise(float2 pixel, float frame) 
{
	frame = frame % 64;
    pixel += (frame * 5.588238f);
    return frac(52.9829189f * frac(0.06711056f*pixel.x + 0.00583715f*pixel.y));  
}

inline float3 KWS_BlendNormals(float3 n1, float3 n2)
{
	return normalize(float3(n1.x + n2.x, n1.y * n2.y, n1.z + n2.z));
}

inline float3 KWS_BlendNormals(float3 n1, float3 n2, float3 n3)
{
	return normalize(float3(n1.x + n2.x + n3.x, n1.y * n2.y * n3.y, n1.z + n2.z + n3.z));
}


float KWS_Pow5(float x)
{
	return x * x * x * x * x;
}


float2 GetRTHandleUV(float2 UV, float2 texelSize, float numberOfTexels, float2 scale)
{
    float2 maxCoord = 1.0f - numberOfTexels * texelSize;
    return min(UV, maxCoord) * scale;
}

float2 GetRTHandleUVBilinear(float2 UV, float4 texelSize, float numberOfTexels, float2 scale)
{
	float2 maxCoord = 1.0f - numberOfTexels * texelSize.xy;
    UV = min(UV, maxCoord);
	return floor(UV * scale * texelSize.zw) * texelSize.xy;
}


// filtering

inline float4 Texture2DSampleAA(Texture2D tex, SamplerState state, float2 uv)
{
	half4 color = tex.Sample(state, uv.xy);

	float2 uv_dx = ddx(uv);
	float2 uv_dy = ddy(uv);

	color += tex.Sample(state, uv.xy + (0.25) * uv_dx + (0.75) * uv_dy);
	color += tex.Sample(state, uv.xy + (-0.25) * uv_dx + (-0.75) * uv_dy);
	color += tex.Sample(state, uv.xy + (-0.75) * uv_dx + (0.25) * uv_dy);
	color += tex.Sample(state, uv.xy + (0.75) * uv_dx + (-0.25) * uv_dy);

	color /= 5.0;

	return color;
}

float4 cubic(float v) {
	float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
	float4 s = n * n * n;
	float x = s.x;
	float y = s.y - 4.0 * s.x;
	float z = s.z - 4.0 * s.y + 6.0 * s.x;
	float w = 6.0 - x - y - z;
	return float4(x, y, z, w) * (1.0 / 6.0);
}


inline float4 Texture2DArraySampleBicubic(Texture2DArray tex, SamplerState state, float2 uv, float4 texelSize, float idx)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.Sample(state, float3(offset.xz, idx));
	half4 sample1 = tex.Sample(state, float3(offset.yz, idx));
	half4 sample2 = tex.Sample(state, float3(offset.xw, idx));
	half4 sample3 = tex.Sample(state, float3(offset.yw, idx));

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

inline float4 Texture2DArraySampleLevelBicubic(Texture2DArray tex, SamplerState state, float2 uv, float4 texelSize, float idx, float level)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.SampleLevel(state, float3(offset.xz, idx), level);
	half4 sample1 = tex.SampleLevel(state, float3(offset.yz, idx), level);
	half4 sample2 = tex.SampleLevel(state, float3(offset.xw, idx), level);
	half4 sample3 = tex.SampleLevel(state, float3(offset.yw, idx), level);

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

inline float4 Texture2DSampleLevelBicubic(Texture2D tex, SamplerState state, float2 uv, float4 texelSize, float level)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.SampleLevel(state, offset.xz, level);
	half4 sample1 = tex.SampleLevel(state, offset.yz, level);
	half4 sample2 = tex.SampleLevel(state, offset.xw, level);
	half4 sample3 = tex.SampleLevel(state, offset.yw, level);

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

inline float4 Texture2DSampleBicubic(Texture2D tex, SamplerState state, float2 uv, float4 texelSize)
{
	uv = uv * texelSize.zw - 0.5;
	float2 fxy = frac(uv);
	uv -= fxy;

	float4 xcubic = cubic(fxy.x);
	float4 ycubic = cubic(fxy.y);

	float4 c = uv.xxyy + float2(-0.5, +1.5).xyxy;
	float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
	offset *= texelSize.xxyy;

	half4 sample0 = tex.Sample(state, offset.xz);
	half4 sample1 = tex.Sample(state, offset.yz);
	half4 sample2 = tex.Sample(state, offset.xw);
	half4 sample3 = tex.Sample(state, offset.yw);

	float sx = s.x / (s.x + s.y);
	float sy = s.z / (s.z + s.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

inline float4 Texture2DSampleBilinear(Texture2D tex, SamplerState state, float2 uv, float4 texelSize)
{
	uv = uv * texelSize.zw + 0.5;
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);
	uv = iuv + fuv * fuv * (3.0 - 2.0 * fuv); // fuv*fuv*fuv*(fuv*(fuv*6.0-15.0)+10.0);;
	uv = (uv - 0.5) / texelSize.zw;
	return tex.Sample(state, uv);
}


inline bool IsOutsideUV(float2 uv)
{
	return uv.x < 0.001 || uv.x > 0.999 || uv.y < 0.001 || uv.y > 0.999;
}

float3 random3(float3 c) {
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0 * j);
    j *= 0.125;
    r.x = frac(512.0 * j);
    j *= 0.125;
    r.y = frac(512.0 * j);
    return r - 0.5;
}

// Skew constants for 3D simplex functions


// 3D simplex noise
float simplex3d(float3 p)
{
	static const float F3 = 0.3333333;
	static const float G3 = 0.1666667;

    // Find current tetrahedron T and its four vertices
    float3 s = floor(p + dot(p, F3));
    float3 x = p - s + dot(s, G3);

    // Calculate i1 and i2
    float3 e = step(0, x - x.yzx);
    float3 i1 = e * (1.0 - e.zxy);
    float3 i2 = 1.0 - e.zxy * (1.0 - e);

    // Calculate x1, x2, x3
    float3 x1 = x - i1 + G3;
    float3 x2 = x - i2 + 2.0 * G3;
    float3 x3 = x - 1.0 + 3.0 * G3;

    // Find four surflets and store them in d
    float4 w;
    float4 d;

    // Calculate surflet weights
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);

    // w fades from 0.6 at the center of the surflet to 0.0 at the margin
    w = max(0.6 - w, 0.0);

    // Calculate surflet components
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);

    // Multiply d by w^4
    w *= w;
    w *= w;
    d *= w;

    // Return the sum of the four surflets
    return clamp(dot(d, float4(52.0, 52.0, 52.0, 52.0)), -1, 1) * 0.5 + 0.5;
}


// end filtering
#endif