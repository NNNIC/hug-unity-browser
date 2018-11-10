Shader "Custom/xgFont_test1" {

	Properties {
		_MainTex ("Main Texture",2D) = "white" {}
		_Chanel  ("Chanel",Color) = (1,0,0,0)
		_Color   ("Color" ,Color) = (1,1,1,1) 
	}

	SubShader {
		Pass {
		
		Fog { Mode Off }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
	
		CGPROGRAM

		#pragma vertex vert 
		#pragma fragment frag
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		float4 _Chanel;
		float4 _Color;

		struct v2f {
			float4 pos   : SV_POSITION;
			float2 uv    : TEXCOORD0;
			//float4 color : COLOR0;
		};

		float4 _MainTex_ST;	

		v2f vert(appdata_full  v)
		{
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);

			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			float4 tc = tex2D(_MainTex, i.uv);
			
			if (dot(float4(1,1,1,1),_Chanel))
			{
				float val = dot(tc, _Chanel);
				
				tc.rgb =  val > 0.5 ? 2*val-1 : 0;
				tc.a   = val > 0.5 ? 1 : 2*val;
				//tc.a = 0;
				}
			
			return tc * _Color;	
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}
