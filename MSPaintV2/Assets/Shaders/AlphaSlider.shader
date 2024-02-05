Shader "Unlit/AlphaSlider"
{
    Properties
    {
        //[HideInInspector][NoScaleOffset] _MainTex("Grid Texture", 2D) = "white"{}
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
                float3 textureCoordinate : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Alpha;
            float _F;
            sampler2D _GridTex;
            float4 _Rgb;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 screenPosition = ComputeScreenPos(o.vertex);
                o.textureCoordinate = float3((screenPosition.xy / screenPosition.w) * 35, v.uv.y);
                o.textureCoordinate.x *= (_ScreenParams.x / _ScreenParams.y);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 textureCol = tex2D(_GridTex, i.textureCoordinate.xy);
                return (textureCol * (1 - i.textureCoordinate.z) + _Rgb * i.textureCoordinate.z) * _F;
            }
            ENDCG
        }
    }
}
