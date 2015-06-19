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

#ifndef PHYSICAL_SHADING_UBERSHADER_CGINC
#define PHYSICAL_SHADING_UBERSHADER_CGINC

#include "CommonFunctions.cginc"

#define OneOnLN2_x6 8.656170 // == 1/ln(2) * 6 (6 is SpecularPower of 5 + 1)
#define Log2Of1OnLn2_Plus2 2.528766
#define Ln2Mul2Div8 0.173287

struct PhysicalSurfaceOutput {
	// Ordered this way to take advantage of packing!
	// Detail shaders won't compile if we don't do this!
	half Alpha;
	half Specular;
	half Occlusion;
	
	half3 Albedo;
	half3 Normal;
	half3 Emission;
	half3 F0;
	half3 EnvMap;
};
		
// Schlick Fresnel approximation with spherical gaussian and fresnel 
// attenuation approximations proposed by:
// http://seblagarde.wordpress.com/2011/08/17/hello-world/#more-1
half Fresnel (half f0, half dotLH) {
  	//return f0 + (1.0h - f0) * pow(1.0h - dotLH, 5.0h);
	//return f0 + (1.0h - f0) * exp2(-OneOnLN2_x6 * dotLH);
	return f0 + (1.0h - f0) * exp2((-5.55473 * dotLH - 6.98316) * dotLH);
}

half Fresnel (half3 f0, half dotLH) {
  	//return f0 + (1.0h - f0) * pow(1.0h - dotLH, 5.0h);
	//return f0 + (1.0h - f0) * exp2(-OneOnLN2_x6 * dotLH);
	return f0 + (1.0h - f0) * exp2((-5.55473 * dotLH - 6.98316) * dotLH);
}

half FresnelWithVisibility (half f0, half smoothness, half dotNE) {
  	//return f0 + (max(smoothness, f0) - f0) * pow(1.0h - dotNE, 5.0h);
	//return f0 + (max(smoothness, f0) - f0) * exp2(-OneOnLN2_x6 * dotNE);
	return f0 + (max(smoothness, f0) - f0) * exp2((-5.55473 * dotNE - 6.98316) * dotNE);
}

half3 FresnelWithVisibility (half3 f0, half smoothness, half dotNE) {
  	//return f0 + (max(smoothness, f0) - f0) * pow(1.0h - dotNE, 5.0h);
	//return f0 + (max(smoothness, f0) - f0) * exp2(-OneOnLN2_x6 * dotNE);
	return f0 + (max(smoothness, f0) - f0) * exp2((-5.55473 * dotNE - 6.98316) * dotNE);
}

// Performs behind the scenes calculations and populates appropriate fields to
// make the Phyical BRDF work with Unity's surface shader framework. This must
// always be the final call for physically-based surface shaders!
void ImageBasedLighting (sampler2D rsrm, half3 worldNormal, half3 worldReflection, half dotNE, inout PhysicalSurfaceOutput o) {    
    half3 worldUp = half3(0.0h, 1.0h, 0.0h);
    half dotRU = dot(worldReflection, worldUp) * 0.5h + 0.5h;
			
	// Approximation to preserve diffuse + reflection + refraction <= 1.
#ifndef CUTOUT
	half lumF0 = LinearLuminance(o.F0);
	o.Alpha = lumF0 + (1.0h - lumF0) * o.Alpha;
#endif
	o.Albedo *= (1.0h - o.F0); // Assumes premultiplied alpha.
			
	// Only apply dotNE fresnel to EnvMap. 
	o.EnvMap = FresnelWithVisibility(o.F0, o.Specular, dotNE);
    o.EnvMap *= tex2D(rsrm, float2(dotRU, o.Specular)).rgb;
	
    // Ambient Lighting 
    // NOTE: For some reason, if I conditionally compile this out then I don't
    // get a world reflection vector for the RSRM in the lightmapped path. 
    // So this part must be included for both paths.
#if 1
    o.Emission += (o.Occlusion * 2.0h) * (
            	ShadeSH9(float4(worldNormal, 1.0f)) * o.Albedo +
            	ShadeSH9(float4(worldReflection, 1.0f)) * o.EnvMap);
#endif
}

void ImageBasedLighting (sampler2D rsrm, samplerCUBE envMap, half3 worldNormal, half3 worldReflection, half dotNE, inout PhysicalSurfaceOutput o) {    
    half3 worldUp = half3(0.0h, 1.0h, 0.0h);
    half dotRU = dot(worldReflection, worldUp) * 0.5h + 0.5h;
	half3 rsrmSample = tex2D(rsrm, float2(dotRU, o.Specular)).rgb;
	half3 envMapSample = texCUBE(envMap, worldReflection).rgb;
	
	// Approximation to preserve diffuse + reflection + refraction <= 1.
#ifndef CUTOUT
	half lumF0 = LinearLuminance(o.F0);
	o.Alpha = lumF0 + (1.0h - lumF0) * o.Alpha;
#endif
	o.Albedo *= (1.0h - o.F0); // Assumes premultiplied alpha.
			
	// Only apply dotNE fresnel to EnvMap. 
	o.EnvMap = FresnelWithVisibility(o.F0, o.Specular, dotNE);
    o.EnvMap *= lerp(rsrmSample, envMapSample, o.Specular);
	
    // Ambient Lighting 
    // NOTE: For some reason, if I conditionally compile this out then I don't
    // get a world reflection vector for the RSRM in the lightmapped path. 
    // So this part must be included for both paths.
#if 1
    o.Emission += (o.Occlusion * 2.0h) * (
            	ShadeSH9(float4(worldNormal, 1.0f)) * o.Albedo +
            	ShadeSH9(float4(worldReflection, 1.0f)) * o.EnvMap);
#endif
}

// Combines FarCry 3's (2sp + 1) / 8 normalization + visibility approximation
// with the spherical gaussian approximation to Blinn Phong, as well as a 4sp
// specular power to make Blinn Phong's highlights consistent with Phong IBL.
half4 LightingPhysicalBrdf (PhysicalSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 n = s.Normal; 
	half3 h = normalize(lightDir + viewDir);
	half dotNL = saturate(dot(n, lightDir));
	half dotNH = saturate(dot(n, h));
	half sp = exp2(s.Specular * 8.0h + Log2Of1OnLn2_Plus2);
	half dv = (sp * Ln2Mul2Div8 + 0.125h) * exp2(sp * dotNH - sp);
	//half sp = exp2(s.Specular * 8.0h + 2.0h); // [4,1024]
	//half dv = (sp * 0.25h + 0.125h) * pow(dotNH, sp); // (2sp + 1) / 8
  	half3 spec = s.F0 * dv;
  
	half4 c;
	c.rgb =  _LightColor0.rgb * (2.0h * atten * dotNL) * (
				s.Albedo +
                spec);
                
	// Needed to support alpha-blending on dynamic objects! >:(
    c.a = s.Alpha + (_LightColor0.a * LinearLuminance(spec) * atten); 
	return c;
}

half4 LightingPhysicalBrdf_PrePass (PhysicalSurfaceOutput s, half4 light) {
	half sp = exp2(s.Specular * 8.0h + 2.0h); // [4,1024]
	half dv = (sp * 0.25h + 0.125h) * light.a; // (2sp + 1) / 8
  	half3 spec = s.F0 * dv;
		
	half4 c;
	c.rgb = (s.Albedo * light.rgb + 
      		spec * LinearChromaticity(light.rgb));
	
	// Not really needed, but here just to be safe.
	c.a = s.Alpha + LinearLuminance(spec); 
	return c;
}

// HACK: Uses "inout" to accumulate the lighting in the SurfaceOutput.Emission
// field. This way, won't pollute the LPP specular color recovery. This also
// prevents the lightmap results from being passed through the _PrePass method.
half4 LightingPhysicalBrdf_DirLightmap (inout PhysicalSurfaceOutput s, half4 color, half4 scale, half3 viewDir, bool surfFuncWritesNormal, out half3 specColor) {
	UNITY_DIRBASIS
	half3 scalePerBasisVector;
	half3 lm = DirLightmapDiffuse(unity_DirBasis, color, scale, s.Normal, surfFuncWritesNormal, scalePerBasisVector);
	
	s.Emission += s.Occlusion * lm * (
					s.Albedo +
					s.EnvMap);
	
	// Make sure it doesn't attempt to add them elsewhere.
	specColor = 0.0h;
	return 0.0h;
}

#endif
