Shader "Custom/BuildGlowCutout"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_EmissionColor ("Color", Color) = (0,0,0,1)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200
		ZTest Always

		// Extra pass that renders to the depth buffer only
		Pass
		{
			ColorMask 0
		}

		CGPROGRAM
		#pragma surface surf Lambert alpha:fade

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _EmissionColor;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			o.Albedo = c.rgb;
			o.Emission = tex.a * c.rgb * _EmissionColor.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Transparent/Diffuse"
}
