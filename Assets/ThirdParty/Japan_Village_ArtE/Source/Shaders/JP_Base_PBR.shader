// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ArtEquelibrium/JP_Base_PBR"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_MetallicGlossMap("_MetallicGlossMap", 2D) = "black" {}
		_OcclusionMap("_OcclusionMap", 2D) = "white" {}
		_BumpMap("Normal", 2D) = "bump" {}
		_EmissionMap("_EmissionMap", 2D) = "black" {}
		[IntRange]_Ghibly_Steps("Ghibly_Steps", Range( 0 , 32)) = 4.894118
		[HDR]_EmissionColor("_EmissionColor", Color) = (0,0,0,0)
		_GrungeMap("GrungeMap", 2D) = "white" {}
		_Ghibly_Softness("Ghibly_Softness", Range( 0 , 1)) = 0.58
		_Ghibly_Exponent("Ghibly_Exponent", Range( 0 , 8)) = 1
		_GrungeScale("GrungeScale", Range( 1 , 64)) = 1
		_Color("Tint", Color) = (0,0,0,0)
		_LightTint("LightTint", Color) = (0.08713066,0.2075472,0.1695317,0)
		_ShadowTint("ShadowTint", Color) = (0.1761748,0.2980748,0.3490566,0)
		[KeywordEnum(R,G,B)] _GrungeChannel("GrungeChannel", Float) = 0
		_Grunge_Tint("Grunge_Tint", Color) = (1,1,1,0.5686275)
		[Toggle(_GIBLYSTYLIZE_ON)] _GiblyStylize("GiblyStylize", Float) = 0
		_Grunge_Min("Grunge_Min", Range( 0 , 1)) = 0
		_GhiblyIntensity("GhiblyIntensity", Range( 0 , 1)) = 0
		_Grunge_Max("Grunge_Max", Range( 0 , 1)) = 1
		[Toggle(_ENABLEGRUNGE_ON)] _EnableGrunge("Enable Grunge", Float) = 0
		_Softness("Softness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature_local _ENABLEGRUNGE_ON
		#pragma shader_feature_local _GRUNGECHANNEL_R _GRUNGECHANNEL_G _GRUNGECHANNEL_B
		#pragma shader_feature_local _GIBLYSTYLIZE_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _BumpMap;
		uniform float _Softness;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _Grunge_Tint;
		uniform sampler2D _GrungeMap;
		uniform float _GrungeScale;
		uniform float _Grunge_Min;
		uniform float _Grunge_Max;
		uniform sampler2D _OcclusionMap;
		uniform float4 _ShadowTint;
		uniform float4 _LightTint;
		uniform float _Ghibly_Exponent;
		uniform float _Ghibly_Steps;
		uniform float _Ghibly_Softness;
		uniform float _GhiblyIntensity;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionColor;
		uniform sampler2D _MetallicGlossMap;


		inline float4 TriplanarSampling13( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = UnpackNormal( tex2D( _BumpMap, i.uv_texcoord ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float fresnelNdotV64 = dot( ase_normWorldNormal, ase_worldViewDir );
			float fresnelNode64 = ( 0.0 + 1.3 * pow( max( 1.0 - fresnelNdotV64 , 0.0001 ), 0.0 ) );
			float clampResult82 = clamp( fresnelNode64 , 0.0 , 1.0 );
			float clampResult73 = clamp( sqrt( clampResult82 ) , 0.0 , 1.0 );
			float temp_output_70_0 = ( _Softness * clampResult73 );
			float4 temp_output_17_0 = ( _Color * tex2D( _MainTex, i.uv_texcoord ) );
			float4 temp_cast_0 = (1.0).xxxx;
			float2 temp_cast_1 = (( 2.0 / _GrungeScale )).xx;
			float4 triplanar13 = TriplanarSampling13( _GrungeMap, ase_worldPos, ase_worldNormal, 1.0, temp_cast_1, 1.0, 0 );
			#if defined(_GRUNGECHANNEL_R)
				float staticSwitch16 = triplanar13.x;
			#elif defined(_GRUNGECHANNEL_G)
				float staticSwitch16 = triplanar13.g;
			#elif defined(_GRUNGECHANNEL_B)
				float staticSwitch16 = triplanar13.b;
			#else
				float staticSwitch16 = triplanar13.x;
			#endif
			float temp_output_23_0 = (0.0 + (staticSwitch16 - _Grunge_Min) * (1.0 - 0.0) / (_Grunge_Max - _Grunge_Min));
			float4 tex2DNode3 = tex2D( _OcclusionMap, i.uv_texcoord );
			float temp_output_32_0 = ( ( 1.0 - tex2DNode3.r ) * 0.5 );
			float lerpResult33 = lerp( temp_output_23_0 , staticSwitch16 , temp_output_32_0);
			float clampResult24 = clamp( lerpResult33 , 0.0 , 1.0 );
			float4 lerpResult20 = lerp( _Grunge_Tint , temp_cast_0 , clampResult24);
			float4 lerpResult46 = lerp( temp_output_17_0 , ( temp_output_17_0 * lerpResult20 ) , _Grunge_Tint.a);
			#ifdef _ENABLEGRUNGE_ON
				float4 staticSwitch37 = lerpResult46;
			#else
				float4 staticSwitch37 = temp_output_17_0;
			#endif
			float4 clampResult78 = clamp( staticSwitch37 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3_g1 = dot( ase_worldlightDir , ase_normWorldNormal );
			float clampResult28_g1 = clamp( dotResult3_g1 , 0.0 , 1.0 );
			float temp_output_22_0_g1 = _Ghibly_Steps;
			float clampResult4_g1 = clamp( ( round( ( pow( clampResult28_g1 , _Ghibly_Exponent ) * temp_output_22_0_g1 ) ) / temp_output_22_0_g1 ) , 0.0 , 1.0 );
			float4 lerpResult11_g1 = lerp( _ShadowTint , _LightTint , clampResult4_g1);
			float4 temp_cast_2 = (clampResult4_g1).xxxx;
			float4 blendOpSrc13_g1 = lerpResult11_g1;
			float4 blendOpDest13_g1 = temp_cast_2;
			float4 lerpResult15_g1 = lerp( ( saturate( (( blendOpDest13_g1 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest13_g1 ) * ( 1.0 - blendOpSrc13_g1 ) ) : ( 2.0 * blendOpDest13_g1 * blendOpSrc13_g1 ) ) )) , lerpResult11_g1 , _Ghibly_Softness);
			float4 blendOpSrc55 = ( i.vertexColor * lerpResult15_g1 );
			float4 blendOpDest55 = staticSwitch37;
			float4 clampResult77 = clamp( ( saturate( 2.0f*blendOpDest55*blendOpSrc55 + blendOpDest55*blendOpDest55*(1.0f - 2.0f*blendOpSrc55) )) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			#ifdef _GIBLYSTYLIZE_ON
				float staticSwitch60 = 1.0;
			#else
				float staticSwitch60 = 0.0;
			#endif
			float clampResult79 = clamp( ( staticSwitch60 * _GhiblyIntensity ) , 0.0 , 1.0 );
			float4 lerpResult61 = lerp( clampResult78 , clampResult77 , clampResult79);
			float4 clampResult74 = clamp( ( temp_output_70_0 * lerpResult61 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Albedo = ( clampResult74 + lerpResult61 ).rgb;
			o.Emission = ( tex2D( _EmissionMap, i.uv_texcoord ) * _EmissionColor ).rgb;
			float clampResult83 = clamp( ( temp_output_70_0 * 0.2 ) , 0.0 , 1.0 );
			float4 tex2DNode2 = tex2D( _MetallicGlossMap, i.uv_texcoord );
			o.Metallic = ( clampResult83 + tex2DNode2.r );
			o.Smoothness = ( clampResult83 + tex2DNode2.a );
			o.Occlusion = tex2DNode3.r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows dithercrossfade 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19002
-1913;-51;1920;933;-4051.717;1346.719;1.158282;True;False
Node;AmplifyShaderEditor.RangedFloatNode;43;-1670.772,50.64282;Inherit;False;Constant;_Float2;Float 2;15;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1721.97,-92.15778;Inherit;False;Property;_GrungeScale;GrungeScale;10;0;Create;True;0;0;0;False;0;False;1;30;1;64;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1404.864,-384.9194;Inherit;True;Property;_GrungeMap;GrungeMap;7;0;Create;True;0;0;0;False;0;False;75623c99564f975449ea10c0ca4368d1;75623c99564f975449ea10c0ca4368d1;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WorldPosInputsNode;28;-1546.455,-614.2343;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1550.816,570.9033;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;44;-1371.772,-25.35718;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;4436.181,329.6272;Inherit;True;Property;_OcclusionMap;_OcclusionMap;2;0;Create;True;0;0;0;False;0;False;-1;None;cd7c259ce293dde43aa574b1c0b74d13;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;13;-1025.97,-483.1578;Inherit;True;Spherical;World;False;Top Texture 0;_TopTexture0;white;-1;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;16;-624.9696,-466.4576;Inherit;False;Property;_GrungeChannel;GrungeChannel;14;0;Create;True;0;0;0;False;0;False;0;0;2;True;;KeywordEnum;3;R;G;B;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-640.4999,-579.6824;Inherit;False;Property;_Grunge_Max;Grunge_Max;19;0;Create;True;0;0;0;False;0;False;1;0.79;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-643.4999,-712.6824;Inherit;False;Property;_Grunge_Min;Grunge_Min;17;0;Create;True;0;0;0;False;0;False;0;0.43;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;21;-359.6695,-242.0579;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;23;-200.1438,-550.0735;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-154.7957,-259.5617;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;64.90356,-254.3612;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;81.0144,-706.1443;Inherit;False;Constant;_Float3;Float 3;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;20.73027,-924.9576;Inherit;False;Property;_Grunge_Tint;Grunge_Tint;15;0;Create;True;0;0;0;False;0;False;1,1,1,0.5686275;0.4470897,0.4726604,0.4811321,0.1294118;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-991,-37.5;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;a3c0a47a9d0bf214586277c78f1734ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-905.9697,-233.1578;Inherit;False;Property;_Color;Tint;11;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.990566,0.990566,0.990566,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;24;252.0701,-385.7651;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-538.9697,-173.1578;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;20;318.7301,-722.9576;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;505.7559,-604.8393;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;52;1199.426,-1220.854;Inherit;False;Property;_LightTint;LightTint;12;0;Create;True;0;0;0;False;0;False;0.08713066,0.2075472,0.1695317,0;1,0.9551615,0.8066038,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;58;2032.07,-1384.652;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;2053.17,-1277.749;Inherit;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;1180.877,-752.3517;Inherit;False;Property;_Ghibly_Softness;Ghibly_Softness;8;0;Create;True;0;0;0;False;0;False;0.58;0.48;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;48;1211.086,-1041.463;Inherit;False;Property;_ShadowTint;ShadowTint;13;0;Create;True;0;0;0;False;0;False;0.1761748,0.2980748,0.3490566,0;0.1056871,0.1859424,0.2358491,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;64;3642.292,-1180.32;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1.3;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;46;745.0735,-689.03;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;49;1199.777,-850.178;Inherit;False;Property;_Ghibly_Exponent;Ghibly_Exponent;9;0;Create;True;0;0;0;False;0;False;1;1.4;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;1185.656,-679.0504;Inherit;False;Property;_Ghibly_Steps;Ghibly_Steps;5;1;[IntRange];Create;True;0;0;0;False;0;False;4.894118;5;0;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;47;1640.266,-935.5791;Inherit;False;JP_GibliShading;-1;;1;d3a1b5653f8d8224e8198a805145c270;0;5;19;COLOR;0.7035422,0.9622642,0.801755,0;False;20;COLOR;0.1891242,0.3701392,0.4716981,0;False;21;FLOAT;1;False;25;FLOAT;0.5;False;22;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;37;1323.737,-504.3171;Inherit;False;Property;_EnableGrunge;Enable Grunge;20;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;57;2113.291,-1155.822;Inherit;False;Property;_GhiblyIntensity;GhiblyIntensity;18;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;82;3990.047,-1054.374;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;60;2291.84,-1282.27;Inherit;False;Property;_GiblyStylize;GiblyStylize;16;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;81;4291.047,-1041.374;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;55;2029.221,-813.5377;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;2610.85,-1262.544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;77;2372.943,-828.2258;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;4051.339,-1445.925;Inherit;False;Property;_Softness;Softness;21;0;Create;True;0;0;0;False;0;False;0;0.73;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;78;2349.377,-503.2492;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;79;2833.703,-1123.873;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;73;4558.461,-1150.15;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;4893.339,-1286.925;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;61;4781.373,-603.386;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;5277.358,-1272.175;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;4894.308,-841.4854;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-980.8857,942.2404;Inherit;True;Property;_EmissionMap;_EmissionMap;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;3828.175,10.32134;Inherit;True;Property;_MetallicGlossMap;_MetallicGlossMap;1;0;Create;True;0;0;0;False;0;False;-1;None;2e26c6346dcf1304eaf9bf80d76d9fa5;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-910.1553,1158.513;Inherit;False;Property;_EmissionColor;_EmissionColor;6;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;83;5479.277,-1252.296;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;74;5142.387,-850.5596;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;76.57036,-416.9645;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;5683.05,-93.89331;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;5384.674,-376.3929;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;5437.379,-727.9699;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;5765.002,-1003.659;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-982.8011,706.0161;Inherit;True;Property;_BumpMap;Normal;3;0;Create;False;0;0;0;False;0;False;-1;80f179a6aae19c74c9aea576bc8db25b;8b116e5401c02694b92bef9572871675;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;80;6088.884,-933.4158;Inherit;False;Constant;_Float4;Float 4;22;0;Create;True;0;0;0;False;0;False;64;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;6061.529,-711.0414;Float;False;True;-1;7;ASEMaterialInspector;0;0;Standard;ArtEquelibrium/JP_Base_PBR;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;True;True;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;44;0;43;0
WireConnection;44;1;14;0
WireConnection;3;1;6;0
WireConnection;13;0;11;0
WireConnection;13;9;28;0
WireConnection;13;3;44;0
WireConnection;16;1;13;1
WireConnection;16;0;13;2
WireConnection;16;2;13;3
WireConnection;21;0;3;1
WireConnection;23;0;16;0
WireConnection;23;1;27;0
WireConnection;23;2;26;0
WireConnection;32;0;21;0
WireConnection;33;0;23;0
WireConnection;33;1;16;0
WireConnection;33;2;32;0
WireConnection;1;1;6;0
WireConnection;24;0;33;0
WireConnection;17;0;15;0
WireConnection;17;1;1;0
WireConnection;20;0;18;0
WireConnection;20;1;45;0
WireConnection;20;2;24;0
WireConnection;40;0;17;0
WireConnection;40;1;20;0
WireConnection;46;0;17;0
WireConnection;46;1;40;0
WireConnection;46;2;18;4
WireConnection;47;19;52;0
WireConnection;47;20;48;0
WireConnection;47;21;49;0
WireConnection;47;25;51;0
WireConnection;47;22;50;0
WireConnection;37;1;17;0
WireConnection;37;0;46;0
WireConnection;82;0;64;0
WireConnection;60;1;58;0
WireConnection;60;0;59;0
WireConnection;81;0;82;0
WireConnection;55;0;47;0
WireConnection;55;1;37;0
WireConnection;56;0;60;0
WireConnection;56;1;57;0
WireConnection;77;0;55;0
WireConnection;78;0;37;0
WireConnection;79;0;56;0
WireConnection;73;0;81;0
WireConnection;70;0;71;0
WireConnection;70;1;73;0
WireConnection;61;0;78;0
WireConnection;61;1;77;0
WireConnection;61;2;79;0
WireConnection;76;0;70;0
WireConnection;68;0;70;0
WireConnection;68;1;61;0
WireConnection;7;1;6;0
WireConnection;2;1;6;0
WireConnection;83;0;76;0
WireConnection;74;0;68;0
WireConnection;25;0;23;0
WireConnection;25;1;32;0
WireConnection;84;0;83;0
WireConnection;84;1;2;1
WireConnection;10;0;7;0
WireConnection;10;1;9;0
WireConnection;67;0;74;0
WireConnection;67;1;61;0
WireConnection;65;0;83;0
WireConnection;65;1;2;4
WireConnection;5;1;6;0
WireConnection;0;0;67;0
WireConnection;0;1;5;0
WireConnection;0;2;10;0
WireConnection;0;3;84;0
WireConnection;0;4;65;0
WireConnection;0;5;3;1
ASEEND*/
//CHKSM=DA7D6C8EAB04645325843910FC3D020D5818BEF2