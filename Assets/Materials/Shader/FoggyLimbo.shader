Shader "Unlit/FoggyLimbo"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}
        _PlayerPosition ("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _FogStrength ("Fog Strength", Range(0,1)) = 0.5
        _FogSpeed ("Fog Speed", Vector) = (0.02, 0.01, 0, 0)
        _Tiling ("Tiling", Vector) = (1, 1, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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
            float4 _FogSpeed;
            float4 _Tiling;

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
                // Move UV based on time for scrolling fog
                float2 movingUV = i.uv * _Tiling.xy + (_Time.y * _FogSpeed.xy);

                // Sample the texture
                fixed4 tex = tex2D(_MainTex, movingUV);

                // Calculate fog intensity based on distance to player
                float dist = distance(i.uv, _PlayerPosition.xy);
                float intensity = saturate(1.0 - dist * 2.0) * _FogStrength;

                // Final color with alpha based on fog intensity
                tex.a *= intensity;

                return tex;
            }
            ENDCG
        }
    }
}
