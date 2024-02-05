Shader "Unlit/CircleShader"
{
    Properties
    {
        _Sqr("Squared Size of Sphere", Float) = 1
        _Res("Resolution of Image (width)", Float) = 3
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Sqr;
            float _Res;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = (v.uv-0.5)*_Res;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float frag (v2f i) : SV_Target
            {
                return (i.uv.x * i.uv.x + i.uv.y * i.uv.y) >= _Sqr;
            }
            ENDCG
        }
    }
}
