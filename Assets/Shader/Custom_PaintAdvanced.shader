Shader "Custom/PaintAdvanced"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_PatternTex ("Pattern (RGB)", 2D) = "white" {}
		_SideTex ("Side (RGB)", 2D) = "white" {}
		_SymbolTex ("Symbol (RGB)", 2D) = "white" {}
		_ColorA ("ColorA", Color) = (0,0,0,1)
		_ColorB ("ColorB", Color) = (0,0,0,1)
		_ColorC ("ColorC", Color) = (0,0,0,1)
		_ColorD ("ColorD", Color) = (0,0,0,1)
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		ZTest [_ZTest]

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _PatternTex;
		sampler2D _SideTex;
		sampler2D _SymbolTex;
		fixed4 _ColorA;
		fixed4 _ColorB;
		fixed4 _ColorC;
		fixed4 _ColorD;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv2_PatternTex;
			float2 uv3_SideTex;
			float2 uv4_SymbolTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 baseCol = tex2D(_MainTex, IN.uv_MainTex);
			fixed symbolMask = tex2D(_SymbolTex, IN.uv4_SymbolTex).r;

			if (symbolMask < 0.5)
			{
				o.Albedo = baseCol.rgb * _ColorD.rgb;
			}
			else
			{
				fixed sideMask = tex2D(_SideTex, IN.uv3_SideTex).r;
				if (sideMask < 0.5)
				{
					o.Albedo = baseCol.rgb * _ColorC.rgb;
				}
				else
				{
					fixed pattern = tex2D(_PatternTex, IN.uv2_PatternTex).r;
					fixed3 paint = lerp(_ColorB.rgb, _ColorA.rgb, pattern);
					o.Albedo = baseCol.rgb * paint;
				}
			}

			o.Alpha = 1;
		}

		ENDCG
	}

	Fallback "Diffuse"
}
