﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LSD/PSX/Transparent" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		
		Pass{
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting On
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc" 

	struct v2f
	{
		fixed4 pos : SV_POSITION;
		half4 color : COLOR0;
		half4 colorFog : COLOR1;
		half4 vertCol : COLOR2;
		float2 uv_MainTex : TEXCOORD0;
		half3 normal: TEXCOORD1;
	};

	float4 _MainTex_ST;
	uniform float _Cutoff;
	uniform half4 unity_FogStart;
	uniform half4 unity_FogEnd;
	uniform float AffineIntensity;

	v2f vert(appdata_full v)
	{
		v2f o;

		//Vertex snapping
		float4 snapToPixel = UnityObjectToClipPos(v.vertex);
		float4 vertex = snapToPixel;
		vertex.xyz = snapToPixel.xyz / snapToPixel.w;
		vertex.x = floor(160 * vertex.x) / 160;
		vertex.y = floor(120 * vertex.y) / 120;
		vertex.xyz *= snapToPixel.w;
		o.pos = vertex;


		//o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		//Vertex lighting 
		//o.color = float4(ShadeVertexLights(v.vertex, v.normal), 1.0);
		o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
		o.color *= v.color;
		o.vertCol = v.color;

		float distance = length(UnityObjectToViewPos(v.vertex));

		//Affine Texture Mapping
		float4 affinePos = vertex;//vertex;				
		o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv_MainTex *= distance + (vertex.w*(AffineIntensity * 8)) / distance / 2;
		o.normal = distance + (vertex.w*(AffineIntensity * 8)) / distance / 2;

		//Fog
		float4 fogColor = unity_FogColor;

		float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
		o.normal.g = fogDensity;
		o.normal.b = 1;

		o.colorFog = fogColor;
		o.colorFog.a = clamp(fogDensity,0,1);

		//Cut out polygons
		if (distance > unity_FogStart.z + unity_FogColor.a * 255)
		{
			o.pos /= 0;
		}


		return o;
	}

	sampler2D _MainTex;

	uniform float _FogStep;

	float4 frag(v2f IN) : COLOR
	{
		/*half4 c = tex2D(_MainTex, IN.uv_MainTex / IN.normal.r);
		half4 color = c*(IN.colorFog.a);
		float fogIntensity = (1 - IN.colorFog.a);
		float steppedFogIntensity = round(fogIntensity / _FogStep) * _FogStep;
		color.rgb += IN.colorFog.rgb*steppedFogIntensity;
		// step is 1 if c.a >= 0 (textured)
		float alpha = lerp(IN.vertCol.a, c.a, step(c.a, 0.1));
		half4 resColor = half4(c.rgb * IN.vertCol.rgb, alpha);*/
		
		half4 color = tex2D(_MainTex, IN.uv_MainTex / IN.normal.r);
		color *= IN.colorFog.a;
		float fogIntensity = (1 - IN.colorFog.a);
		float steppedFogIntensity = round(fogIntensity / _FogStep) * _FogStep;
		color.rgb += IN.colorFog * steppedFogIntensity;
		color *= IN.vertCol;
		
		return color;
	}
		ENDCG
	}
	}
}