Shader "SC/SuggestiveContoursEffect"
{
	Properties
	{		
		_FZ("FZ",Range(0,100)) = 50
		_C_Limit("C_limit",Range(0,100)) = 50
		_SC_Limit("SC_limit",Range(0,100)) = 50
		_DWKR_Limit("DWKR_limit",Range(0,100)) = 50
		_JeroenMethod("_Jeroen method",Int) = 0
	}
	CGINCLUDE
	#include "UnityCG.cginc"	
	sampler2D _CameraDepthTexture;
	float3 view;
	float3 w;
	float2 _MainTex_TexelSize;	
	float _FZ;
	float _C_Limit;
	float _SC_Limit;
	float _DWKR_Limit;
	
	int _JeroenMethod;

	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float3 pdir1 : TEXCOORD0;
		float3 pdir2 : TEXCOORD1;
		float3 curv1 : TEXCOORD2;
		float3 curv2 : TEXCOORD3;
		float4 dcurv : TEXCOORD4;
	};

	struct v2f 
	{
		float4 pos : SV_POSITION;
		float4 color : COLOR;
		float uv : TEXCOORD0;
		float ndotv : float;
		float t_kr : float;
		float t_dwkr : float;
	};

	v2f vert_sc(appdata a) 
	{
		v2f o;		
		o.pos = UnityObjectToClipPos(a.vertex);		
		view = -UNITY_MATRIX_IT_MV[2].xyz;
		o.ndotv = (1.0f / length(view)) * dot(a.normal, view);		
		if (!(o.ndotv < 0.0f))
		{			
			w = normalize(view - a.normal * dot(view, a.normal));
			float u = dot(w, a.pdir1);
			float v = dot(w, a.pdir2);
			float u2 = u * u;
			float v2 = v * v;
			o.t_kr = (a.curv1.x * u2) + (a.curv2.x * v2);
			float uv = u * v;
			float dwII = (u2 * u * a.dcurv.x) + (3.0 * u * uv * a.dcurv.y) + (3.0 * uv * v * a.dcurv.z) + (v * v2 * a.dcurv.w);
			o.t_dwkr = dwII + 2.0 * a.curv1 * a.curv2 * o.ndotv / sqrt((1.0 - pow(o.ndotv, 2.0)));
		}
		//
		o.uv = a.pdir1.xy;
		o.color.xyz = view.xyz;
		o.color.w = 1;
		//
		return o;
	}

	fixed4 frag(v2f v) : SV_Target
	{
		fixed4 baseColor = fixed4(1,1,1,1);
				
		float kr = _FZ * abs(v.t_kr);
		float dwkr = _FZ * _FZ * v.t_dwkr;
		float dwkr2 = (dwkr - dwkr * pow(v.ndotv, 2.0));
		float dwkr_theta = (dwkr - dwkr * pow(v.ndotv, 2.0)) / length(w);
		//
		float contour_limit = _C_Limit * (pow(v.ndotv, 2.0) / kr);

		_SC_Limit = _SC_Limit * (kr/ dwkr2);
		float suggestive_contour_limit;
		if (_JeroenMethod)
		{
			suggestive_contour_limit = _SC_Limit * (abs(kr) / dwkr_theta);
		}
		else 
		{
			suggestive_contour_limit = _SC_Limit * (dwkr);
		}
		//
		//Contours//
		if (contour_limit < 1.0)
		{
			baseColor.xyz = min(baseColor.xyz, float3(0, 0, 0));
		}
		//Suggestive contours//
		else if ((suggestive_contour_limit < 1.0) && dwkr_theta > _DWKR_Limit)		
		//else if(_SC_Limit < 1.0 && dwkr_theta > _DWKR_Limit)
		{
			baseColor.xyz = min(baseColor.xyz, float3(0, 0, 0));
		}
		/*baseColor.xyz = v.color.xyz;
		fixed4 test = tex2D(_CameraDepthTexture, v.uv);*/
		return baseColor;
	}
	ENDCG

	SubShader
	{		
		Cull Off ZWrite Off ZTest Always		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_sc
			#pragma fragment frag		
			ENDCG
		}
	}
}
