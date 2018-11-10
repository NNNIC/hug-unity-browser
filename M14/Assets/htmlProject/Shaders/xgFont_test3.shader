Shader "Custom/xgFont_test3" {

	Properties {
		_MainTex ("Main Texture",2D) = "white" {}
	}

	SubShader {
		Pass {
		
		Fog { Mode Off }
		//Cull Off
		//ZWrite Off
		//ZTest LEqual
		//Blend SrcAlpha OneMinusSrcAlpha
		//AlphaTest Greater 0
		
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Ztest Off
		AlphaTest Greater 0
		
		CGPROGRAM

		#pragma vertex vert 
		#pragma fragment frag
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		struct v2f {
			float4 pos   : SV_POSITION;
			float2 uv    : TEXCOORD0;	
			fixed4 color : COLOR0;
			fixed4 color1: COLOR1;
		};

		float4 _MainTex_ST;	

		v2f vert(appdata_full  v)
		{
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			//float x = v.normal.x;
			//if (x==0.2) o.color = float4(0,1,0,0); //-- 2
			//else if (x==0.4) o.color = float4(1,0,0,0); //-- 4
			//else if (x==0.1) o.color = float4(0,0,1,0); //-- 1
			//else o.color = float4(0,0,0,1)
			//float3 f3z = float3(0,0,0);
			
			if (v.normal.x + v.normal.y + v.normal.z != 0 ) 
			{
			     o.color1 = float4(v.normal.x, v.normal.y,v.normal.z,0);		
			}
			else 
			{
				o.color1 = float4(0,0,0,1);
			}
			
			o.color = v.color;
			
			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			float4 tc = tex2D(_MainTex, i.uv);
			
			float val;
			if      (i.color1.r==1) val = tc.r;
			else if (i.color1.g==1)	val = tc.g;
			else if (i.color1.b==1) val = tc.b;
			else if (i.color1.a==1) val = tc.a;
			else val = 1;
																					
			float c = (val > 0 ? 1: 0) * i.color;				
			tc = float4(c,c,c,val);				
		    //}
		    //else 
		    //{
		    //	tc = float4(0,0,0,0);
		    //}
			
			return tc;
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}