Shader "Custom/xgFont_Vcolor" {

	Properties {
		_MainTex ("Main Texture",2D) = "white" {}
		_SubTex  ("Sub  Texture",2D) = "white" {}
		_Alpha   ("Alpha", Float)    = 1
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
		sampler2D _SubTex; 
		float     _Alpha;

		struct v2f {
			float4 pos   : SV_POSITION;
			float2 uv    : TEXCOORD0;	
			fixed4 color : COLOR0;
			float4 color1: COLOR1;
			fixed  texId;
		};

		float4 _MainTex_ST;	
		float4 _SubTex_ST;	

		v2f vert(appdata_full  v)
		{
			v2f o;
			
			fixed x = v.color.a;
			if      (x==0  ) o.color1 = float4(0,0,1,0);
			else if (x<0.015625) o.color1 = float4(0,1,0,0); 		// 1/64				
			else if (x<0.03125) o.color1 = float4(1,0,0,0); 		// 2/64				
			else if (x<0.046875) o.color1 = float4(0,0,0,1); 		// 3/64
			
			if (x < 0.25)
			{
				o.texId = 0;
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			}
			else
			{
				o.texId = 1;
				o.uv = TRANSFORM_TEX (v.texcoord, _SubTex);
			}

			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.color   = v.color;
			o.color.a = _Alpha;
			
			return o; 
		}
 
		half4 frag(v2f i) : COLOR
		{		  
			float4 tc = float4(0,0,0,0); 
			if (i.texId==0)
			{  
		    	tc = tex2D(_MainTex, i.uv);
				if (dot(float4(1,1,1,1),i.color1))
				{
					float val = dot(tc, i.color1);
					
					tc.rgb = val > 0 ? 1: 0;
					tc.a   = val;
			    }
		    }
		    else 
		    {
		    	tc = tex2D(_SubTex, i.uv);
		    }
			
			//return tc * _Color;	
			
			return tc * i.color;
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}