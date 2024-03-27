Shader "Hidden/KriptoFX/KWS/CausticDecal"
{
	Properties
	{
		[HideInInspector]KWS_StencilMaskValue("KWS_StencilMaskValue", Int) = 32
	}

	Subshader
	{
		ZWrite Off
		Cull Front

		ZTest Always
		Blend DstColor SrcColor
		//Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref [KWS_StencilMaskValue]
            ReadMask [KWS_StencilMaskValue]
            //WriteMask [KWS_StencilMaskValue]
			Comp Greater
			Pass keep
		}

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#define SHORELINE_CAUSTIC_STRENGTH 0.1
			#define DYNAMIC_WAVES_CAUSTIC_STRENGTH_NORMALS 0.5
			#define DYNAMIC_WAVES_CAUSTIC_STRENGTH_HEIGHT 1
			#define CURVEDWORLD_DISABLED_ON

			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ KW_FLOW_MAP
			#pragma multi_compile _ USE_DISPERSION

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

			float KWS_CaustisStrength;

			bool GetClipFade(float3 worldPos)
			{
				float3 localPos = WorldToLocalPos(worldPos);
				float3 fadeByAxis = saturate(0.5 - abs(localPos));
				float clipFade = fadeByAxis.x * fadeByAxis.y * fadeByAxis.z;
				return clipFade;
			}

			float GetDepthFade(float depth, float2 screenUV)
			{
				float terrainFade =  saturate(LinearEyeDepth(depth) - LinearEyeDepth(GetWaterDepth(screenUV)));
				return lerp(terrainFade, 1 - terrainFade, GetWaterMask(screenUV));
			}

			half3 GetCaustic(float depth, float2 screenUV, float3 worldPos, uint waterID)
			{
				float2 orthoDepthUV = GetWaterOrthoDepthUV(worldPos);
				//float terrainDepth = max(0, -GetWaterOrthoDepth(orthoDepthUV));

				float domainSize = GetDomainSize(1);
				float2 causticUV = (worldPos.xz / domainSize);

				float3 caustic = 0;

				#ifdef USE_SHORELINE
					float3 shorelineDisplacement = GetShorelineDisplacement(worldPos);
					causticUV -= shorelineDisplacement.xz * SHORELINE_CAUSTIC_STRENGTH;
				#endif
				#ifdef KW_DYNAMIC_WAVES
					float3 dynamicWavesNormals = GetDynamicWavesNormals(worldPos);
					causticUV -= dynamicWavesNormals.xz * DYNAMIC_WAVES_CAUSTIC_STRENGTH_NORMALS;
				#endif

				#ifdef KW_FLOW_MAP 
					caustic = GetCausticWithFlowmap(causticUV, worldPos, waterID, domainSize);
				#else
					caustic = GetCaustic(causticUV, waterID);
				#endif

				#ifdef KW_DYNAMIC_WAVES
					float dynamicWavesHeight = GetDynamicWavesDisplacement(worldPos);
					caustic += dynamicWavesHeight * DYNAMIC_WAVES_CAUSTIC_STRENGTH_HEIGHT;
				#endif

				float depthFade = GetDepthFade(depth, screenUV);
				caustic = lerp(float3(0, 0, 0), caustic - KWS_CAUSTIC_MULTIPLIER, depthFade);

				half verticalDepth = lerp(1, GetVerticalDepthFade(waterID, worldPos.y), KWS_UnderwaterVisible); 
				caustic = lerp(float3(0, 0, 0), caustic, verticalDepth);

				return max(-KWS_CAUSTIC_MULTIPLIER, caustic * KWS_CaustisStrength);
			}

			struct vertexInput
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct vertexOutput
			{
				float4 vertex : SV_POSITION;
				float4 screenUV : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = ObjectToClipPos(v.vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 screenUV = i.screenUV.xy / i.screenUV.w;
 				uint waterID = GetWaterID(screenUV);
				if (waterID != KWS_WaterInstanceID) discard;

				float depth = GetSceneDepth(screenUV);
				float3 worldPos = GetWorldSpacePositionFromDepth(screenUV, depth);
				float clipFade = GetClipFade(worldPos);
				if(clipFade < 0.01) return float4(0.5, 0.5, 0.5, 0.5);
				
				half3 caustic = GetCaustic(depth, screenUV, worldPos, waterID);
			
				half3 fogColor;
				half3 fogOpacity;
				float linearDepth = LinearEyeDepth(depth);
				GetInternalFogVariables(i.vertex, 0, linearDepth, linearDepth, fogColor, fogOpacity);
			
				
				caustic = lerp(float3(0.5, 0.5, 0.5) + caustic, float3(0.5, 0.5, 0.5), fogOpacity);
				return float4(caustic, 1);
			}

			ENDHLSL
		}
	}
}