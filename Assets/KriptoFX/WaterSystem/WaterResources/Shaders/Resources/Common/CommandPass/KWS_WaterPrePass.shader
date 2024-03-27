Shader "Hidden/KriptoFX/KWS/WaterPrePass"
{
	Properties
	{
		srpBatcherFix ("srpBatcherFix", Float) = 0
		[HideInInspector]KWS_StencilMaskValue("KWS_StencilMaskValue", Int) = 32
	}

	SubShader
	{
		Tags { "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "true" }
		
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref [KWS_StencilMaskValue]
            ReadMask [KWS_StencilMaskValue]
			Comp Greater
			Pass keep
		}

		//0 non-tesselated
		Pass
		{  
			Blend Off
			ZWrite On
			Cull Off

			HLSLPROGRAM

			//#define MASK_DEPTH_NORMAL_PASS

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_WATER_INSTANCING

			#include "../../PlatformSpecific/Includes/KWS_VertFragIncludes.cginc"

			#pragma target 4.6
			#pragma vertex vertDepth
			#pragma fragment fragDepth
			
			#pragma editor_sync_compilation

			ENDHLSL
		}


		//1 tesselated
		Pass
		{
			//Blend One Zero
			Blend Off
			ZWrite On
			Cull Off

			HLSLPROGRAM
			//#define MASK_DEPTH_NORMAL_PASS

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_WATER_INSTANCING

			#include "../../PlatformSpecific/Includes/KWS_VertFragIncludes.cginc"
			#include "../KWS_Tessellation.cginc"

			#pragma vertex vertHull
			#pragma fragment fragDepth
			#pragma hull HS
			#pragma domain DS_Depth
			#pragma target 4.6
			#pragma editor_sync_compilation

			ENDHLSL
		}
	}
}