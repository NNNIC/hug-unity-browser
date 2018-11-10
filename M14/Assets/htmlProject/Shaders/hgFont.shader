Shader "Custom/hgFont" {

	Properties {
		_MainTex ("Main Texture",2D) = "white" {}
		_SubTex  ("Sub  Texture",2D) = "white" {}
		_PalTex  ("Palet Texture",2D)= "white" {}
		_Alpha   ("Alpha", Float)    = 1
		_White   ("White", Float)    = 0
	}

	SubShader {
		Pass {
		Tags { "Queue"="Background" }
		Fog { Mode Off }
		
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		//ZWrite Off 
		Ztest Off
		AlphaTest Greater 0
		
		CGPROGRAM

		#pragma vertex vert 
		#pragma fragment frag
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _SubTex; 
		sampler2D _PalTex;
		float     _Alpha;
		float     _White;

		struct v2f {
			float4 pos   : SV_POSITION;
			float2 uv    : TEXCOORD0;	
			float4 color : COLOR0;
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
			
			o.color = v.color;
			
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
		    
			//
			float4 palcolor = float4(0,0,0,0);
			{
		    	float2 uv;
		    	uv.x = i.color.r ;  // 1/512 
		    	uv.y = i.color.g ;  // 1/512 
				palcolor = tex2D(_PalTex, uv);
			}
			palcolor.a = palcolor.a * _Alpha * 1.2;
			
			
			return tc * palcolor + float4(_White,_White,_White,0);
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}