Shader "Unlit/RedSlider"
{
    Properties
    {
        
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

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float3 _Rgb;

            v2f vert (MeshData v)
            {
                v2f o;
                o.uv = v.uv.x;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.uv, _Rgb.g, _Rgb.b, 1);
            }
            ENDCG
        }
    }
}
