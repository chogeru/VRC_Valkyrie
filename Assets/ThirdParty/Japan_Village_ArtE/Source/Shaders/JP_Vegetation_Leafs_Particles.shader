// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ArtEquelibrium/JP_Vegetation_Leafs_Base"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.86
		_BillboardScale("BillboardScale", Range( 0 , 5)) = 1
		_BillBoardOffset("BillBoardOffset", Range( -1 , 1)) = 0
		_BillBoardNoisePower("BillBoardNoisePower", Range( 0 , 16)) = 0
		_BillBoardNoiseRemaper("BillBoardNoiseRemaper", Vector) = (0,1,1,1)
		_Tint("Tint", Color) = (0,0,0,0)
		_GhiblyIntensity("GhiblyIntensity", Range( 0 , 1)) = 0
		[Toggle(_GIBLYSTYLIZE_ON)] _GiblyStylize("GiblyStylize", Float) = 0
		[IntRange]_Ghibly_Steps("Ghibly_Steps", Range( 0 , 32)) = 4.894118
		_Ghibly_Softness("Ghibly_Softness", Range( 0 , 1)) = 0.58
		_Ghibly_Exponent("Ghibly_Exponent", Range( 0 , 8)) = 1
		_LightTint("LightTint", Color) = (0.08713066,0.2075472,0.1695317,0)
		_ShadowTint("ShadowTint", Color) = (0.1761748,0.2980748,0.3490566,0)
		_OverlayFactor("OverlayFactor", Range( 0 , 1)) = 0
		[Toggle(_FACECAMERABILLBOARD_ON)] _FaceCameraBillboard("FaceCameraBillboard", Float) = 0
		_WindScale("WindScale", Range( 0 , 128)) = 1
		_WindSpeed("WindSpeed", Range( 0 , 3)) = 1
		_WindPower("WindPower", Range( 0 , 2)) = 1
		[Toggle(_USEUV2_ON)] _UseUV2("UseUV2", Float) = 0
		[Toggle(_UV_AS_WINDMASK_ON)] _UV_as_WindMask("UV_as_WindMask", Float) = 0
		_AmbientOcclusion_Power("AmbientOcclusion_Power", Range( 0 , 1)) = 1
		[HDR]_SubSurfaceTint("SubSurfaceTint", Color) = (0,0,0,0)
		_SubSurfaceDistorion("SubSurface Distorion", Range( 0 , 1)) = 0
		_SSSPower("SSS Power", Range( 0 , 16)) = 0
		_SSSScale("SSS Scale", Range( 0 , 8)) = 0
		[Toggle(_GLOBALTINT_ENABLE_ON)] _GlobalTint_Enable("GlobalTint_Enable", Float) = 0
		_Global_Blend("Global_Blend", Range( 0 , 1)) = 0.524
		_GlobalTintScale("GlobalTintScale", Range( 1 , 2048)) = 1024
		_GlobalMask("GlobalMask", 2D) = "white" {}
		_RandomizeColorFactor("RandomizeColorFactor", Range( 0.0001 , 64)) = 0
		_RandomizerDarkness("RandomizerDarkness", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 5.0
		#pragma shader_feature_local _FACECAMERABILLBOARD_ON
		#pragma shader_feature_local _UV_AS_WINDMASK_ON
		#pragma shader_feature_local _USEUV2_ON
		#pragma shader_feature_local _GIBLYSTYLIZE_ON
		#pragma shader_feature_local _GLOBALTINT_ENABLE_ON
		#pragma multi_compile __ LOD_FADE_CROSSFADE
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float3 worldNormal;
			float3 viewDir;
		};

		uniform float _BillBoardOffset;
		uniform float _BillBoardNoisePower;
		uniform float4 _BillBoardNoiseRemaper;
		uniform float _BillboardScale;
		uniform float _WindSpeed;
		uniform float _WindScale;
		uniform float _WindPower;
		uniform float _AmbientOcclusion_Power;
		uniform sampler2D _MainTex;
		uniform float4 _Tint;
		uniform float4 _ShadowTint;
		uniform float4 _LightTint;
		uniform float _Ghibly_Exponent;
		uniform float _Ghibly_Steps;
		uniform float _Ghibly_Softness;
		uniform float _OverlayFactor;
		uniform float _GhiblyIntensity;
		uniform sampler2D _GlobalMask;
		uniform float _GlobalTintScale;
		uniform float _Global_Blend;
		uniform float _RandomizeColorFactor;
		uniform float _RandomizerDarkness;
		uniform float _SubSurfaceDistorion;
		uniform float _SSSPower;
		uniform float _SSSScale;
		uniform float4 _SubSurfaceTint;
		uniform float _Cutoff = 0.86;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 appendResult5_g21 = (float3((float2( -1,-1 ) + (v.texcoord.xy - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) , 0.0));
			float3 normalizeResult14_g21 = normalize( mul( float4( mul( float4( appendResult5_g21 , 0.0 ), UNITY_MATRIX_V ).xyz , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float simplePerlin3D24_g21 = snoise( ( ase_worldPos * 1.0 )*_BillBoardNoisePower );
			simplePerlin3D24_g21 = simplePerlin3D24_g21*0.5 + 0.5;
			float4 break34_g21 = _BillBoardNoiseRemaper;
			float3 lerpResult17_g21 = lerp( ( ( ase_vertexNormal * _BillBoardOffset ) + normalizeResult14_g21 ) , float3( 0,0,0 ) , ( (break34_g21.z + (simplePerlin3D24_g21 - break34_g21.x) * (break34_g21.w - break34_g21.z) / (break34_g21.y - break34_g21.x)) * -( _BillboardScale + -1.0 ) ));
			#ifdef _FACECAMERABILLBOARD_ON
				float3 staticSwitch293 = lerpResult17_g21;
			#else
				float3 staticSwitch293 = float3( 0,0,0 );
			#endif
			float4 appendResult313 = (float4(ase_worldPos.x , ( ase_worldPos.y * 0.5 ) , ase_worldPos.z , 0.0));
			float simplePerlin3D303 = snoise( ( appendResult313 + ( _Time.x * _WindSpeed ) ).xyz*_WindScale );
			float temp_output_311_0 = ( simplePerlin3D303 * _WindPower * v.color.r );
			#ifdef _UV_AS_WINDMASK_ON
				float staticSwitch377 = ( temp_output_311_0 * pow( v.texcoord.xy.y , 3.0 ) );
			#else
				float staticSwitch377 = temp_output_311_0;
			#endif
			float3 VertexFinal484 = ( staticSwitch293 + staticSwitch377 );
			v.vertex.xyz += VertexFinal484;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_cast_0 = (1.0).xxxx;
			float4 lerpResult382 = lerp( temp_cast_0 , i.vertexColor , _AmbientOcclusion_Power);
			#ifdef _USEUV2_ON
				float2 staticSwitch374 = i.uv2_texcoord2;
			#else
				float2 staticSwitch374 = i.uv_texcoord;
			#endif
			float4 tex2DNode213 = tex2D( _MainTex, staticSwitch374 );
			float4 MainColTinted478 = ( ( lerpResult382 * tex2DNode213 ) * _Tint );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float dotResult3_g12 = dot( ase_worldlightDir , ase_normWorldNormal );
			float clampResult28_g12 = clamp( dotResult3_g12 , 0.0 , 1.0 );
			float temp_output_22_0_g12 = _Ghibly_Steps;
			float clampResult4_g12 = clamp( ( round( ( pow( clampResult28_g12 , _Ghibly_Exponent ) * temp_output_22_0_g12 ) ) / temp_output_22_0_g12 ) , 0.0 , 1.0 );
			float4 lerpResult11_g12 = lerp( _ShadowTint , _LightTint , clampResult4_g12);
			float4 temp_cast_1 = (clampResult4_g12).xxxx;
			float4 blendOpSrc13_g12 = lerpResult11_g12;
			float4 blendOpDest13_g12 = temp_cast_1;
			float4 lerpResult15_g12 = lerp( ( saturate( (( blendOpDest13_g12 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest13_g12 ) * ( 1.0 - blendOpSrc13_g12 ) ) : ( 2.0 * blendOpDest13_g12 * blendOpSrc13_g12 ) ) )) , lerpResult11_g12 , _Ghibly_Softness);
			float4 temp_output_282_0 = ( i.vertexColor * lerpResult15_g12 );
			float4 blendOpSrc232 = MainColTinted478;
			float4 blendOpDest232 = temp_output_282_0;
			float4 lerpResult376 = lerp( temp_output_282_0 , ( saturate( (( blendOpDest232 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest232 ) * ( 1.0 - blendOpSrc232 ) ) : ( 2.0 * blendOpDest232 * blendOpSrc232 ) ) )) , _OverlayFactor);
			#ifdef _GIBLYSTYLIZE_ON
				float staticSwitch266 = 1.0;
			#else
				float staticSwitch266 = 0.0;
			#endif
			float temp_output_296_0 = ( staticSwitch266 * _GhiblyIntensity );
			float4 lerpResult297 = lerp( MainColTinted478 , lerpResult376 , temp_output_296_0);
			float4 MixedGhiblyCol481 = lerpResult297;
			float2 appendResult438 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 blendOpSrc461 = MixedGhiblyCol481;
			float4 blendOpDest461 = tex2D( _GlobalMask, ( ( 1.0 / _GlobalTintScale ) * appendResult438 ) );
			float4 temp_output_461_0 = ( saturate( ( blendOpSrc461 * blendOpDest461 ) ));
			#ifdef _GLOBALTINT_ENABLE_ON
				float staticSwitch430 = 1.0;
			#else
				float staticSwitch430 = 0.0;
			#endif
			float temp_output_454_0 = ( _Global_Blend * staticSwitch430 );
			float4 lerpResult444 = lerp( MixedGhiblyCol481 , temp_output_461_0 , temp_output_454_0);
			float4 AO_Final475 = lerpResult382;
			float3 objToWorld502 = mul( unity_ObjectToWorld, float4( float3( 0,0,-1.4 ), 1 ) ).xyz;
			float simplePerlin3D523 = snoise( ( objToWorld502 * _RandomizeColorFactor )*_RandomizeColorFactor );
			simplePerlin3D523 = simplePerlin3D523*0.5 + 0.5;
			float clampResult508 = clamp( (_RandomizerDarkness + (simplePerlin3D523 - 0.2) * (1.0 - _RandomizerDarkness) / (1.0 - 0.2)) , 0.0 , 1.0 );
			float RandomDarkness512 = clampResult508;
			float4 FinalAlbedo490 = ( lerpResult444 * AO_Final475 * RandomDarkness512 );
			o.Albedo = FinalAlbedo490.rgb;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float dotResult394 = dot( i.viewDir , -( ase_worldlightDir + ( ase_worldNormal * _SubSurfaceDistorion ) ) );
			float dotResult398 = dot( pow( dotResult394 , _SSSPower ) , _SSSScale );
			float4 FinalSubSurface500 = ( ( ase_lightColor * 0.2 ) * saturate( dotResult398 ) * _SubSurfaceTint * AO_Final475 * MixedGhiblyCol481 );
			float4 lerpResult290 = lerp( float4( 0,0,0,0 ) , temp_output_282_0 , temp_output_296_0);
			float4 ClearGhibly498 = lerpResult290;
			float4 temp_output_404_0 = ( FinalSubSurface500 + ClearGhibly498 );
			float4 lerpResult453 = lerp( temp_output_404_0 , ( temp_output_461_0 * temp_output_404_0 ) , temp_output_454_0);
			float4 FinalEmmision488 = ( lerpResult453 * AO_Final475 * RandomDarkness512 );
			o.Emission = FinalEmmision488.rgb;
			float FinalSmoothness493 = 0.0;
			o.Smoothness = FinalSmoothness493;
			o.Occlusion = AO_Final475.r;
			o.Alpha = 1;
			float FinalOpacity495 = tex2DNode213.a;
			clip( FinalOpacity495 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19002
-1913;17;1920;903;-22630.37;1557.8;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;469;18194.15,-4158.641;Inherit;False;2609.32;760.4895;Comment;14;235;236;382;475;219;374;372;373;220;213;383;384;478;495;Input;0.5011196,0.3831435,0.6603774,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;372;18244.15,-3911.734;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;373;18247.9,-3778.394;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;219;19173.8,-4035.453;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;374;18634.9,-3843.394;Inherit;False;Property;_UseUV2;UseUV2;19;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;383;19575.2,-4108.641;Inherit;False;Constant;_Float4;Float 4;21;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;384;19486.64,-3816.742;Inherit;False;Property;_AmbientOcclusion_Power;AmbientOcclusion_Power;21;0;Create;True;0;0;0;False;0;False;1;0.769;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;382;19847.04,-3995.01;Inherit;False;3;0;COLOR;1,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;213;19153.15,-3747.079;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;7e0a8c6bcd72d6a498989b49d077f7c6;True;0;False;white;LockedToTexture2D;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;20257.76,-3781.696;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;385;22146.42,-1658.263;Inherit;False;2252.295;1178.86;Comment;21;483;477;403;400;401;405;399;398;397;395;396;393;394;392;391;390;389;388;386;387;500;FakeSSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;236;20005.85,-3674.858;Inherit;False;Property;_Tint;Tint;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;386;22212.52,-826.7084;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;387;22222.22,-687.5853;Inherit;False;Property;_SubSurfaceDistorion;SubSurface Distorion;23;0;Create;True;0;0;0;False;0;False;0;0.294;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;472;18199.61,-2763.136;Inherit;False;3401.42;1314.579;Comment;20;498;481;290;479;297;232;480;283;376;256;259;272;260;271;296;291;266;295;294;282;GhiblyStylize;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;235;20419.81,-3765.871;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;388;22198.57,-1008.24;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;272;18268.51,-1902.777;Inherit;False;Property;_Ghibly_Exponent;Ghibly_Exponent;11;0;Create;True;0;0;0;False;0;False;1;0.32;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;260;18279.82,-2094.062;Inherit;False;Property;_ShadowTint;ShadowTint;13;0;Create;True;0;0;0;False;0;False;0.1761748,0.2980748,0.3490566,0;0,0.3470968,0.509434,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;389;22543.6,-809.4524;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;259;18268.16,-2273.453;Inherit;False;Property;_LightTint;LightTint;12;0;Create;True;0;0;0;False;0;False;0.08713066,0.2075472,0.1695317,0;0.7445419,0.9245283,0.2660198,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;256;18254.39,-1731.649;Inherit;False;Property;_Ghibly_Steps;Ghibly_Steps;9;1;[IntRange];Create;True;0;0;0;False;0;False;4.894118;5;0;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;478;20571.8,-3908.309;Inherit;False;MainColTinted;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;271;18249.61,-1804.951;Inherit;False;Property;_Ghibly_Softness;Ghibly_Softness;10;0;Create;True;0;0;0;False;0;False;0.58;0.578;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;295;18595.32,-2606.234;Inherit;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;294;18574.22,-2713.136;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;480;19068.76,-2493.793;Inherit;False;478;MainColTinted;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;390;22599.68,-964.7523;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;282;18707.5,-2269.01;Inherit;True;JP_GibliShading;-1;;12;d3a1b5653f8d8224e8198a805145c270;0;5;19;COLOR;0.7035422,0.9622642,0.801755,0;False;20;COLOR;0.1891242,0.3701392,0.4716981,0;False;21;FLOAT;1;False;25;FLOAT;0.5;False;22;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;266;18833.99,-2610.755;Inherit;False;Property;_GiblyStylize;GiblyStylize;8;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;291;18655.44,-2484.306;Inherit;False;Property;_GhiblyIntensity;GhiblyIntensity;7;0;Create;True;0;0;0;False;0;False;0;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;392;22455.86,-1089.715;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;283;19151.74,-2216.865;Inherit;False;Property;_OverlayFactor;OverlayFactor;14;0;Create;True;0;0;0;False;0;False;0;0.481;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;232;19360.97,-2548.964;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;391;22196.42,-1176.55;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;393;22873.38,-913.3403;Inherit;False;Property;_SSSPower;SSS Power;24;0;Create;True;0;0;0;False;0;False;0;2.4;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;376;19607.4,-2502.218;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;479;20451.44,-2627.401;Inherit;False;478;MainColTinted;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;296;19211.89,-2700.684;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;394;22607.01,-1154.037;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;396;22804.25,-1131.4;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;470;19635.07,-776.6808;Inherit;False;1926.23;760.283;Comment;16;305;306;307;314;313;308;304;309;406;378;303;312;311;381;379;377;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;395;23218.38,-914.3403;Inherit;False;Property;_SSSScale;SSS Scale;25;0;Create;True;0;0;0;False;0;False;0;2.9;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;297;20724.46,-2596.52;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;398;23040.38,-1123.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;429;22165.18,-4181.085;Inherit;False;2218.252;954.9104;Comment;15;428;407;430;437;438;439;444;449;461;482;454;448;457;455;456;Global_Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;305;19685.07,-511.9099;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;481;21099.88,-2586.578;Inherit;False;MixedGhiblyCol;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;511;21684.18,-2726.671;Inherit;False;2178.025;961.926;Comment;7;502;512;509;508;510;506;523;RandomBrightness;0.4716981,0.09122463,0.09122463,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;475;20180.77,-4025.805;Inherit;False;AO_Final;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;399;23639.08,-1001.927;Inherit;False;Constant;_Float5;Float 5;18;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;397;23480.48,-1149.689;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;407;22208.46,-3869.471;Inherit;False;Property;_GlobalTintScale;GlobalTintScale;28;0;Create;True;0;0;0;False;0;False;1024;1024;1;2048;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;400;23243.85,-1163.074;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;502;21716.65,-2498.671;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,-1.4;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;456;22722.42,-3821.565;Inherit;False;Constant;_Float9;Float 9;30;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;314;19882.65,-363.768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;307;20173.24,-223.9749;Inherit;False;Property;_WindSpeed;WindSpeed;17;0;Create;True;0;0;0;False;0;False;1;0.23;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;437;22264.13,-3680.217;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TimeNode;306;19993.24,-716.9748;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;506;21779.65,-2161.671;Inherit;False;Property;_RandomizeColorFactor;RandomizeColorFactor;30;0;Create;True;0;0;0;False;0;False;0;36.4;0.0001;64;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;483;23522.9,-1336.307;Inherit;False;481;MixedGhiblyCol;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;477;23534,-1520.626;Inherit;False;475;AO_Final;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;405;23266.98,-1519.936;Inherit;False;Property;_SubSurfaceTint;SubSurfaceTint;22;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;602.8622,676.2264,625.2357,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;401;23771.93,-1122.577;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;290;20800.86,-2332.564;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;308;20284.24,-671.9748;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;455;22910.37,-3807.36;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;438;22473.81,-3677.182;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;313;20035.85,-445.3121;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;403;23748.68,-1458.956;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;524;21977.87,-2467.706;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;457;23096.13,-3778.95;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;523;22202.2,-2331.226;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;500;23968.07,-1468.598;Inherit;False;FinalSubSurface;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;498;21100.67,-2300.66;Inherit;False;ClearGhibly;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;304;20182.28,-321.7109;Inherit;False;Property;_WindScale;WindScale;16;0;Create;True;0;0;0;False;0;False;1;7.7;0;128;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;309;20368.24,-480.9749;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;497;23990.14,-2979.538;Inherit;False;1692.32;573.2021;Comment;11;488;490;463;462;453;476;451;404;501;499;513;PreFinalMixing;0.6981132,0.03622286,0.606012,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;510;22964.09,-2398.605;Inherit;False;Property;_RandomizerDarkness;RandomizerDarkness;31;0;Create;True;0;0;0;False;0;False;1;0.356;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;312;20181.94,-132.398;Inherit;False;Property;_WindPower;WindPower;18;0;Create;True;0;0;0;False;0;False;1;0.243;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;378;20787.47,-273.4529;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;501;24047.68,-2694.903;Inherit;False;500;FinalSubSurface;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;303;20530.51,-675.082;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;471;18527.38,-603.7629;Inherit;False;875.7617;600.9888;Comment;5;467;229;466;242;468;Billboards;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;499;24031.23,-2597.025;Inherit;False;498;ClearGhibly;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;439;22574.31,-3586.347;Inherit;True;Property;_GlobalMask;GlobalMask;29;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;406;20693.99,-457.2101;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;482;22308.54,-3428.792;Inherit;False;481;MixedGhiblyCol;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;509;23180.99,-2636.946;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;448;23789.12,-3652.455;Inherit;False;Constant;_Float7;Float 7;30;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;449;23423.01,-3700.017;Inherit;False;Constant;_Float8;Float 8;30;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;467;18577.38,-377.8523;Inherit;False;Property;_BillBoardNoiseRemaper;BillBoardNoiseRemaper;5;0;Create;True;0;0;0;False;0;False;0,1,1,1;0,1,-18.52,1.52;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;404;24273.14,-2637.489;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;229;18584.89,-553.7629;Inherit;False;Property;_BillBoardOffset;BillBoardOffset;3;0;Create;True;0;0;0;False;0;False;0;0.04;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;466;18583.38,-468.8522;Inherit;False;Property;_BillBoardNoisePower;BillBoardNoisePower;4;0;Create;True;0;0;0;False;0;False;0;2.88;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;381;21082.94,-303.811;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;508;23389.57,-2623.293;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;311;20885.38,-716.268;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;242;18583.06,-118.7741;Inherit;False;Property;_BillboardScale;BillboardScale;2;0;Create;True;0;0;0;False;0;False;1;1.02;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;430;23612.15,-3754.68;Inherit;False;Property;_GlobalTint_Enable;GlobalTint_Enable;26;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;428;22833.42,-4147.009;Inherit;False;Property;_Global_Blend;Global_Blend;27;0;Create;True;0;0;0;False;0;False;0.524;0.524;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;461;23076.49,-3563.664;Inherit;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;379;21399.3,-335.7729;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;454;23906.11,-3800.081;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;512;23530.85,-2602.367;Inherit;False;RandomDarkness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;468;19038.14,-367.2264;Inherit;True;JP_FacesToBillboards;-1;;21;ef7aae7e8f97991499719944f9735b51;0;4;19;FLOAT;0;False;29;FLOAT;1;False;33;FLOAT4;0,1,1,1;False;20;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;451;24330.26,-2929.538;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;485;20701.7,-1100.329;Inherit;False;877.041;203.2383;Comment;3;293;310;484;VertexOffsetMixed;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;513;24659.43,-2548.847;Inherit;False;512;RandomDarkness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;377;21215.17,-726.6808;Inherit;False;Property;_UV_as_WindMask;UV_as_WindMask;20;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;453;24584.72,-2816.24;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;473;24270.27,-1974.694;Inherit;False;905.6836;205.5546;Comment;2;493;375;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;476;24661.1,-2640.626;Inherit;False;475;AO_Final;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;293;20751.7,-1050.329;Inherit;False;Property;_FaceCameraBillboard;FaceCameraBillboard;15;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;444;23494.16,-3550.664;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;462;24996.72,-2727.259;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;463;24956.79,-2897.62;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;310;21128.03,-1032.091;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;375;24320.27,-1924.694;Inherit;False;Constant;_Float3;Float 3;20;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;490;25140.67,-2896.173;Inherit;False;FinalAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;484;21354.74,-1028.148;Inherit;False;VertexFinal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;495;19614.02,-3616.166;Inherit;False;FinalOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;493;24597.42,-1925.213;Inherit;False;FinalSmoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;488;25140.67,-2735.173;Inherit;False;FinalEmmision;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;494;25625.92,-2680.138;Inherit;False;493;FinalSmoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;487;25662.06,-2370.701;Inherit;False;484;VertexFinal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;489;25646.67,-2760.173;Inherit;False;488;FinalEmmision;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;496;25634.58,-2452.148;Inherit;False;495;FinalOpacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;491;25647.67,-2867.173;Inherit;False;490;FinalAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;492;25648.49,-2592.801;Inherit;False;475;AO_Final;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;175;26445.75,-2853.87;Float;False;True;-1;7;ASEMaterialInspector;0;0;Standard;ArtEquelibrium/JP_Vegetation_Leafs_Base;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;True;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.86;True;True;0;False;TransparentCutout;;AlphaTest;All;18;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Pragma;multi_compile __ LOD_FADE_CROSSFADE;False;;Custom;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;374;1;372;0
WireConnection;374;0;373;0
WireConnection;382;0;383;0
WireConnection;382;1;219;0
WireConnection;382;2;384;0
WireConnection;213;1;374;0
WireConnection;220;0;382;0
WireConnection;220;1;213;0
WireConnection;235;0;220;0
WireConnection;235;1;236;0
WireConnection;389;0;386;0
WireConnection;389;1;387;0
WireConnection;478;0;235;0
WireConnection;390;0;388;0
WireConnection;390;1;389;0
WireConnection;282;19;259;0
WireConnection;282;20;260;0
WireConnection;282;21;272;0
WireConnection;282;25;271;0
WireConnection;282;22;256;0
WireConnection;266;1;294;0
WireConnection;266;0;295;0
WireConnection;392;0;390;0
WireConnection;232;0;480;0
WireConnection;232;1;282;0
WireConnection;376;0;282;0
WireConnection;376;1;232;0
WireConnection;376;2;283;0
WireConnection;296;0;266;0
WireConnection;296;1;291;0
WireConnection;394;0;391;0
WireConnection;394;1;392;0
WireConnection;396;0;394;0
WireConnection;396;1;393;0
WireConnection;297;0;479;0
WireConnection;297;1;376;0
WireConnection;297;2;296;0
WireConnection;398;0;396;0
WireConnection;398;1;395;0
WireConnection;481;0;297;0
WireConnection;475;0;382;0
WireConnection;400;0;398;0
WireConnection;314;0;305;2
WireConnection;401;0;397;0
WireConnection;401;1;399;0
WireConnection;290;1;282;0
WireConnection;290;2;296;0
WireConnection;308;0;306;1
WireConnection;308;1;307;0
WireConnection;455;0;456;0
WireConnection;455;1;407;0
WireConnection;438;0;437;1
WireConnection;438;1;437;3
WireConnection;313;0;305;1
WireConnection;313;1;314;0
WireConnection;313;2;305;3
WireConnection;403;0;401;0
WireConnection;403;1;400;0
WireConnection;403;2;405;0
WireConnection;403;3;477;0
WireConnection;403;4;483;0
WireConnection;524;0;502;0
WireConnection;524;1;506;0
WireConnection;457;0;455;0
WireConnection;457;1;438;0
WireConnection;523;0;524;0
WireConnection;523;1;506;0
WireConnection;500;0;403;0
WireConnection;498;0;290;0
WireConnection;309;0;313;0
WireConnection;309;1;308;0
WireConnection;303;0;309;0
WireConnection;303;1;304;0
WireConnection;439;1;457;0
WireConnection;509;0;523;0
WireConnection;509;3;510;0
WireConnection;404;0;501;0
WireConnection;404;1;499;0
WireConnection;381;0;378;2
WireConnection;508;0;509;0
WireConnection;311;0;303;0
WireConnection;311;1;312;0
WireConnection;311;2;406;1
WireConnection;430;1;448;0
WireConnection;430;0;449;0
WireConnection;461;0;482;0
WireConnection;461;1;439;0
WireConnection;379;0;311;0
WireConnection;379;1;381;0
WireConnection;454;0;428;0
WireConnection;454;1;430;0
WireConnection;512;0;508;0
WireConnection;468;19;229;0
WireConnection;468;29;466;0
WireConnection;468;33;467;0
WireConnection;468;20;242;0
WireConnection;451;0;461;0
WireConnection;451;1;404;0
WireConnection;377;1;311;0
WireConnection;377;0;379;0
WireConnection;453;0;404;0
WireConnection;453;1;451;0
WireConnection;453;2;454;0
WireConnection;293;0;468;0
WireConnection;444;0;482;0
WireConnection;444;1;461;0
WireConnection;444;2;454;0
WireConnection;462;0;453;0
WireConnection;462;1;476;0
WireConnection;462;2;513;0
WireConnection;463;0;444;0
WireConnection;463;1;476;0
WireConnection;463;2;513;0
WireConnection;310;0;293;0
WireConnection;310;1;377;0
WireConnection;490;0;463;0
WireConnection;484;0;310;0
WireConnection;495;0;213;4
WireConnection;493;0;375;0
WireConnection;488;0;462;0
WireConnection;175;0;491;0
WireConnection;175;2;489;0
WireConnection;175;4;494;0
WireConnection;175;5;492;0
WireConnection;175;10;496;0
WireConnection;175;11;487;0
ASEEND*/
//CHKSM=1D9C37F77D5345B24D239F2ECC882121DF6BE277