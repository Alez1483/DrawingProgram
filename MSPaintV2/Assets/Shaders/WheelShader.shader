Shader "Unlit/WheelShader"
{
    Properties
    {
        //_MainTex("Sprite Texture", 2D) = "white" {}
        _Thick("Thickness of Ring", Range(0, 1)) = 0.25
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct MeshData
                {
                    float4 vertex   : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                float _Thick;
                float3 _Hsv;

                v2f vert(MeshData v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = (v.uv * -2) + 1;
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float halfThick = _Thick * 0.5;
                    float dst = abs((1 - halfThick) - length(i.uv));
                    float pixelSize = ddx(i.uv.x) * -2;
                    float a = smoothstep(halfThick, halfThick - pixelSize, dst);
                    clip(a - 0.001);
                    float angle = (atan2(i.uv.x, i.uv.y) + 3.14159265359) * 0.15915494309; //0 to 1
                    float3 col = lerp(1, saturate(3.0 * abs(1.0 - 2.0 * frac(angle + float3(0.0, -1.0 / 3.0, 1.0 / 3.0))) - 1), _Hsv.y) * _Hsv.z;
                    return float4(col, a);
                }
                ENDCG
            }
        }
}