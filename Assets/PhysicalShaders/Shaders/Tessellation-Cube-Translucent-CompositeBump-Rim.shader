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

Shader "Physical/Tessellation/Transparent/Cube CompositeBump Rim" {
Properties {
	_RimTint            ("Rim Tint", Color)                 		= (1,1,1,1)
	_RimIntensity       ("Rim Intensity", Float)            		= 0 
	_RimPower           ("Rim Power", Float)                		= 4

	_BaseTexTransforms	("Base Texture Transforms", Vector) 		= (1,1,0,0)
	_Parallax 			("Height", Float) 							= 0.5 // [0,1]
	_EdgeLength 	    ("Edge length", Float) 						= 10 // [3,50]
	_ParallaxMap 		("Heightmap (A)", 2D) 						= "black" {}
	
    _Color         		("Light Bake Tint", Color)          		= (1,1,1,1)
	_MainTex          	("Light Bake Color (RGB)", 2D)      		= "white" {}
    _AlbedoTint         ("Albedo Tint", Color)              		= (1,1,1,1)
    _AlbedoMap          ("Albedo (RGB) Trans (A)", 2D)              = "white" {}
    _ReflectanceScale   ("Reflectance Tint", Color)        			= (1,1,1,1)
    _ReflectanceMap     ("F0 (RGB) Glossiness (A)", 2D)     		= "white" {}
    _AoMap              ("Ambient Occlusion (G)", 2D)       		= "white" {}
    _BumpMap            ("Normal Map", 2D)                   		= "bump" {}
		
    _DetailIntensity	("Detail Intensity", Range(0,1))    		= 1.0
	_DetailTexTransforms("Base Texture Transforms", Vector) 		= (1,1,0,0)
    _Detail    			("Detail Albedo (RGB)", 2D)           		= "white" {}
    _DetailOcclusionMap ("Detail Occlusion (G)", 2D)        		= "white" {}
	_DetailBumpMap      ("Detail Normal Map", 2D)             		= "bump" {}
	_DetailParallax 	("Detail Height", Float) 					= 0.5 // [0,1]
	_DetailParallaxMap 	("Detail Heightmap (A)", 2D) 				= "black" {}
	
	_IncandescenceTexTransforms	("Incandescence Texture Transforms", Vector) = (1,1,0,0)
    _IncandescenceTint  ("Incandescence Tint", Color)       		= (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  		= 0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         		= "white" {}
    _IncandescenceMask  ("Incandescence Mask (A)", 2D)          	= "white" {}
	
    _EnvMap             ("Reflection Cube Map", CUBE)       		= "black" {}
    _Rsrm               ("Radially-Symmetric Reflection Map", 2D)	= "black" {}
}
    
SubShader { 
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    LOD 500
    
    Zwrite On
    Blend One OneMinusSrcAlpha
   
CGPROGRAM
    #pragma surface surf PhysicalBrdf vertex:disp tessellate:tess noambient novertexlights 
	#pragma exclude_renderers flash gles
    #pragma target 5.0
    #include "Tessellation.cginc"
	#include "PhysicalShading.cginc"
   
	struct appdata {
		float4 vertex : POSITION;
		float4 tangent : TANGENT;
		float3 normal : NORMAL;
		float2 texcoord : TEXCOORD0;
		float2 texcoord1 : TEXCOORD1;
	};
	                  
	struct Input {
	    float2 uv_MainTex; 
	    float3 viewDir;
	    float3 worldRefl;
	    float3 worldNormal;
	    INTERNAL_DATA
	};
	       
    float4 _BaseTexTransforms;
    float4 _DetailTexTransforms; 
	float4 _AlbedoTint; 
	float4 _ReflectanceScale;
	float4 _IncandescenceTexTransforms;
	float4 _IncandescenceTint;
	float _IncandescenceScale;
	float _EdgeLength;
	float _Parallax;
	float _DetailIntensity;
	float _DetailParallax;
	float4 _RimTint;
	float _RimIntensity;
	float _RimPower;
	sampler2D _AoMap;
	sampler2D _AlbedoMap; 
	sampler2D _ReflectanceMap;
	sampler2D _IncandescenceMap;
	sampler2D _IncandescenceMask;
	sampler2D _BumpMap;
	sampler2D _ParallaxMap;
	sampler2D _DetailOcclusionMap;
	sampler2D _Detail; 
	sampler2D _DetailBumpMap;
	sampler2D _DetailParallaxMap;
	sampler2D _Rsrm;
	samplerCUBE _EnvMap;
	
	float4 tess (appdata v0, appdata v1, appdata v2) {
		return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength, _Parallax * 1.5f);
	}
	
	void disp (inout appdata v) {
		float2 baseTexcoords = v.texcoord.xy * _BaseTexTransforms.xy + _BaseTexTransforms.zw;
		float2 detailTexcoords = v.texcoord.xy * _DetailTexTransforms.xy + _DetailTexTransforms.zw;
		float bd = tex2Dlod(_ParallaxMap, float4(baseTexcoords,0,0)).a * _Parallax;
		float dd = tex2Dlod(_DetailParallaxMap, float4(detailTexcoords,0,0)).a * _DetailParallax;
		v.vertex.xyz += v.normal * lerp(bd, dd, _DetailIntensity);
	}
	
	void surf (Input IN, inout PhysicalSurfaceOutput o) {    
		float2 baseTexcoords = IN.uv_MainTex * _BaseTexTransforms.xy + _BaseTexTransforms.zw;
		float2 incandescenceTexcoords = IN.uv_MainTex * _IncandescenceTexTransforms.xy + _IncandescenceTexTransforms.zw;
		float2 detailTexcoords = IN.uv_MainTex * _DetailTexTransforms.xy + _DetailTexTransforms.zw;
		half ao = tex2D(_AoMap, baseTexcoords).g;
	    half4 albedo = _AlbedoTint * tex2D(_AlbedoMap, baseTexcoords);
	    half4 reflectance = _ReflectanceScale * tex2D(_ReflectanceMap, baseTexcoords);
	    half3 bump = RecoverNormal(tex2D(_BumpMap, baseTexcoords)).xyz;
	    half3 incandescence = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, incandescenceTexcoords).rgb;
	    
	    ao *= tex2D(_DetailOcclusionMap, detailTexcoords).g;
	    albedo *= tex2D(_Detail, detailTexcoords);
	    
	    half3 detailBump = bump;
	    detailBump.xy += RecoverNormal(tex2D(_DetailBumpMap, detailTexcoords)).xy;
	    detailBump = normalize(detailBump);
	    
	    o.Occlusion = ao;
	    o.Alpha		= albedo.a;
	    o.Albedo    = albedo.rgb * albedo.a;
	    o.Normal    = lerp(bump, detailBump, _DetailIntensity);
	    o.F0		= reflectance.rgb;
	    o.Specular  = reflectance.a;
	    o.Emission  = incandescence * tex2D(_IncandescenceMask, baseTexcoords).a;
	
		half dotNE = saturate(dot(o.Normal, normalize(IN.viewDir)));  
    	half3 n = normalize((half3)WorldNormalVector(IN, o.Normal));
    	half3 r = normalize((half3)WorldReflectionVector(IN, o.Normal));
    	
    	o.Emission += RimLight(_RimTint.rgb, DeGamma(_RimIntensity), _RimPower, dotNE);
		ImageBasedLighting(_Rsrm, _EnvMap, n, r, dotNE, o);
	}
ENDCG
}
    
Fallback "Transparent/Bumped Specular"
}