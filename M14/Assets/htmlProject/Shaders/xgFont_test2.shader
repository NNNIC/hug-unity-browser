Shader "Custom/xgFont_test2" {

	Properties {
		_MainTex ("Main Texture",2D) = "white" {}
		//_Chanel  ("Chanel",Color) = (1,0,0,0)
		//_Color   ("Color" ,Color) = (1,1,1,1) 
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

		//float4 _Chanel;
		//float4 _Color;

		struct v2f {
			float4 pos   : SV_POSITION;
			float2 uv    : TEXCOORD0;	
			float4 color : COLOR0;
			float4 color1: COLOR1;
		};

		float4 _MainTex_ST;	

		v2f vert(appdata_full  v)
		{
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			float x = v.normal.x;
			if (x==0.2) o.color = float4(0,1,0,0); //-- 2
			if (x==0.4) o.color = float4(1,0,0,0); //-- 4
			if (x==0.1) o.color = float4(0,0,1,0); //-- 1
			
			o.color1 = v.color;
			
			//o.color = float4(0,1,0,0);
			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			float4 tc = tex2D(_MainTex, i.uv);
			
			if (dot(float4(1,1,1,1),i.color))
			{
				float val = dot(tc, i.color);
				
				tc.rgb = val > 0 ? 1: 0;// > 0.5 ? 1 : val;// val > 0.5 ? 2*val-1 : 0;
				tc.a   = val;// > 0.9 ? 1 : val; //  ;// > 0.2 ? 1 : val;
		    }
			
			//return tc * _Color;	
			return tc * i.color1;
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}
