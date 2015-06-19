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

Shader "Physical/Transparent/Rsrm Distort MaskedIncandescence" {
Properties {
	_BumpAmt  			("Distortion", range (0,128)) 				= 10
    _Color         		("Light Bake Tint", Color)          		= (1,1,1,1)
	_MainTex          	("Light Bake Color (RGB)", 2D)      		= "white" {}
    _AlbedoTint         ("Albedo Tint", Color)              		= (1,1,1,1)
    _AlbedoMap          ("Albedo (RGB) Trans (A)", 2D)              = "white" {}
    _ReflectanceScale   ("Reflectance Tint", Color)        			= (1,1,1,1)
    _ReflectanceMap     ("F0 (RGB) Glossiness (A)", 2D)     		= "white" {}
    _AoMap              ("Ambient Occlusion (G)", 2D)       		= "white" {}
    _BumpMap            ("Normal Map", 2D)                   		= "bump" {}
    _IncandescenceTint  ("Incandescence Tint", Color)       		= (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  		= 0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         		= "white" {}
    _IncandescenceMask  ("Incandescence Mask (A)", 2D)          	= "white" {}
	
    _Rsrm               ("Radially-Symmetric Reflection Map", 2D)	= "black" {}
}
    
SubShader {
    // We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }
    LOD 500

	// This pass grabs the screen behind the object into a texture.
	// We can access the result in the next pass as _GrabTexture
	GrabPass {							
		Name "BASE"
		Tags { "LightMode" = "Always" }
	}
	
	// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
	// on to the screen
	Pass {
		Name "BASE"
		Tags { "LightMode" = "Always" }
			
CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma fragmentoption ARB_precision_hint_fastest
	#include "UnityCG.cginc"
	
	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord: TEXCOORD0;
	};
	
	struct v2f {
		float4 vertex : POSITION;
		float4 uvgrab : TEXCOORD0;
		float2 uvbump : TEXCOORD1;
		float2 uvmain : TEXCOORD2;
	};
	
	float _BumpAmt;
	float4 _BumpMap_ST;
	float4 _MainTex_ST;
	
	v2f vert (appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
		#else
		float scale = 1.0;
		#endif
		o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
		o.uvgrab.zw = o.vertex.zw;
		o.uvbump = TRANSFORM_TEX( v.texcoord, _MainTex );
		o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
		return o;
	}
	
	#include "CommonFunctions.cginc"
	
	float4 _AlbedoTint;
	float4 _ReflectanceScale;
	sampler2D _GrabTexture;
	float4 _GrabTexture_TexelSize;
	sampler2D _BumpMap;
	sampler2D _AlbedoMap;
	sampler2D _ReflectanceMap;
	
	half4 frag( v2f i ) : COLOR
	{
		// calculate perturbed coordinates
		half2 bump = RecoverNormal(tex2D( _BumpMap, i.uvbump )).rg; // we could optimize this by just reading the x & y without reconstructing the Z
		float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
		i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
		
		half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
		
		// NOTE: This will break when we switch to premultiplied alpha in the 
		// albedo map!
		half4 tint = tex2D( _AlbedoMap, i.uvmain ) * _AlbedoTint;
		half3 f0 = (_ReflectanceScale * tex2D(_ReflectanceMap, i.uvmain)).rgb;
	    
	    tint.rgb *= (1.0h - f0);
		return col * tint;
	}
ENDCG
	}
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    LOD 500
    
    Zwrite Off
    Blend One OneMinusSrcAlpha
    
CGPROGRAM
    #pragma surface surf PhysicalBrdf noambient novertexlights
	#pragma exclude_renderers flash gles
    #pragma target 3.0
	
	#include "PhysicalShading.cginc"
	                  
	struct Input {
	    float2 uv_MainTex; 
	    float2 uv2_IncandescenceMap; 
	    float3 viewDir;
	    float3 worldRefl;
	    float3 worldNormal;
	    INTERNAL_DATA
	};
        
	float4 _AlbedoTint; 
	float4 _ReflectanceScale;
	float4 _IncandescenceTint;
	float _IncandescenceScale;
	sampler2D _AoMap;
	sampler2D _AlbedoMap; 
	sampler2D _ReflectanceMap;
	sampler2D _IncandescenceMap;
	sampler2D _IncandescenceMask;
	sampler2D _BumpMap;
	sampler2D _Rsrm;
		
	void surf (Input IN, inout PhysicalSurfaceOutput o) {    
	    half4 albedo = _AlbedoTint * tex2D(_AlbedoMap, IN.uv_MainTex);
	    half4 reflectance = _ReflectanceScale * tex2D(_ReflectanceMap, IN.uv_MainTex);
	    half3 incandescence = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, IN.uv2_IncandescenceMap).rgb;
	    
	    o.Occlusion = tex2D(_AoMap, IN.uv_MainTex).g;
	    o.Alpha		= albedo.a;
	    o.Albedo    = albedo.rgb * albedo.a;
	    o.Normal    = RecoverNormal(tex2D(_BumpMap, IN.uv_MainTex)).xyz;
	    o.F0		= reflectance.rgb;
	    o.Specular  = reflectance.a;
	    o.Emission  = incandescence * tex2D(_IncandescenceMask, IN.uv_MainTex).a;
	
		half dotNE = saturate(dot(o.Normal, normalize(IN.viewDir))); 
    	half3 n = normalize((half3)WorldNormalVector(IN, o.Normal));
    	half3 r = normalize((half3)WorldReflectionVector(IN, o.Normal));
		ImageBasedLighting(_Rsrm, n, r, dotNE, o);
	}
ENDCG
}
    
Fallback "Transparent/Bumped Specular"
}