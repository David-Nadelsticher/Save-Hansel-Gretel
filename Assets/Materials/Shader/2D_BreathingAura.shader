Shader "Custom/2D_BreathingAura"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0, 1, 0, 1)
        _BaseRadius ("Base Radius", Range(0,1)) = 0.4
        _BreathAmount ("Breath Amplitude", Range(0,0.5)) = 0.1
        _BreathSpeed ("Breath Speed", Float) = 2.0
        _Intensity ("Glow Intensity", Range(0,5)) = 2.0
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

            fixed4 _GlowColor;
            float _BaseRadius;
            float _BreathAmount;
            float _BreathSpeed;
            float _Intensity;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0; // ממרכז ל [-1,1]
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.uv);

                // חישוב נשימה: רדיו משתנה עם הזמן
                float pulse = sin(_Time.y * _BreathSpeed) * _BreathAmount;
                float radius = _BaseRadius + pulse;

                float glow = 1.0 - smoothstep(radius, 1.0, dist);
                glow *= _Intensity;

                fixed4 col = _GlowColor;
                col.a *= glow;

                return col;
            }
            ENDCG
        }
    }
}
