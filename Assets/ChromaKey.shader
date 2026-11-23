Shader "Custom/ChromaKey"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _KeyColor ("Chroma Key Color", Color) = (0, 1, 0, 1) // 기본 초록색
        _Threshold ("Threshold (제거 범위)", Range(0, 1)) = 0.45
        _Smooth ("Smoothness (경계 부드러움)", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _KeyColor;
            float _Threshold;
            float _Smooth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // 초록색과의 거리 계산 후 투명 처리
                float diff = distance(col.rgb, _KeyColor.rgb);
                float alpha = smoothstep(_Threshold, _Threshold + _Smooth, diff);
                return fixed4(col.rgb, col.a * alpha);
            }
            ENDCG
        }
    }
}