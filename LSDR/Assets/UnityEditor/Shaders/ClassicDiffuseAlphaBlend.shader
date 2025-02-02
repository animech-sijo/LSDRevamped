Shader "LSDR/ClassicDiffuseAlphaBlend" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1, 1, 1, 1)
        [PerRendererData]_FogAddition ("FogAddition", Color) = (0, 0, 0)
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // incoming vertices
            struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            // data from vert to frag shader
            struct classicV2F
            {
                fixed4 pos : SV_POSITION;
                half4 color : COLOR0;
                float2 uv_MainTex : TEXCOORD0;
                half3 normal : TEXCOORD1;
            };
            
            uniform float _AffineIntensity;
            
            classicV2F vert(appdata v)
            {
                classicV2F output;
                
                // vertex snapping
                float4 snapToPixel = UnityObjectToClipPos(v.position);
                snapToPixel.xyz = snapToPixel.xyz / snapToPixel.w;
                snapToPixel.x = floor(160 * snapToPixel.x) / 160;
                snapToPixel.y = floor(120 * snapToPixel.y) / 120;
                snapToPixel.xyz *= snapToPixel.w;
                output.pos = snapToPixel;
                
                // vertex color
                output.color = v.color;
                
                float distance = length(UnityObjectToViewPos(v.position));
                
                // affine texture mapping
                output.uv_MainTex = v.uv;
                output.uv_MainTex *= distance + (v.position.w * _AffineIntensity * 8) / distance / 2;
                output.normal = distance + (v.position.w * _AffineIntensity * 8) / distance / 2;
                
                return output;
            }
            
            sampler2D _MainTex;
            fixed4 _Tint;
            fixed4 _FogAddition;
            
            float4 frag(classicV2F input) : COLOR
            {
                half4 output = tex2D(_MainTex, input.uv_MainTex / input.normal.x);
                
                // apply vertex color
                output *= input.color;
                
                // apply tint
                output *= _Tint;
                
                // apply fog
                output += half4(_FogAddition.r, _FogAddition.g, _FogAddition.b, 0);
                
                return output;
            }
            
            ENDCG
        }
    }
}