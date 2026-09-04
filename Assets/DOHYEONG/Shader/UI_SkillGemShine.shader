Shader "UI/SkillGemShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shine)]
        _ShineColor ("Shine Color", Color) = (1,0.9,0.55,1)
        _ShineStrength ("Shine Strength", Range(0, 3)) = 1
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.12
        _ShineSpeed ("Shine Speed", Range(-5, 5)) = 0.5
        _ShineAngle ("Shine Angle", Range(-3.14, 3.14)) = 0.6

        [HideInInspector]
        _StencilComp ("Stencil Comparison", Float) = 8

        [HideInInspector]
        _Stencil ("Stencil ID", Float) = 0

        [HideInInspector]
        _StencilOp ("Stencil Operation", Float) = 0

        [HideInInspector]
        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        [HideInInspector]
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector]
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _ShineColor;

            float _ShineStrength;
            float _ShineWidth;
            float _ShineSpeed;
            float _ShineAngle;

            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 원본 UI Sprite
                fixed4 sprite = tex2D(_MainTex, i.texcoord) * i.color;

                // 방향 벡터
                float2 direction = float2(
                    cos(_ShineAngle),
                    sin(_ShineAngle)
                );

                // UV를 중심 기준으로 변환
                float2 centeredUV = i.texcoord - 0.5;

                // 빛의 진행 위치
                float shinePosition =
                    frac(_Time.y * _ShineSpeed) * 2.0 - 1.0;

                float projected =
                    dot(centeredUV, direction);

                float distanceFromShine =
                    abs(projected - shinePosition);

                // 부드러운 빛 띠
                float shine =
                    1.0 - smoothstep(
                        0.0,
                        _ShineWidth,
                        distanceFromShine
                    );

                // ★ Sprite가 투명한 곳에는 Shine도 나오지 않음
                shine *= sprite.a;

                sprite.rgb +=
                    _ShineColor.rgb *
                    shine *
                    _ShineStrength;

                #ifdef UNITY_UI_CLIP_RECT
                sprite.a *= UnityGet2DClipping(
                    i.worldPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(sprite.a - 0.001);
                #endif

                return sprite;
            }

            ENDCG
        }
    }
}