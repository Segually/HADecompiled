Shader "Mobile/DiffuseColorONLY"
{
	Properties
	{
		_Color ("Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 150

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		fixed4 _Color;

		struct Input
		{
			half dummy;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}

		ENDCG
	}

	Fallback "Mobile/VertexLit"
}
