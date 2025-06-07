Shader "Unlit/Two Layer Composite"
{
    // Author: Glenn Storm
    // composite two-layer unlit with transparency, rendered both sides
    // _LineArt outline,
    // _AltFill fill image, _AltCol color property for alt fill
    // _Maintex fill image, _Color property for fill

    Properties
    {
        _LineArt ("Texture", 2D) = "white" {}
        _AltFill ("Textue", 2D) = "white" {}
        _AltCol ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "QUEUE"="Transparent"
            "IGNOREPROJECTOR"="true"
            "RenderType"="Transparent"
        }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _LineArt;
            sampler2D _AltFill;
            sampler2D _MainTex;
            float4 _LineArt_ST;
            float4 _AltFill_ST;
            float4 _MainTex_ST;
            float4 _AltCol;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample fill texture
                fixed4 lin = tex2D(_LineArt, i.uv);
                fixed4 alt = tex2D(_AltFill, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply alt fill color
                alt = alt * _AltCol;
                // apply fill color
                col = col * _Color;
                // lay alt on top of main
                col = lerp(col,alt,alt.a);
                // lay line on top
                col = lerp(col,lin,lin.a);
                // clamp add alpha from all
                col.a = clamp(lin.a + alt.a + col.a,0,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
