Shader "Unlit/InfinitePixelSpiral"
{
    Properties
    {
        // NOTE: _MainTex is a dummy – we never sample it, but we need it
        // so Unity generates _MainTex_ST (tiling / offset).
        _MainTex("Dummy", 2D) = "white" {}
        _Rot     ("Rotation (cos,sin)", Vector) = (1,0,0,0)
        _LineCol("Line Color", Color)       = (0.7,0.25,0.85,1)
        _BackCol("Background Color", Color) = (0.05,0.05,0.08,1)
        _Steps  ("Palette Steps", Range(2,16)) = 4
        _Tight  ("Spiral Tightness", Range(0.05,0.6)) = 0.25
        _PixelBlocks("Pixel Blocks (per axis)", Range(8, 256)) = 64
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _LineCol, _BackCol;
            float  _Steps, _Tight;

            // Unity supplies _MainTex_ST because we declared _MainTex
            float4 _MainTex_ST;
            float2 _Rot;
            float _PixelBlocks;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // apply tiling + offset:  uv' = uv * scale + offset
                float2 c = (v.uv - 0.5);
                float2 r;
                r.x =  c.x * _Rot.x - c.y * _Rot.y;   // x' =  cos·x − sin·y
                r.y =  c.x * _Rot.y + c.y * _Rot.x;   // y' =  sin·x + cos·y
                float2 uv = r + 0.5;
                o.uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 qUV = floor(i.uv * _PixelBlocks) / _PixelBlocks;
                
                // centre into [-1,1] range
                float2 p = (qUV  - 0.5) * 2.0;

                // polar coordinates
                float angle  = atan2(p.y, p.x);
                float radius = length(p);

                // normalise angle to [0,1)
                float a = (angle + UNITY_PI) * (1/(2*UNITY_PI));   // 1/(2π)

                // logarithmic spiral phase
                float phase = frac(a + _Tight * log(radius + 1e-5));

                // posterise to palette steps
                float stripe = floor(phase * _Steps) / (_Steps - 1.0);

                return lerp(_BackCol, _LineCol, stripe);
            }
            ENDCG
        }
    }
}
