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

Shader "Physical/Self-Illumin/Incandescence HDR Cutout" {
Properties {
	_Cutoff 			("Alpha Cutoff", Range(0,1)) 		= 0.5
    _AlphaMap   		("Cutout (A)", 2D)           		= "white" {}
    _IncandescenceScale ("Incandescence Intensity", Float)  = 1.0
	_Color  			("Incandescence Tint", Color)       = (1,1,1,1)
    _MainTex   			("Incandescence Map (RGB)", 2D)     = "white" {}
    _EmissionLM 		("Emission (Lightmapper)", Float) 	= 0
}
     
SubShader { 
    Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
    LOD 400
    
CGPROGRAM
    #pragma surface surf LightingDisabled alphatest:_Cutoff noambient nolightmap nodirlightmap novertexlights
	#pragma exclude_renderers flash gles
    #pragma target 3.0 

	#include "CommonFunctions.cginc"
	
    struct Input {
    	float2 uv_AlphaMap;
    	float2 uv_MainTex;
    };
    
    float _IncandescenceScale;
    float4 _Color; // Have to use this name for color bounce! ;_;
    sampler2D _AlphaMap;
    sampler2D _MainTex; // Have to use this name for color bounce! ;_;

    void surf (Input IN, inout MinimumSurfaceOutput o) { 
    	o.Alpha = tex2D(_AlphaMap, IN.uv_AlphaMap).a;
        o.Emission = DeGamma(_IncandescenceScale) * _Color.rgb 
        				* tex2D(_MainTex, IN.uv_MainTex).rgb;
    }
ENDCG
}
    
Fallback "Self-Illumin/Bumped Diffuse"
}