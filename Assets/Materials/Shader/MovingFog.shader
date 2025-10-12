Shader "Custom/2D_Fog_Animated"
{
    Properties
    {
        _MainTex ("Fog Sprite Sheet", 2D) = "white" {}
        _Rows ("Rows", Float) = 4
        _Columns ("Columns", Float) = 4
        _Speed ("Speed (Frames/Sec)", Float) = 4
        _Direction ("Direction (X,Y)", Vector) = (1, 0, 0, 0)
        _Alpha ("Alpha", Range(0,1)) = 0.5
       
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rows;
            float _Columns;
            float _Speed;
            float4 _Direction;
            float _Alpha;

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

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float totalFrames = _Rows * _Columns;
                float frame = floor(_Time.y * _Speed);
                float frameIndex = fmod(frame, totalFrames);

                float2 uv = i.uv;

                // תזוזה עם הזמן
                uv += _Time.y * _Direction.xy * 0.05;

                // מיקום פריים בטקסטורה
                float2 cellSize = float2(1.0 / _Columns, 1.0 / _Rows);
                float2 offset = float2(fmod(frameIndex, _Columns), floor(frameIndex / _Columns)) * cellSize;

                uv = frac(uv); // לוודא שה-uv תמיד בתוך הטווח [0,1]
                uv = uv * cellSize + offset;

                fixed4 col = tex2D(_MainTex, uv);
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}
