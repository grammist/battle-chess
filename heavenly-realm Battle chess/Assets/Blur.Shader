Shader "Custom/SceneBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _BlurSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 color = tex2D(_MainTex, uv);
                float2 offset = float2(_BlurSize / 100.0, _BlurSize / 100.0);

                color += tex2D(_MainTex, uv + offset);
                color += tex2D(_MainTex, uv - offset);
                color += tex2D(_MainTex, uv + float2(offset.x, -offset.y));
                color += tex2D(_MainTex, uv + float2(-offset.x, offset.y));

                color /= 5;
                return color;
            }
            ENDCG
        }
    }
}
