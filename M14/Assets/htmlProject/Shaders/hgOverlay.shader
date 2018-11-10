Shader "Custom/hgOverlay" {
	Properties {
		_MainTex ("Base (RGB)", 2D)   = "white" {}
		_Alpha1 ("Alpha1",Range(0,1)) = 1
		_White  ("White", Float) = 0
	}
	Category {
		Tags { "Queue"="Overlay+100" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Ztest Off
		Fog { Mode Off } 
	 
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
	
		SubShader {
			Pass {
				SetTexture [_MainTex] {
					constantColor([_White],[_White],[_White],[_Alpha1])
					combine texture * primary + constant, texture * constant
				}
			}
		}
	}
}
