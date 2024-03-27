#ifndef UNITY_COMMON_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#endif

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#endif

#ifndef UNITY_COLOR_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif

#ifdef UNITY_ATMOSPHERIC_SCATTERING_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"
#endif

#include "../../Common/KWS_WaterVariables.cginc"