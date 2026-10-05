Shader "Custom/CombinedOutlineGlowShader"
{
	Properties
	{
		_MainTex ("Base Texture", 2D) = "white" {}
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_OutlineThickness ("Outline Thickness", Range(0, 10)) = 1
		_GlowColor ("Glow Color", Color) = (1,1,1,1)
		_GlowOpacity ("Glow Opacity", Float) = 1
		_GlowSize ("Glow Size", Range(0, 10)) = 1
		_OutlineAlphaThreshold ("Outline Alpha Threshold", Range(0, 1)) = 0.9
		_OverlayTex ("Overlay Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		// Additive glow drawn only outside of the sprite's opaque area.
		Pass
		{
			Name "GlowPass"
			Blend One One
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 texelSize : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _GlowColor;
			half _GlowOpacity;
			half _GlowSize;
			half _OutlineThickness;
			half _OutlineAlphaThreshold;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.texelSize = float2(1.0 / 128.0, 1.0 / 128.0);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float alpha = tex2D(_MainTex, i.uv).a;

				// Neighbourhood coverage on a hexagon, used to cull the sprite interior.
				half outlineCheck = 0;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 1.0,       0.0)      * i.texelSize).a;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.5,       0.866025) * i.texelSize).a;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-0.5,       0.866025) * i.texelSize).a;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-1.0,       0.0)      * i.texelSize).a;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-0.5,      -0.866025) * i.texelSize).a;
				outlineCheck += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.5,      -0.866025) * i.texelSize).a;
				outlineCheck /= 6.0;

				if (alpha > 0 || outlineCheck > _OutlineAlphaThreshold)
					discard;

				const float2 directions[8] =
				{
					float2( 1.0,       0.0),
					float2( 0.707107,  0.707107),
					float2( 0.0,       1.0),
					float2(-0.707107,  0.707107),
					float2(-1.0,       0.0),
					float2(-0.707107, -0.707107),
					float2( 0.0,      -1.0),
					float2( 0.707107, -0.707107)
				};

				int steps = (int)min(max(_GlowSize * 2.0, 1.0), 5.0);
				half glow = 0;
				for (int d = 0; d < 8; d++)
				{
					float2 stepUV = directions[d] * _OutlineThickness * i.texelSize;
					for (int s = 1; s <= steps; s++)
					{
						half weight = (1.0 / steps) * (steps - s + 1);
						glow += tex2D(_MainTex, i.uv + stepUV * s).a * weight;
					}
				}

				return fixed4(_GlowColor.rgb, glow * _GlowOpacity);
			}
			ENDCG
		}

		// Outline around the sprite plus an optional premultiplied overlay.
		Pass
		{
			Name "MainOutlinePass"
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature_local _ USE_OVERLAY
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 overlayUV : TEXCOORD1;
				float2 texelSize : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _OverlayTex;
			float4 _MainTex_ST;
			float4 _OverlayTex_ST;
			fixed4 _OutlineColor;
			half _OutlineThickness;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.overlayUV = TRANSFORM_TEX(v.uv, _OverlayTex);
				o.texelSize = float2(1.0 / 128.0, 1.0 / 128.0);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				half outlineAlpha = 0;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 1.0,       0.0)      * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.707107,  0.707107) * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.0,       1.0)      * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-0.707107,  0.707107) * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-1.0,       0.0)      * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2(-0.707107, -0.707107) * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.0,      -1.0)      * i.texelSize).a;
				outlineAlpha += tex2D(_MainTex, i.uv + _OutlineThickness * float2( 0.707107, -0.707107) * i.texelSize).a;
				outlineAlpha = saturate(outlineAlpha * 0.625);

				half4 outline = half4(outlineAlpha > 0 ? _OutlineColor.rgb : half3(0, 0, 0), outlineAlpha);

				fixed4 baseCol = tex2D(_MainTex, i.uv);
				half4 col = baseCol.a > 0 ? baseCol : outline;

			#ifdef USE_OVERLAY
				fixed4 overlay = tex2D(_OverlayTex, i.overlayUV);
				return col * (1.0 - overlay.a) + overlay;
			#else
				return col;
			#endif
			}
			ENDCG
		}
	}
}
