Shader "Custom/Effect" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_EffectType("EffectType",Int) = 0
	}
	SubShader 
		{
		Tags { "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM		
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		int _EffectType;
		void surf (Input IN, inout SurfaceOutputStandard o) 
		{	
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			if (_EffectType == 0)
			{

			}
			else if (_EffectType == 1)
			{
				float gray = c.r * 0.3 + c.g * 0.6 + c.b * 0.1;
				c.r = gray;
				c.g = gray;
				c.b = gray;
			}
			else if (_EffectType == 2)
			{
				c.r = 1 - c.r;
				c.g = 1 - c.g;
				c.b = 1 - c.b;
			}			
			o.Albedo = c.rgb;		
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
