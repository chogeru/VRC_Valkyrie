// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ArtEquelibrium/JP_Vegetation_Leafs"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.8
		_Dist("Dist", Range( 0 , 64)) = 0
		_Speed("Speed", Float) = 0
		_Exp("Exp", Range( 0 , 16)) = 0
		_Tint_Gradient("Tint_Gradient", 2D) = "white" {}
		_AO_Power("AO_Power", Range( 0 , 1)) = 0
		_Tint("Tint", Color) = (1,1,1,0)
		_GlobalMap("GlobalMap", 2D) = "white" {}
		_WindPower("WindPower", Range( 0 , 2)) = 0
		_GlobalScale("GlobalScale", Range( 0 , 512)) = 0
		_VertexShadeExponent("VertexShadeExponent", Range( 0.5 , 8)) = 0
		[Toggle(_GLOBALCOLORMAP_ON)] _GlobalColorMap("GlobalColorMap", Float) = 0
		_SunPower("SunPower", Range( 0 , 16)) = 1
		[Toggle(_DITHER_ON)] _Dither("Dither", Float) = 0
		_MainTex("MainTex", 2D) = "white" {}
		_SubSurfaceDistorion("SubSurface Distorion", Range( 0 , 1)) = 0
		_SSSPower("SSS Power", Range( 0 , 16)) = 0
		_SSSScale("SSS Scale", Range( 0 , 8)) = 0
		[HDR]_SSS_Color("SSS_Color", Color) = (0,0,0,0)
		[Toggle(_VERTEXCOLORTINT_ON)] _VertexColorTint("VertexColorTint", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma shader_feature_local _VERTEXCOLORTINT_ON
		#pragma shader_feature_local _GLOBALCOLORMAP_ON
		#pragma shader_feature_local _DITHER_ON
		#pragma multi_compile __ LOD_FADE_CROSSFADE
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 viewDir;
			float3 worldNormal;
			float4 screenPosition;
		};

		uniform float _Speed;
		uniform float _WindPower;
		uniform float _Dist;
		uniform float _Exp;
		uniform float4 _Tint;
		uniform sampler2D _MainTex;
		uniform float _VertexShadeExponent;
		uniform sampler2D _Tint_Gradient;
		uniform sampler2D _GlobalMap;
		uniform float _GlobalScale;
		uniform float _SubSurfaceDistorion;
		uniform float _SSSPower;
		uniform float _SSSScale;
		uniform float4 _SSS_Color;
		uniform float _SunPower;
		uniform float _AO_Power;
		uniform float _Cutoff = 0.8;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		inline float Dither4x4Bayer( int x, int y )
		{
			const float dither[ 16 ] = {
				 1,  9,  3, 11,
				13,  5, 15,  7,
				 4, 12,  2, 10,
				16,  8, 14,  6 };
			int r = y * 4 + x;
			return dither[r] / 16; // same # of instructions as pre-dividing due to compiler magic
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float simplePerlin3D49 = snoise( ( ase_worldPos + ( _Time.y * _Speed ) )*0.35 );
			float3 objToWorld20 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float clampResult26 = clamp( pow( ( distance( objToWorld20 , ase_worldPos ) / _Dist ) , _Exp ) , 0.0 , 1.0 );
			float lerpResult38 = lerp( clampResult26 , clampResult26 , clampResult26);
			float clampResult40 = clamp( (0.2677519 + (lerpResult38 - 0.0) * (1.0 - 0.2677519) / (1.0 - 0.0)) , 0.0 , 1.0 );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( simplePerlin3D49 * _WindPower * clampResult40 * ase_vertexNormal );
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode1 = tex2D( _MainTex, i.uv_texcoord );
			float4 temp_cast_0 = (_VertexShadeExponent).xxxx;
			float3 objToWorld60 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 normalizeResult62 = normalize( frac( ( objToWorld60 * 1.0 ) ) );
			float3 break87 = normalizeResult62;
			float3 appendResult88 = (float3(break87.z , break87.y , break87.x));
			float3 appendResult89 = (float3(break87.x , break87.z , break87.y));
			float dotResult84 = dot( appendResult88 , appendResult89 );
			float2 appendResult91 = (float2(dotResult84 , 0.0));
			float3 hsvTorgb96 = RGBToHSV( tex2D( _Tint_Gradient, appendResult91 ).rgb );
			float3 hsvTorgb98 = HSVToRGB( float3(hsvTorgb96.x,(0.0 + (hsvTorgb96.y - 0.0) * (0.8 - 0.0) / (1.0 - 0.0)),(0.15 + (hsvTorgb96.z - 0.0) * (0.8 - 0.15) / (1.0 - 0.0))) );
			float4 blendOpSrc92 = ( tex2DNode1 * pow( i.vertexColor , temp_cast_0 ) );
			float4 blendOpDest92 = float4( hsvTorgb98 , 0.0 );
			float4 lerpResult102 = lerp( ( saturate( 2.0f*blendOpDest92*blendOpSrc92 + blendOpDest92*blendOpDest92*(1.0f - 2.0f*blendOpSrc92) )) , tex2DNode1 , 0.5);
			float3 ase_worldPos = i.worldPos;
			float2 appendResult110 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 blendOpSrc115 = lerpResult102;
			float4 blendOpDest115 = tex2D( _GlobalMap, ( appendResult110 / _GlobalScale ) );
			float4 lerpBlendMode115 = lerp(blendOpDest115,(( blendOpDest115 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest115 ) * ( 1.0 - blendOpSrc115 ) ) : ( 2.0 * blendOpDest115 * blendOpSrc115 ) ),0.5);
			#ifdef _GLOBALCOLORMAP_ON
				float4 staticSwitch113 = ( saturate( lerpBlendMode115 ));
			#else
				float4 staticSwitch113 = lerpResult102;
			#endif
			float4 temp_output_107_0 = ( _Tint * staticSwitch113 );
			float4 blendOpSrc196 = temp_output_107_0;
			float4 blendOpDest196 = i.vertexColor;
			float4 lerpResult197 = lerp(  (( blendOpSrc196 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc196 - 0.5 ) ) * ( 1.0 - blendOpDest196 ) ) : ( 2.0 * blendOpSrc196 * blendOpDest196 ) ) , temp_output_107_0 , _Tint.a);
			#ifdef _VERTEXCOLORTINT_ON
				float4 staticSwitch194 = lerpResult197;
			#else
				float4 staticSwitch194 = temp_output_107_0;
			#endif
			o.Albedo = staticSwitch194.rgb;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = i.worldNormal;
			float dotResult134 = dot( i.viewDir , -( ase_worldlightDir + ( ase_worldNormal * _SubSurfaceDistorion ) ) );
			float dotResult141 = dot( pow( dotResult134 , _SSSPower ) , _SSSScale );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float dotResult199 = dot( ase_worldlightDir , ase_worldNormal );
			float clampResult204 = clamp( ( dotResult199 * _SunPower ) , 0.0 , 998999.0 );
			o.Emission = ( ( tex2DNode1 * saturate( dotResult141 ) * _SSS_Color * ( ase_lightColor * 0.2 ) ) + ( staticSwitch194 * ase_lightColor * clampResult204 ) ).rgb;
			o.Smoothness = 0.0;
			float3 objToWorld20 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float clampResult26 = clamp( pow( ( distance( objToWorld20 , ase_worldPos ) / _Dist ) , _Exp ) , 0.0 , 1.0 );
			float lerpResult38 = lerp( clampResult26 , clampResult26 , clampResult26);
			float lerpResult104 = lerp( 1.0 , ( i.vertexColor.r * lerpResult38 ) , _AO_Power);
			float clampResult155 = clamp( lerpResult104 , 0.0 , 1.0 );
			o.Occlusion = clampResult155;
			o.Alpha = 1;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen212 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither212 = Dither4x4Bayer( fmod(clipScreen212.x, 4), fmod(clipScreen212.y, 4) );
			dither212 = step( dither212, tex2DNode1.a );
			#ifdef _DITHER_ON
				float staticSwitch210 = dither212;
			#else
				float staticSwitch210 = tex2DNode1.a;
			#endif
			clip( staticSwitch210 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows dithercrossfade vertex:vertexDataFunc 

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
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.screenPosition;
				o.worldPos = worldPos;
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
				surfIN.screenPosition = IN.customPack2.xyzw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
-1913;-1;1920;921;-13328.96;2613.622;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;154;3103.126,-1181.872;Inherit;False;2662.104;777.3309;Comment;10;64;60;63;61;62;87;88;89;84;91;GlobalTile;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;64;3153.126,-520.5416;Inherit;False;Constant;_ColorMaskSize;ColorMaskSize;11;0;Create;True;0;0;0;False;0;False;1;0;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;60;3244.7,-868.7022;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;3539.126,-695.5414;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FractNode;61;3785.312,-682.4032;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;62;4085.47,-767.5427;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;87;4613.013,-947.934;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;89;4941.909,-762.7776;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;88;4930.946,-1131.872;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;84;5336.312,-957.8485;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,1,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;153;12149.63,-804.525;Inherit;False;2694.293;1225.151;Comment;19;110;129;111;108;115;109;102;113;103;92;45;98;90;96;100;101;43;48;47;GlobalMask;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;91;5604.23,-1032.982;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;90;12199.63,-50.51718;Inherit;True;Property;_Tint_Gradient;Tint_Gradient;4;0;Create;True;0;0;0;False;0;False;-1;None;7c66cb660b2cf3e45a9205e11b5ba8f1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;160;9219.293,-2370.423;Inherit;False;718.5977;280;Comment;1;13;MainTex;1,1,1,1;0;0
Node;AmplifyShaderEditor.RGBToHSVNode;96;12568.74,-178.8923;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;9269.293,-2320.021;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;12365.56,-593.6003;Inherit;False;Property;_VertexShadeExponent;VertexShadeExponent;10;0;Create;True;0;0;0;False;0;False;0;0.5;0.5;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;43;12233.93,-474.4633;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;100;12907.73,-92.37919;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;101;12906.83,213.6263;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.15;False;4;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;13674.17,-2656.488;Inherit;True;Property;_MainTex;MainTex;14;0;Create;True;0;0;0;False;0;False;-1;None;0d1538518fe0fc3419d4568fef8454e0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;109;13098.25,-754.5251;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PowerNode;47;12574.81,-448.7403;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;110;13324.25,-712.5251;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.HSVToRGBNode;98;13287.62,6.789519;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;158;10006.52,-900.0344;Inherit;False;1756.465;1086.678;Comment;17;136;137;132;138;139;131;133;142;134;143;140;141;135;150;183;184;185;FakeSSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;129;13304.1,-575.9991;Inherit;False;Property;_GlobalScale;GlobalScale;9;0;Create;True;0;0;0;False;0;False;0;512;0;512;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;12908.17,-535.9473;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;159;7852.602,587.2744;Inherit;False;3236.118;1025.335;Comment;20;55;20;21;19;23;25;22;24;51;54;26;53;50;42;38;41;52;49;40;59;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;137;10082.32,70.64372;Inherit;False;Property;_SubSurfaceDistorion;SubSurface Distorion;15;0;Create;True;0;0;0;False;0;False;0;0.62;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;92;13571.13,-208.9153;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;103;13752.39,-12.21329;Inherit;False;Constant;_Float1;Float 1;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;111;13530.25,-700.5251;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;136;10072.62,-68.47865;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;20;7902.602,803.6061;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;108;13758.03,-585.6303;Inherit;True;Property;_GlobalMap;GlobalMap;7;0;Create;True;0;0;0;False;0;False;-1;None;7c66cb660b2cf3e45a9205e11b5ba8f1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;10403.7,-51.22316;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;102;13919.65,-255.0883;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;132;10058.67,-250.0109;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;21;8023.955,1397.875;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;139;10459.78,-206.5229;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode;115;14150.96,-484.4013;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;23;8309.784,1017.998;Inherit;False;Property;_Dist;Dist;1;0;Create;True;0;0;0;False;0;False;0;0;0;64;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;19;8330.938,821.2544;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;157;10975.69,-2239.252;Inherit;False;530.5684;449.6489;Comment;1;106;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;131;10056.52,-418.3211;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;113;14564.92,-319.9663;Inherit;False;Property;_GlobalColorMap;GlobalColorMap;11;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;133;10315.96,-331.4858;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;207;13671.65,-1705.84;Inherit;False;1209.617;459.6057;CustomLight;7;200;202;199;201;204;205;198;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;106;11025.69,-2189.252;Inherit;False;Property;_Tint;Tint;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,0.888814,0.740566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;8740.836,1022.428;Inherit;False;Property;_Exp;Exp;3;0;Create;True;0;0;0;False;0;False;0;1.5;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;8538.938,814.2544;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;134;10467.11,-395.8083;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;198;13721.65,-1655.84;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;12011.56,-2187.693;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;186;11942.69,-2005.115;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;24;8796.836,815.4285;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;10733.48,-155.1114;Inherit;False;Property;_SSSPower;SSS Power;16;0;Create;True;0;0;0;False;0;False;0;0;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;200;13761.64,-1429.234;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;143;11078.48,-156.1114;Inherit;False;Property;_SSSScale;SSS Scale;17;0;Create;True;0;0;0;False;0;False;0;2.79;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;140;10664.35,-373.1707;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;14129.25,-1392.298;Inherit;False;Property;_SunPower;SunPower;12;0;Create;True;0;0;0;False;0;False;1;0;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;51;9953.048,1306.504;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;54;10022.57,1496.609;Inherit;False;Property;_Speed;Speed;2;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;199;13964.37,-1536.234;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;26;9103.968,824.8282;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;196;12215.25,-2501.699;Inherit;False;HardLight;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;9371.161,1131.509;Inherit;False;Constant;_MaxShadow;MaxShadow;11;0;Create;True;0;0;0;False;0;False;0.2677519;0.145;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;11499.18,-243.6984;Inherit;False;Constant;_Float0;Float 0;18;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;156;11095.68,-1442.693;Inherit;False;726.9238;338.5453;Comment;4;104;155;105;28;AO;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;141;10900.48,-365.1115;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;197;12385.52,-2303.457;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;2;10928.55,-1385.822;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;10172.48,1356.475;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;9972.6,1116.401;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;38;9372.132,877.7759;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;14292.16,-1597.797;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;183;11340.58,-391.4604;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;11138.95,-1355.614;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;204;14490.82,-1550.257;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;998999;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;194;12622.48,-2162.629;Inherit;False;Property;_VertexColorTint;VertexColorTint;19;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;206;13061.3,-2567.896;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;52;10280.02,1217.427;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;105;11145.68,-1220.148;Inherit;False;Property;_AO_Power;AO_Power;5;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;41;9659.221,921.4109;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;11632.03,-364.3481;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;135;11103.95,-404.8446;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;150;11065.18,-850.0344;Inherit;False;Property;_SSS_Color;SSS_Color;18;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0.7873164,1,0.5424528,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;14212.82,-2205.754;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;205;14719.26,-1597.267;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DitheringNode;212;14179.82,-2496.512;Inherit;False;0;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;104;11397.63,-1392.693;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;55;10466.76,637.2744;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;40;9945.958,933.303;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;49;10470.9,1086.321;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.35;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;9964.755,693.5881;Inherit;False;Property;_WindPower;WindPower;8;0;Create;True;0;0;0;False;0;False;0;0.16;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;189;12569.48,-1753.629;Inherit;False;Constant;_Float2;Float 2;17;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;208;14398.03,-2343.231;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;210;14383.82,-2653.512;Inherit;False;Property;_Dither;Dither;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;215;15120.59,-1875.031;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TransformDirectionNode;182;10347.02,321.1841;Inherit;False;Object;World;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;14829.79,-2145.217;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewMatrixNode;214;14973.59,-1779.031;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;213;14847.87,-1932.961;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;155;11651.6,-1368.68;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;181;10128.02,324.1841;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;175;15001.26,-2467.795;Float;False;True;-1;7;ASEMaterialInspector;0;0;Standard;ArtEquelibrium/JP_Vegetation_Leafs;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;Back;1;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.8;True;True;0;False;TransparentCutout;;AlphaTest;All;18;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;0;-1;-1;0;False;0;0;False;;-1;0;False;;1;Pragma;multi_compile __ LOD_FADE_CROSSFADE;False;;Custom;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;63;0;60;0
WireConnection;63;1;64;0
WireConnection;61;0;63;0
WireConnection;62;0;61;0
WireConnection;87;0;62;0
WireConnection;89;0;87;0
WireConnection;89;1;87;2
WireConnection;89;2;87;1
WireConnection;88;0;87;2
WireConnection;88;1;87;1
WireConnection;88;2;87;0
WireConnection;84;0;88;0
WireConnection;84;1;89;0
WireConnection;91;0;84;0
WireConnection;90;1;91;0
WireConnection;96;0;90;0
WireConnection;100;0;96;2
WireConnection;101;0;96;3
WireConnection;1;1;13;0
WireConnection;47;0;43;0
WireConnection;47;1;48;0
WireConnection;110;0;109;1
WireConnection;110;1;109;3
WireConnection;98;0;96;1
WireConnection;98;1;100;0
WireConnection;98;2;101;0
WireConnection;45;0;1;0
WireConnection;45;1;47;0
WireConnection;92;0;45;0
WireConnection;92;1;98;0
WireConnection;111;0;110;0
WireConnection;111;1;129;0
WireConnection;108;1;111;0
WireConnection;138;0;136;0
WireConnection;138;1;137;0
WireConnection;102;0;92;0
WireConnection;102;1;1;0
WireConnection;102;2;103;0
WireConnection;139;0;132;0
WireConnection;139;1;138;0
WireConnection;115;0;102;0
WireConnection;115;1;108;0
WireConnection;19;0;20;0
WireConnection;19;1;21;0
WireConnection;113;1;102;0
WireConnection;113;0;115;0
WireConnection;133;0;139;0
WireConnection;22;0;19;0
WireConnection;22;1;23;0
WireConnection;134;0;131;0
WireConnection;134;1;133;0
WireConnection;107;0;106;0
WireConnection;107;1;113;0
WireConnection;24;0;22;0
WireConnection;24;1;25;0
WireConnection;140;0;134;0
WireConnection;140;1;142;0
WireConnection;199;0;198;0
WireConnection;199;1;200;0
WireConnection;26;0;24;0
WireConnection;196;0;107;0
WireConnection;196;1;186;0
WireConnection;141;0;140;0
WireConnection;141;1;143;0
WireConnection;197;0;196;0
WireConnection;197;1;107;0
WireConnection;197;2;106;4
WireConnection;53;0;51;2
WireConnection;53;1;54;0
WireConnection;38;0;26;0
WireConnection;38;1;26;0
WireConnection;38;2;26;0
WireConnection;201;0;199;0
WireConnection;201;1;202;0
WireConnection;28;0;2;1
WireConnection;28;1;38;0
WireConnection;204;0;201;0
WireConnection;194;1;107;0
WireConnection;194;0;197;0
WireConnection;52;0;50;0
WireConnection;52;1;53;0
WireConnection;41;0;38;0
WireConnection;41;3;42;0
WireConnection;184;0;183;0
WireConnection;184;1;185;0
WireConnection;135;0;141;0
WireConnection;144;0;1;0
WireConnection;144;1;135;0
WireConnection;144;2;150;0
WireConnection;144;3;184;0
WireConnection;205;0;194;0
WireConnection;205;1;206;0
WireConnection;205;2;204;0
WireConnection;212;0;1;4
WireConnection;104;1;28;0
WireConnection;104;2;105;0
WireConnection;40;0;41;0
WireConnection;49;0;52;0
WireConnection;208;0;144;0
WireConnection;208;1;205;0
WireConnection;210;1;1;4
WireConnection;210;0;212;0
WireConnection;215;0;213;0
WireConnection;215;1;214;0
WireConnection;182;0;181;0
WireConnection;58;0;49;0
WireConnection;58;1;59;0
WireConnection;58;2;40;0
WireConnection;58;3;55;0
WireConnection;155;0;104;0
WireConnection;175;0;194;0
WireConnection;175;2;208;0
WireConnection;175;4;189;0
WireConnection;175;5;155;0
WireConnection;175;10;210;0
WireConnection;175;11;58;0
ASEEND*/
//CHKSM=C386DE11F71C5F52CCE37AB35F20C189A89FD7CB