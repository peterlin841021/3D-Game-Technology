Shader "NSS" 
{
	Properties {
		_Color("Color", Color) = (0,0,0,1)
		_MainTex ("Albedo (RGB)", 2D) = "black" {}
		_NormalTex("Normal map", 2D) = "black" {}		
		_SpecularColor ("Specular color", Color) = (0,0,0,1)
		_EmissionColor("Emissuin color", Color) = (0,0,0,1)
		_Alpha("Alpha", Range(0,1)) = 1
	}
	SubShader 
		{		
		Tags { "RenderType" = "Transparent"}		
		LOD 200		
		CGPROGRAM		
		#pragma surface surf StandardSpecular finalcolor:mycolor

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _EmissionColor;
		fixed4 _Color;
		fixed4 _SpecularColor;		
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)
			
		float _Alpha;
		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			float3 normal = UnpackNormal(tex2D(_NormalTex, IN.uv_MainTex));
			c += _Color;
			o.Albedo = c.rgb;			
			o.Specular = _SpecularColor.rgb;			
			if (normal.b > 0)
				o.Normal = normal;
			o.Alpha = _Alpha;
			o.Emission = _EmissionColor.xyz;
		}
		void mycolor(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color)
		{
			//color.rgb = o.Albedo.rgb;
			color.a = o.Alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
