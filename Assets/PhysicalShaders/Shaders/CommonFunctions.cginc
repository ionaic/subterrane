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

#ifndef COMMON_FUNCTIONS_CGINC
#define COMMON_FUNCTIONS_CGINC

#define EPSILON 1e-6f
#define C1 0.282095h // 1 / (2 * sqrt(PI)) 
#define C4 0.886227h

#define DeGamma(a) a * a

half3 RecoverNormal(half4 norm) {
#if 0
	return UnpackNormal(norm);
#else
	half3 n;
	n.xy = norm.wy * 2.0h - 1.0h;
	n.z = sqrt(1.0h - saturate(dot(n.xy, n.xy)));

	return n;
#endif
}

half3 GetSH0() {
	return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
}

half LinearLuminance(half3 color) {
	return dot(color, half3(0.2126h, 0.7152h, 0.0722h));
}

half3 LinearChromaticity(half3 color) {
	return color / (LinearLuminance(color) + EPSILON);
}

half3 RimLight(half3 rimTint, half rimIntensity, half rimPower, half dotNE) {
	return rimTint * (rimIntensity * pow(1.0h - dotNE, rimPower));
}

// Barebones surface structure, only required fields.
struct MinimumSurfaceOutput {
	half Alpha;
	half Specular;
	
	half3 Albedo;
	half3 Normal;
	half3 Emission;
};

// Easy way to prevent a surface from receiving any illumination.
half4 LightingLightingDisabled(MinimumSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	return 0.0h;
}

half4 LightingLightingDisabled_PrePass(MinimumSurfaceOutput s, half4 light) {
	return 0.0h;
}

half4 LightingLightingDisabled_DirLightmap(MinimumSurfaceOutput s, half4 color, half4 scale, half3 viewDir, bool surfFuncWritesNormal, out half3 specColor) {
	return 0.0h;
}

#endif
