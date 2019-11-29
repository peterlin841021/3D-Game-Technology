Shader "Contour/ContourEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float2 _MainTex_TexelSize;	
	sampler2D_float _CameraDepthTexture;
	sampler2D _CameraGBufferTexture2;

	half4 _Color;
	half4 _Background;

	half _Threshold;
	float _InvRange;

	half _ColorSensitivity;
	half _DepthSensitivity;
	half _NormalSensitivity;
	float _InvFallOff;

	half4 frag(v2f_img i) : SV_Target
	{
		half4 c0 = tex2D(_MainTex, i.uv);	
		float2 uv0 = i.uv;                                   // TL
		float2 uv1 = i.uv + _MainTex_TexelSize.xy;           // BR
		float2 uv2 = i.uv + float2(_MainTex_TexelSize.x, 0); // TR
		float2 uv3 = i.uv + float2(0, _MainTex_TexelSize.y); // BL

		half edge = 0;

	#ifdef _CONTOUR_COLOR

		// Color samples
		float3 c1 = tex2D(_MainTex, uv1).rgb;
		float3 c2 = tex2D(_MainTex, uv2).rgb;
		float3 c3 = tex2D(_MainTex, uv3).rgb;
		
		float3 cg1 = c1 - c0;
		float3 cg2 = c3 - c2;
		float cg = sqrt(dot(cg1, cg1) + dot(cg2, cg2));

		edge = cg * _ColorSensitivity;

	#endif

	#ifdef _CONTOUR_DEPTH

		// Depth samples
		float zs0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv0);
		float zs1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv1);
		float zs2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv2);
		float zs3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv3);

		// Calculate fall-off parameter from the depth of the nearest point
		float zm = min(min(min(zs0, zs1), zs2), zs3);
		float falloff = 1.0 - saturate(LinearEyeDepth(zm) * _InvFallOff);

		// Convert to linear depth values.
		float z0 = Linear01Depth(zs0);
		float z1 = Linear01Depth(zs1);
		float z2 = Linear01Depth(zs2);
		float z3 = Linear01Depth(zs3);

		// Roberts cross operator
		float zg1 = z1 - z0;
		float zg2 = z3 - z2;
		float zg = sqrt(zg1 * zg1 + zg2 * zg2);

		edge = max(edge, zg * falloff * _DepthSensitivity / Linear01Depth(zm));

	#endif

		// Thresholding
		edge = saturate((edge - _Threshold) * _InvRange);

		half3 cb = lerp(c0.rgb, _Background.rgb, _Background.a);
		half3 co = lerp(cb, _Color.rgb, edge * _Color.a);
		return half4(co, c0.a);
	}

	ENDCG

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma multi_compile _ _CONTOUR_COLOR
			#pragma multi_compile _ _CONTOUR_DEPTH			
			ENDCG
		}
	}
}
