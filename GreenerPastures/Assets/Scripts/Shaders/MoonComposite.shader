Shader "Unlit/Moon Composite"
{
    // Author: Glenn Storm
    // composite one-layer unlit with transparency, rendered both sides
    // _Shadow overlay color to be shaped,
    // _Maintex moon underlay image, _Color property for ul image

    Properties
    {
        _Shadow ("Moon Shadow", 2D) = "white" {}
        _MainTex ("Moon Texture", 2D) = "white" {}
        _Color ("Moon Color", Color) = (1,1,1,1)
        _MoonPhase ("Moon Phase", Float) = 0
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

            sampler2D _Shadow;
            sampler2D _MainTex;
            float4 _Shadow_ST;
            float4 _MainTex_ST;
            float4 _Color;
            float _MoonPhase;

            float2 scaleLeft( float2 uv, float scale )
            {
                float2 ret;

                ret.x = ((1-scale) * .5) + (uv.x * scale);

                if (scale > 0 && ret.x >= .5)
                    ret.x = 1;

                ret.y = uv.y;
                ret = clamp(ret, 0.0, 1.0);

                return ret;
            }

            float2 scaleRight( float2 uv, float scale )
            {
                float2 ret;

                ret.x = ((1-scale) * .5) + (uv.x * scale);

                if (scale > 0 && ret.x <= .5)
                    ret.x = 0;

                ret.y = uv.y;
                ret = clamp(ret, 0.0, 1.0);

                return ret;
            }

            float2 cullLeft( float2 uv )
            {
                float2 ret;

                ret.x = uv.x;
                if (ret.x <= .5)
                    ret.x = 0;

                ret.y = uv.y;
                ret = clamp(ret, 0.0, 1.0);

                return ret;
            }

            float2 cullRight( float2 uv )
            {
                float2 ret;

                ret.x = uv.x;
                if (ret.x >= .5)
                    ret.x = 1;

                ret.y = uv.y;
                ret = clamp(ret, 0.0, 1.0);

                return ret;
            }

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
                // store moon image
                fixed4 moon = tex2D(_MainTex, i.uv);
                // store full circle shadow
                fixed4 fullShadow = tex2D(_Shadow, i.uv);
                fullShadow = fullShadow * moon;
                
                // TODO: get 'cull' functions to properly remove half
                // store half circle shadows (left and right)
                cullRight(i.uv);
                fixed4 leftShadow = tex2D(_Shadow, i.uv);
                leftShadow = leftShadow * fullShadow;
                cullLeft(i.uv);
                fixed4 rightShadow = tex2D(_Shadow, i.uv);
                rightShadow = rightShadow * fullShadow;

                // convert _MoonPhase to four quarters of progress
                float edgeProgress;
                float midProgress;
                if ( _MoonPhase < .25 )
                {
                    edgeProgress = 2 * _MoonPhase;
                    midProgress = 0;
                }
                else if ( _MoonPhase >= .75 )
                {
                    edgeProgress = 2 * (_MoonPhase - .5);
                    midProgress = 1;
                }
                if ( _MoonPhase >= .25 && _MoonPhase < .5 )
                {
                    midProgress = 2 * _MoonPhase;
                    edgeProgress = .5;
                }
                else if ( _MoonPhase >= .5 && _MoonPhase < .75 )
                {  
                    midProgress = 2 * (_MoonPhase - .5);
                    edgeProgress = .5;
                }

                // left edge shadow cutout
                if (_MoonPhase < .25)
                    i.uv = scaleLeft(i.uv, 1/clamp(((1-edgeProgress)-.5)*2,0,1));
                // sample fill texture
                fixed4 shadow = tex2D(_Shadow, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply shadow color
                col = col * shadow;
                // cut shadow with fill
                shadow.a = col.a;
                // store left shadow
                fixed4 leftEdgeShadow = shadow;

                // right mid shadow cutout
                if (_MoonPhase >= .25 && _MoonPhase < .5)
                    i.uv = scaleRight(i.uv, 1/clamp(((midProgress-.5)*2),0,1));
                shadow = tex2D(_Shadow, i.uv);
                col = tex2D(_MainTex, i.uv);
                // apply shadow color 
                col = col * shadow;
                // cut shadow with fill
                shadow.a = col.a;
                // store right shadow
                fixed4 rightMidShadow = rightShadow;
                // use as mask to cut full shadow
                rightMidShadow = rightShadow - shadow.a;

                // left mid shadow cutout
                if (_MoonPhase >= .5 && _MoonPhase < .75)
                    i.uv = scaleLeft(i.uv, 1/clamp(((1-midProgress)-.5)*2,0,1));
                shadow = tex2D(_Shadow, i.uv);
                col = tex2D(_MainTex, i.uv);
                // apply shadow color 
                col = col * shadow;
                // cut shadow with fill
                shadow.a = col.a;
                // store left shadow
                fixed4 leftMidShadow = leftShadow;
                // use as mask to cut full shadow
                leftMidShadow = leftShadow - shadow.a;

                // right edge shadow cutout
                if (_MoonPhase >= .75)
                    i.uv = scaleRight(i.uv, 1/clamp(((edgeProgress-.5)*2),0,1));
                // sample fill texture
                shadow = tex2D(_Shadow, i.uv);
                col = tex2D(_MainTex, i.uv);
                // apply shadow color
                col = col * shadow;
                // cut shadow with fill
                shadow.a = col.a;
                fixed4 rightEdgeShadow = shadow;

                // compose shadow mid and edge elements
                if (_MoonPhase >= .75)
                    shadow = rightEdgeShadow;
                else if (_MoonPhase < .25)
                    shadow = leftEdgeShadow;
                if (_MoonPhase >= .25 && _MoonPhase < .5)
                    shadow = rightMidShadow;
                else if (_MoonPhase >= .5 && _MoonPhase < .75)
                    shadow = leftMidShadow;

                // lay shadow on top of moon
                // TEMP disable  // moon = lerp(moon, shadow, shadow.a);
                
                // clamp add alpha from both/
                //moon.a = clamp(moon.a + col.a, 0, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, moon);
                return moon;
            }
            ENDCG
        }
    }
}
