//
//  OutlineFill.shader
//  QuickOutline
//
//  Created by Chris Nolet on 2/21/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

Shader "Custom/Outline Fill"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineSubColor("Sub Color", Color) = (0, 0, 0, 1) // 新增渐变颜色属性
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+110"
            "RenderType" = "Transparent"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Fill"
            Cull Off
            ZTest [_ZTest]
            ZWrite Off
            Blend One zero
            ColorMask RGB

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 smoothNormal : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
                float dist : TEXCOORD0; // 新增顶点到模型中心的距离变量
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform fixed4 _OutlineColor;
            uniform fixed4 _OutlineSubColor; // 新增渐变颜色变量
            uniform float _OutlineWidth;

            v2f vert(appdata input)
            {
                v2f output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
                float3 viewPosition = UnityObjectToViewPos(input.vertex);
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

                output.position = UnityViewToClipPos(viewPosition + viewNormal * _OutlineWidth / 200.0);
                output.dist = length(viewPosition);
                output.color = _OutlineColor;

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float t = sin(input.dist * 3.1415926 + _Time.y * 2) * 0.5 + 0.5;
                fixed4 finalColor = lerp(_OutlineColor, _OutlineSubColor, t);
                return finalColor;
            }
            ENDCG
        }
    }
}