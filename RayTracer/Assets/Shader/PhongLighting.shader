// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PhongLighting/Phong"
{	
	Properties
	{
		_Diffuse("Diffuse", Color) = (0,0,0,1)//Kd
		_Specular("Specular", Color) = (0,0,0,1)//Ks
		_Emission("Emission", Color) = (0,0,0,1)//Ke
		_Shiness("Shiness", Range(1.0, 255)) = 20//NS
		_RefractiveCoefficient("Refract",Range(1.0, 10.0)) = 1//Ni
		_Alpha("Alpha",Range(0, 1.0)) = 1//D
		_KAMAP("KaMap", 2D) = "black" {}
		_KDMAP("KdMap", 2D) = "black" {}
		_BUMPMAP("BumpMap", 2D) = "black" {}
		_LightColor("LightColor",Color) = (1,1,0,1)		
	}
		
	SubShader
	{
		Pass
		{		
		//Tags{ "LightingMode" = "ForwardBase" }
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM		
			#include "Lighting.cginc"
		
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Diffuse;
			fixed4 _Specular;
			fixed4 _LightColor;
			fixed4 _LightPosition;
			float _Shiness;
			float _Alpha;
			float _RefractiveCoefficient;
			fixed4 _Emission;
			sampler2D _KAMAP;
			sampler2D _KDMAP;
			sampler2D _BUMPMAP;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
		
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
		
			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);//Normal to world space			
				o.worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));//Vertex to world space			
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = v.uv;
				return o;
			}
		
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 ka = tex2D(_KAMAP, i.uv);
				fixed4 kd = tex2D(_KDMAP, i.uv);
				fixed4 bump = tex2D(_BUMPMAP, i.uv);
				//
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _Diffuse;		
				fixed3 worldLight = normalize(_LightPosition.xyz);				
				fixed3 worldNormal = normalize(i.worldNormal);
				half3 normal;
				half nl;
				fixed4 d = fixed4(0,0,0,1);
				if (bump.r > 0)
				{
					//normal = UnityObjectToWorldNormal(bump);
					normal = bump.xyz;
					nl = max(0, dot(worldNormal, _LightPosition.xyz));
					d = nl * _LightColor;					
					d.xyz += ShadeSH9(half4(normal,1));
					d.xyz += normal.xyz;
				}
				//
								
				//
				//diffuse
				fixed3 diffuse = _LightColor.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLight));				
				fixed3 reflectDir = normalize(reflect(-worldLight, worldNormal));				
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);				
				fixed3 specular = _LightColor.rgb * _Specular.rgb * pow(max(0,dot(reflectDir, viewDir)), _Shiness);				
				fixed3 color = diffuse + ambient + specular;				
				color += _Emission.xyz;
				color += ka.xyz;
				color += kd.xyz;
				//
				if(d.x !=0)
					color *= d;				

				return fixed4(color, _Alpha);
			}
		ENDCG
		}
	}
}
