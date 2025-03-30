Shader "Unlit/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 1)) = 1
        _BlurAmount ("Blur Amount", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        LOD 100
        Cull Back
        ZTest Always
        blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurAmount;
            sampler2D _MaskTex;
            fixed4 _Color;
            fixed _Intensity;

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 pixel;
            half2 uv;
            int i = 0;
            float iBlur1;
            float iBlur2;


            float rand(float i, float j, float k)
            {
                float x = frac(sin(dot(float3(i, j, k), float3(12.9898, 78.233, 151.7182))) * 43758.5453) * 2 - 1;
                return x * x * x;
            }

            half4 frag(v2f input) : SV_Target
            {
                half2 uv = input.uv;
                pixel = 0;
                fixed4 maskColor = tex2D(_MaskTex, uv);
                fixed alpha = maskColor.a * _Intensity;
                fixed blur_amount = alpha * _BlurAmount;

                //下面随机采样6个半径在_BlurAmount范围内的点
                for (i = 0; i < 6; i++)
                {
                    iBlur1 = blur_amount * rand(i, input.position.x, input.position.y);
                    iBlur2 = blur_amount * rand(input.position.x, input.position.y, i);
                    pixel += tex2D(_MainTex, uv + float2(iBlur1, iBlur2));
                }
                //下面平均地采样周围半径在_BlurAmount范围内的6个点, 每次旋转60度
                for (i = 0; i < 6; i++)
                {
                    iBlur1 = blur_amount * cos(i * 1.0472);
                    iBlur2 = blur_amount * sin(i * 1.0472);
                    pixel += tex2D(_MainTex, uv + float2(iBlur1, iBlur2) * 0.5) * 0.5;
                }
                pixel /= 9;
                pixel.rgb = maskColor.rgb * alpha * _Color.rgb + pixel.rgb * (1 - alpha);
                return pixel;
            }
            ENDCG
        }
    }
}