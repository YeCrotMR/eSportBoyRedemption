Shader "Custom/SpriteOutline8"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Thickness ("Thickness", Range(0,1000)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height
            fixed4 _OutlineColor;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);

                // 如果像素透明，就去周围采样看看有没有不透明像素
                if (c.a == 0)
                {
                    float2 offset = _MainTex_TexelSize.xy * _Thickness;
                    fixed a = 0;

                    // 上下左右
                    a += tex2D(_MainTex, i.uv + float2( offset.x,  0)).a;
                    a += tex2D(_MainTex, i.uv + float2(-offset.x,  0)).a;
                    a += tex2D(_MainTex, i.uv + float2( 0,  offset.y)).a;
                    a += tex2D(_MainTex, i.uv + float2( 0, -offset.y)).a;

                    // 四个对角
                    a += tex2D(_MainTex, i.uv + float2( offset.x,  offset.y)).a;
                    a += tex2D(_MainTex, i.uv + float2(-offset.x,  offset.y)).a;
                    a += tex2D(_MainTex, i.uv + float2( offset.x, -offset.y)).a;
                    a += tex2D(_MainTex, i.uv + float2(-offset.x, -offset.y)).a;

                    if (a > 0) return _OutlineColor;
                }

                return c;
            }
            ENDCG
        }
    }
}
