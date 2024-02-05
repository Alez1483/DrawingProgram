Shader "Unlit/SquareShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Back
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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
                o.uv = v.uv;
                return o;
            }

            float3 _Hsv;

            float4 frag(v2f i) : SV_Target
            {
                return float4(lerp(1, saturate(3.0 * abs(1.0 - 2.0 * frac(_Hsv.x + float3(0.0,-1.0 / 3.0,1.0 / 3.0))) - 1), i.uv.x) * i.uv.y , 1);
                //return float4((((_Full - 1) * i.uv.x + 1) * i.uv.y), 1);
            }
            ENDCG
        }
    }
}
