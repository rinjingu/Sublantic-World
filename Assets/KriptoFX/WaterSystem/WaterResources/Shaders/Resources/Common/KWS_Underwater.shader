Shader "Hidden/KriptoFX/KWS/Underwater"
{
	Properties
	{
		[HideInInspector]KWS_StencilMaskValue ("KWS_StencilMaskValue", Int) = 32
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
	
		ZWrite Off

		Stencil
		{
			Ref [KWS_StencilMaskValue]
			ReadMask [KWS_StencilMaskValue]
			Comp Greater
			Pass keep
		}

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vertUnderwater
			#pragma fragment fragUnderwater
			#pragma target 4.6

			#pragma multi_compile _ KWS_USE_VOLUMETRIC_LIGHT
			#pragma multi_compile _ USE_PHYSICAL_APPROXIMATION_COLOR USE_PHYSICAL_APPROXIMATION_SSR
			#pragma multi_compile _ USE_HALF_LINE_TENSION

			#include "../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			
			DECLARE_TEXTURE(_SourceRT);
			float4 KWS_Underwater_RTHandleScale;
			float4 _SourceRTHandleScale;
			float4 _SourceRT_TexelSize;


			float MaskToAlpha(float mask)
			{
				return saturate(mask * mask * mask * 20);
			}

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


			vertexOutput vertUnderwater(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}

			half4 fragUnderwater(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 uv = i.uv;
				half mask = GetWaterMask(uv - float2(0, _SourceRT_TexelSize.y * 3));
				if (mask < 0.01) return 0;
				
				float z = GetSceneDepth(uv);

				float waterDepth = GetWaterDepth(uv - float2(0, KW_WaterDepth_TexelSize.y * 2));
				float linearZ = LinearEyeDepth(z);
				float depthSurface = LinearEyeDepth(waterDepth);
				half waterSurfaceMask = (depthSurface - linearZ) > 0;
				
				#if KWS_USE_VOLUMETRIC_LIGHT
					half4 volumeScattering = GetVolumetricLight(uv.xy);
					volumeScattering.w = saturate(volumeScattering.w * 100);
				#else
					half4 volumeScattering = half4(GetAmbientColor() * GetExposure(), 1);
				#endif
				
				float fade = max(0, min(depthSurface, linearZ)) * UNDERWATER_DEPTH_SCALE;
				
				half2 normal = GetWaterMaskScatterNormals(uv.xy).yz * 2 - 1;
				half3 refraction = GetSceneColor(lerp(uv.xy + normal, uv.xy, waterSurfaceMask));
				float verticalDepthFade = GetVerticalDepthFade(KWS_WaterInstanceID, GetCameraAbsolutePosition().y);
				refraction *= max(volumeScattering.a, verticalDepthFade);
				
				#if defined(USE_PHYSICAL_APPROXIMATION_COLOR) || defined(USE_PHYSICAL_APPROXIMATION_SSR)
					float3 waterWorldPos = GetWorldSpacePositionFromDepth(uv, waterDepth);
					float3 worldViewDir = GetWorldSpaceViewDirNorm(waterWorldPos);
					
					float3 refractedRay = Refract(-worldViewDir, float3(normal.x * 5, 1, normal.y * 5), 1.33);
					float3 refractedRay2 = Refract(-worldViewDir, float3(normal.x, 1, normal.y), 0.74);
					refractedRay.y *= 0.25; //fix information lost in the near camera
					float refractedMask = 1 - clamp(-refractedRay2.y * 100, 0, 1);
					float3 reflection = 0;
				#endif

				#ifdef USE_PHYSICAL_APPROXIMATION_SSR
					float4 refractedClipPos = mul(UNITY_MATRIX_VP, float4(GetCameraRelativePosition(waterWorldPos + refractedRay), 1.0));
					float4 refractionScreenPos = ComputeScreenPos(refractedClipPos);
					float2 refractedUV = refractionScreenPos.xy / refractionScreenPos.w;
					
					reflection = GetScreenSpaceReflection(refractedUV, waterWorldPos);
				#endif

				#ifdef USE_PHYSICAL_APPROXIMATION_COLOR
					reflection = ComputeUnderwaterSurfaceColor(volumeScattering.rgb, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
				#endif
				
				#if defined(USE_PHYSICAL_APPROXIMATION_COLOR) || defined(USE_PHYSICAL_APPROXIMATION_SSR)
					reflection = ComputeUnderwaterColor(reflection, volumeScattering.rgb, fade, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
					float3 internalReflection = lerp(reflection, refraction, waterSurfaceMask);
					refraction = lerp(refraction, internalReflection, refractedMask);
					//return float4(refraction, 1);
				#endif
				
				half3 underwaterColor = ComputeUnderwaterColor(refraction, volumeScattering.rgb, fade, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
				float alpha = mask > 0.5;
				//return half4(saturate(waterSurfaceMask.xxx), alpha);
				#if USE_HALF_LINE_TENSION
					half invertedHalfMask = 1 - saturate((mask * 2 - 1) * 10);
					half halfMask = saturate(invertedHalfMask * mask * 2 - 0.03);
					float2 halfLineUV = float2(uv.x, lerp(uv.y, halfMask, halfMask));
					half3 refractionHalfLine = GetSceneColor(halfLineUV);
					half3 underwaterHalfColor = ComputeUnderwaterColor(refractionHalfLine, volumeScattering.rgb, 0.25, max(2, KW_Transparent), KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
					underwaterHalfColor = lerp(refractionHalfLine, underwaterHalfColor, halfMask);
					underwaterColor = lerp(underwaterColor, underwaterHalfColor, invertedHalfMask);
					float halflineAlpha = max(waterSurfaceMask, halfMask);
					alpha = max(MaskToAlpha(mask), halflineAlpha);
				#endif

				return half4(underwaterColor, alpha);
			}

			ENDHLSL
		}
	}
}