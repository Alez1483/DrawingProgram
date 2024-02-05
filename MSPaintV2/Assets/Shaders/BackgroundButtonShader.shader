Shader "Unlit/BackgroundButtonShader"
{
    Properties
    {
        //[NoScaleOffset] _MainTex("Main Texture", 2D) = "white"{}
        _Color("Main Color", Color) = (1,1,1,1)
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
            };

            struct v2f
            {
                float2 textureCoordinate : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _GridTex;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 screenPosition = ComputeScreenPos(o.vertex);
                o.textureCoordinate = (screenPosition.xy / screenPosition.w) * 35;
                o.textureCoordinate.x *= (_ScreenParams.x / _ScreenParams.y);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(lerp(tex2D(_GridTex, i.textureCoordinate).rgb, _Color.rgb, _Color.a), 1);
            }
            ENDCG
        }
    }
}
