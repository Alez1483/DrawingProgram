Shader "Unlit/PickerIndicatorShader"
{
    Properties
    {
        [HideInInspector][NoScaleOffset] _MainTex("RT", 2D) = "white" {}
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
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 uvs      : TEXCOORD0;
                float2 textureCoordinate : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _GridTex;
            sampler2D _Main;
            float2 _ImgSz;
            float _Ddx;
            float4 _Color;
            float3 _CamBckgnd;

            v2f vert(appdata v)
            {
                v2f o;
                o.uvs = float4(v.uv, mul(unity_ObjectToWorld, v.vertex).xy / _ImgSz);
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 screenPosition = ComputeScreenPos(o.vertex);
                o.textureCoordinate = float2((screenPosition.xy / screenPosition.w) * 35);
                o.textureCoordinate.x *= (_ScreenParams.x / _ScreenParams.y);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uvs.xy;
                if (tex2D(_MainTex, uv).x > 0.5)
                {
                    discard;
                }

                float2 x = float2(_Ddx, -_Ddx);

                if (tex2D(_MainTex, uv + x).x < 0.5 && tex2D(_MainTex, uv - x).x < 0.5 &&
                    tex2D(_MainTex, uv + x.xx).x < 0.5 && tex2D(_MainTex, uv + x.yy).x < 0.5)
                {
                    float4 col = _Color;
                    float3 gridColor = tex2D(_GridTex, i.textureCoordinate).rgb;
                    return float4(gridColor * (1 - col.a) + col.rgb * col.a, 1);
                }
                float2 mUv = i.uvs.zw;
                float4 main = tex2D(_Main, mUv);
                float3 gridColor = tex2D(_GridTex, i.textureCoordinate).rgb;
                float3 col = gridColor * (1 - main.a) + main.rgb * main.a;
                if (mUv.x > 1 || mUv.x < 0 || mUv.y > 1 || mUv.y < 0) {
                    col = _CamBckgnd;
                }

                return float4(col < 0.5, 1);
                //float y = col.r + col.g + col.b < 1.5;
                //return float4(y, y, y ,1);
            }
            ENDCG
        }
    }
}