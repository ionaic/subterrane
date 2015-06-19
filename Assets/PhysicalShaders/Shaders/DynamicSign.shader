/*  Author's info:
 *  - Blog  : http://n00body.squarespace.com/
 *  - E-mail: crunchy.bytes.blog@gmail.com
 *
 *  Usage Requirements:
 *  - If you use a large portion of my codebase, please credit either myself
 *      or the original author for portions of code that have been borrowed
 *  - If you wish to use bits of my code in a commercial project, please use
 *      the provided information to contact me and tell me about it.
 */

Shader "Physical/Self-Illumin/Dynamic Sign" {
Properties {
    _Color        		("Diffuse Tint", Color)             = (1,1,1,1)
	_MainTex			("Diffuse (RGB)", 2D)				= "black" {}
    _IncandescenceTint  ("Incandescence Tint", Color)       = (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  = 1.0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         = "black" {}
    _IncandescenceMask  ("Incandescence Mask (A)", 2D)      = "white" {}
    _EmissionLM 		("Emission (Lightmapper)", Float) 	= 0
}
     
SubShader { 
    Tags { "RenderType"="Opaque" }
    LOD 300
    
CGPROGRAM
    #pragma surface surf LightingDisabled noambient nolightmap nodirlightmap novertexlights
	#pragma exclude_renderers flash gles
    #pragma target 3.0 

	#include "CommonFunctions.cginc"
	
    struct Input {
	    float2 uv_MainTex; 
	    float2 uv_IncandescenceMask;
	    float2 uv2_IncandescenceMap;
    };
    
	float4 _IncandescenceTint;
	float _IncandescenceScale;
    float4 _Color; // Have to use this name for color bounce! ;_;
    sampler2D _MainTex; // Have to use this name for color bounce! ;_;
	sampler2D _IncandescenceMap;
	sampler2D _IncandescenceMask;

    void surf (Input IN, inout MinimumSurfaceOutput o) { 
		half3 incandescence = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, IN.uv2_IncandescenceMap).rgb;
	    
	    o.Emission  = _Color.rgb * tex2D(_MainTex, IN.uv_MainTex).rgb;
	    o.Emission += incandescence * tex2D(_IncandescenceMask, IN.uv_IncandescenceMask).a;
    }
ENDCG
}
    
Fallback "Self-Illumin/Bumped Diffuse"
}