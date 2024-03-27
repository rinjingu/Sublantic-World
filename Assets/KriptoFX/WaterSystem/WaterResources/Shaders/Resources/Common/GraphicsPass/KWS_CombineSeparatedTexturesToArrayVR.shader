﻿Shader "Hidden/KriptoFX/KWS/KWS_CombineSeparatedTexturesToArrayVR"
{

	SubShader
	{
	    Cull Off
	    ZWrite Off
	    ZTest Always

	    Pass
	    {
	        HLSLPROGRAM

	        #pragma vertex vert
	        #pragma fragment frag
			
			#ifndef KWS_PLATFORM_SPECIFIC_HELPERS
				#include "..\..\PlatformSpecific\KWS_PlatformSpecificHelpers.cginc"
			#endif

	        struct appdata_t
	        {
	            uint vertexID : SV_VertexID;
	            //UNITY_VERTEX_INPUT_INSTANCE_ID
	        };

	        struct v2f
	        {
	            float2 uv : TEXCOORD0;
	            float4 vertex : SV_POSITION;
	            //UNITY_VERTEX_OUTPUT_STEREO
	        };

	        v2f vert(appdata_t v)
	        {
	            v2f o;
	            //UNITY_SETUP_INSTANCE_ID(v);
	            //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	            o.vertex = GetTriangleVertexPosition(v.vertexID);
	            o.uv = GetTriangleUVScaled(v.vertexID);
	            return o;
	        }
	
	        Texture2D KWS_PlanarLeftEye;
	        Texture2D KWS_PlanarRightEye;
			float KWS_StereoIndex;

	        float4 frag(v2f i) : SV_Target
	        {
	            //UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	            if (KWS_StereoIndex == 0)  return KWS_PlanarLeftEye.Sample(sampler_linear_clamp, i.uv);
	            else return KWS_PlanarRightEye.Sample(sampler_linear_clamp, i.uv);
	        }
	        ENDHLSL
	    }
	}

}