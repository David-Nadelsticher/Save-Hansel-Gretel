Shader "Unlit/Fog2D"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}
        _PlayerPosition ("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _FogStrength ("Fog Strength", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PlayerPosition;
            float _FogStrength;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float2 playerUV = _PlayerPosition.xy;
                float dist = distance(uv, playerUV);

                float fogFactor = saturate(1.0 - dist * 2.0);
                fogFactor *= _FogStrength;

                fixed4 tex = tex2D(_MainTex, uv);
                fixed4 col = tex;
                col.a *= fogFactor;

                return col;
            }
            ENDCG
        }
    }
}
