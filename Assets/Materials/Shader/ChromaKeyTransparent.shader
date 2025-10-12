Shader "Custom/SpriteChromaKey_Discard"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _KeyColor ("Key Color", Color) = (0,1,0,1) // ברירת מחדל: ירוק
        _Threshold ("Color Threshold", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Lighting Off
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _KeyColor;
            float _Threshold;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord);
                float d = distance(c.rgb, _KeyColor.rgb);

                if (d < _Threshold)
                {
                    discard; // מוחק את הפיקסל לגמרי
                }

                return c;
            }
            ENDCG
        }
    }
}
