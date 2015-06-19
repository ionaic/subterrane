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

Shader "Physical/Core/Rsrm Detail Rim" {
Properties {
	_RimTint            ("Rim Tint", Color)                 		= (1,1,1,1)
	_RimIntensity       ("Rim Intensity", Float)            		= 0 
	_RimPower           ("Rim Power", Float)                		= 4

	_Color         		("Light Bake Tint", Color)          		= (1,1,1,1)
	_MainTex          	("Light Bake Color (RGB)", 2D)      		= "white" {}
    _AlbedoTint         ("Albedo Tint", Color)              		= (1,1,1,1)
    _AlbedoMap          ("Albedo (RGB)", 2D)                		= "white" {}
    _ReflectanceScale   ("Reflectance Tint", Color)        			= (1,1,1,1)
    _ReflectanceMap     ("F0 (RGB) Glossiness (A)", 2D)     		= "white" {}
    _AoMap              ("Ambient Occlusion (G)", 2D)       		= "white" {}
    _BumpMap            ("Normal Map", 2D)                   		= "bump" {}
    _DetailIntensity	("Detail Intensity", Range(0,1))    		= 1.0
    _Detail    			("Detail Albedo (RGB)", 2D)           		= "white" {}
    _DetailOcclusionMap ("Detail Occlusion (G)", 2D)        		= "white" {}
	_DetailBumpMap      ("Detail Normal Map", 2D)             		= "bump" {}
    _IncandescenceTint  ("Incandescence Tint", Color)       		= (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  		= 0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         		= "white" {}
	
    _Rsrm               ("Radially-Symmetric Reflection Map", 2D)	= "black" {}
}
    
SubShader { 
    Tags { "RenderType" = "Opaque" }
    LOD 300
    
CGPROGRAM
    #pragma surface surf PhysicalBrdf noambient novertexlights
	#pragma exclude_renderers flash gles
    #pragma target 3.0 
        
	#include "PhysicalShading.cginc"
        
	struct Input {
	    float2 uv_MainTex; 
	    float2 uv_Detail; 
	    float3 viewDir;
	    float3 worldRefl;
	    float3 worldNormal;
	    INTERNAL_DATA
	};
        
	float4 _AlbedoTint; 
	float4 _ReflectanceScale;
	float4 _IncandescenceTint;
	float _IncandescenceScale;
	float _DetailIntensity;
	float4 _RimTint;
	float _RimIntensity;
	float _RimPower;
	sampler2D _AoMap;
	sampler2D _AlbedoMap; 
	sampler2D _ReflectanceMap;
	sampler2D _IncandescenceMap;
	sampler2D _BumpMap;
	sampler2D _DetailOcclusionMap;
	sampler2D _Detail; 
	sampler2D _DetailBumpMap;
	sampler2D _Rsrm;
		
	void surf (Input IN, inout PhysicalSurfaceOutput o) { 
		half ao = tex2D(_AoMap, IN.uv_MainTex).g;
	    half4 albedo = _AlbedoTint * tex2D(_AlbedoMap, IN.uv_MainTex);
	    half4 reflectance = _ReflectanceScale * tex2D(_ReflectanceMap, IN.uv_MainTex);
	    half3 bump = RecoverNormal(tex2D(_BumpMap, IN.uv_MainTex)).xyz;
	    
	    ao *= tex2D(_DetailOcclusionMap, IN.uv_Detail).g;
	    albedo *= tex2D(_Detail, IN.uv_Detail);
	    
	    half3 detailBump = bump;
	    detailBump.xy += RecoverNormal(tex2D(_DetailBumpMap, IN.uv_Detail)).xy;
	    detailBump = normalize(detailBump);
	    
	    o.Occlusion = ao;
	    o.Albedo    = albedo.rgb;
	    o.Normal    = lerp(bump, detailBump, _DetailIntensity);
	    o.F0		= reflectance.rgb;
	    o.Specular  = reflectance.a;
	    o.Emission  = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, IN.uv_MainTex).rgb;
	
		half dotNE = saturate(dot(o.Normal, normalize(IN.viewDir)));  
    	half3 n = normalize((half3)WorldNormalVector(IN, o.Normal));
    	half3 r = normalize((half3)WorldReflectionVector(IN, o.Normal));
    	
    	o.Emission += RimLight(_RimTint.rgb, DeGamma(_RimIntensity), _RimPower, dotNE);
		ImageBasedLighting(_Rsrm, n, r, dotNE, o);
	}
ENDCG
}
    
Fallback "Diffuse Detail"
}