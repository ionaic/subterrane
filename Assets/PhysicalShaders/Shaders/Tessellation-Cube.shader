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

Shader "Physical/Tessellation/Cube" {
Properties {
	_Parallax 			("Height", Float) 							= 0.5 // [0,1]
	_EdgeLength 	    ("Edge length", Float) 						= 10 // [3,50]
	_ParallaxMap 		("Heightmap (A)", 2D) 						= "black" {}
	
    _Color         		("Light Bake Tint", Color)          		= (1,1,1,1)
	_MainTex          	("Light Bake Color (RGB)", 2D)      		= "white" {}
    _AlbedoTint         ("Albedo Tint", Color)              		= (1,1,1,1)
    _AlbedoMap          ("Albedo (RGB)", 2D)                		= "white" {}
    _ReflectanceScale   ("Reflectance Tint", Color)        			= (1,1,1,1)
    _ReflectanceMap     ("F0 (RGB) Glossiness (A)", 2D)     		= "white" {}
    _AoMap              ("Ambient Occlusion (G)", 2D)       		= "white" {}
    _BumpMap            ("Normal Map", 2D)                   		= "bump" {}
    _IncandescenceTint  ("Incandescence Tint", Color)       		= (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  		= 0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         		= "white" {}
	
    _EnvMap             ("Reflection Cube Map", CUBE)       		= "black" {}
    _Rsrm               ("Radially-Symmetric Reflection Map", 2D)	= "black" {}
 
}
    
SubShader { 
    Tags { "RenderType" = "Opaque" }
    LOD 300
    
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
        
	float4 _AlbedoTint; 
	float4 _ReflectanceScale;
	float4 _IncandescenceTint;
	float _IncandescenceScale;
	float _EdgeLength;
	float _Parallax;
	sampler2D _AoMap;
	sampler2D _AlbedoMap; 
	sampler2D _ReflectanceMap;
	sampler2D _IncandescenceMap;
	sampler2D _BumpMap;
	sampler2D _ParallaxMap;
	sampler2D _Rsrm;
	samplerCUBE _EnvMap;
	
	float4 tess (appdata v0, appdata v1, appdata v2) {
		return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength, _Parallax * 1.5f);
	}
	
	void disp (inout appdata v) {
		float d = tex2Dlod(_ParallaxMap, float4(v.texcoord.xy,0,0)).a * _Parallax;
		v.vertex.xyz += v.normal * d;
	}
	
	void surf (Input IN, inout PhysicalSurfaceOutput o) {    
	    half4 albedo = _AlbedoTint * tex2D(_AlbedoMap, IN.uv_MainTex);
	    half4 reflectance = _ReflectanceScale * tex2D(_ReflectanceMap, IN.uv_MainTex);
	    
	    o.Occlusion = tex2D(_AoMap, IN.uv_MainTex).g;
	    o.Albedo    = albedo.rgb;
	    o.Normal    = RecoverNormal(tex2D(_BumpMap, IN.uv_MainTex)).xyz;
	    o.F0		= reflectance.rgb;
	    o.Specular  = reflectance.a;
	    o.Emission  = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, IN.uv_MainTex).rgb;
	
		half dotNE = saturate(dot(o.Normal, normalize(IN.viewDir)));  
    	half3 n = normalize((half3)WorldNormalVector(IN, o.Normal));
    	half3 r = normalize((half3)WorldReflectionVector(IN, o.Normal));
		ImageBasedLighting(_Rsrm, _EnvMap, n, r, dotNE, o);
	}
ENDCG
}
    
Fallback "Tessellation/Bumped Specular (displacement)"
}