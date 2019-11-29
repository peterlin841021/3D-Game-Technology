Shader "Water/Water"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "White" {}
		_ReflectTex("ReflectTex", 2D) = "White" {}
		_F("Wave length", range(1, 30)) = 10
		_A("Alplitude", range(0, 0.1)) = 0.01
		_R("Wave radius", range(0, 1)) = 0
		_S("Wave speed", range(0, 30)) = 1
		_CubeMap("CubeMap", CUBE) = ""{}
		_RefractRatio("Refract Ratio", Float) = 0.5
		_FresnelScale("Fresnel Scale", Float) = 0.5
	}
		
	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			 #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"


			sampler2D _MainTex;
			sampler2D _ReflectTex;
			sampler2D _GrabTex;						
			samplerCUBE _CubeMap;

            float _FresnelScale;            
			float4 _MainTex_ST;
			float _F;
			float _A;
			float _R;
			float _S;
			float _RefractRatio;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};			

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldReflect : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldViewDir : TEXCOORD3;
				float4 vertexLocal : TEXCOORD4;
				float3 worldRefract : TEXCOORD5;
				float2 uv : TEXCOORD6;
				float2 screenUV : TEXCOORD7;				
			};

			v2f vert(appdata v)
			{
				v2f o;
				//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.vertexLocal = v.vertex;
				o.uv = v.uv;
				o.pos = UnityObjectToClipPos(v.vertex);				
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = mul(unity_ObjectToWorld, v.normal);
				o.worldViewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos.xyz);
				o.worldReflect = reflect(-o.worldViewDir, normalize(o.worldNormal));
				o.worldRefract = refract(-o.worldViewDir, normalize(o.worldNormal), _RefractRatio);				
				o.screenUV = (o.pos.xy / o.pos.w + 1)*0.5;
				return o;
			}

			fixed4 frag(v2f IN) : COLOR
			{
				//Water reflection//
				float4 fresnelReflectFactor = _FresnelScale + (1 - _FresnelScale)*pow(1 - dot(IN.worldViewDir,IN.worldNormal), 5);
				fixed4 colReflect = texCUBE(_CubeMap, normalize(IN.worldReflect));
				//Water reflection//

				//Water refraction//
				//fixed4 colDistort = tex2D(_DistortTex, IN..uv);				
				float2 sceneUV = IN.screenUV.xy;//*0.5+0.5;
				sceneUV.y = 1 - sceneUV.y;
				//fixed4 colRefract = tex2D(_GrabTex, float2(sceneUV.x, sceneUV.y) + 0);
				fixed4 colRefract = texCUBE(_CubeMap, normalize(IN.worldRefract));
				//Water refraction//

				//Reflection + Refraction//
				fixed4 mix = fresnelReflectFactor * colReflect + (1 - fresnelReflectFactor) * colRefract;
				//Reflection + Refraction//

				float2 uv = IN.uv;
				float dis = distance(uv, float2(0, 0.5));
				float scale = 0;
				_A *= saturate(1 - dis / _R);
				scale = _A * sin(-dis * 3.14 * _F + _Time.y * _S);
				uv = uv + uv * scale;				
				fixed4 color = tex2D(_MainTex, uv) + fixed4(1, 1, 1, 1) * saturate(scale) * 100;
				color.a = 0.5;
				
				float2 flip_uv = float2(1 - uv.x,uv.y);
				fixed4 reflection = tex2D(_ReflectTex, flip_uv);
				//Blending
				color = lerp(color, reflection, 0.7);
				color = lerp(color, mix, 0.7);
				return color;
			}
			ENDCG
		}
	}
		//FallBack "Diffuse"
}