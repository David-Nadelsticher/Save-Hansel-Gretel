Shader "Unlit/GlobalDarkness\"
{
    Properties
    {
        _Color ("Dark Color\", Color) = (0,0,0,0.5)
        _Darkness ("Darkness\", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue\"="Overlay\" "RenderType\"="Transparent\" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Darkness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = _Color;
                c.a *= _Darkness;
                return c;
            }
            ENDCG
        }
    }
}
