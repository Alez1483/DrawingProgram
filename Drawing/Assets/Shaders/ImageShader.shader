Shader "Unlit/ImageShader"
{
    Properties
    {
        [HideInInspector][NoScaleOffset] _MainTex("Main Texture", 2D) = "white"{}
        //[NoScaleOffset] _GridTex("Grid Texture", 2D) = "white"{}
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
                float4 textureCoordinate : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _GridTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 screenPosition = ComputeScreenPos(o.vertex);
                o.textureCoordinate = float4((screenPosition.xy / screenPosition.w) * 35, v.uv.xy);
                o.textureCoordinate.x *= (_ScreenParams.x / _ScreenParams.y);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 textureCol = tex2D(_MainTex, i.textureCoordinate.zw);
                float3 gridColor = tex2D(_GridTex, i.textureCoordinate.xy).rgb;
                return float4(gridColor * (1 - textureCol.a) + textureCol.rgb * textureCol.a, 1);
            }
            ENDCG
        }
    }
}
