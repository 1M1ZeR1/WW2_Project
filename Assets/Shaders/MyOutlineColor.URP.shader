// Fixed version of OutlineColor.URP
Shader "Hidden/MyOutlineColorFixed.URP"
{
    HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        half _Cutoff;

        struct MyAttributes
        {
            float4 positionOS : POSITION;
            float2 uv         : TEXCOORD0;
        };

        struct MyVaryings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv          : TEXCOORD0;
        };

        MyVaryings MyVert(MyAttributes input)
        {
            MyVaryings o;
            o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
            o.uv = input.uv;
            return o;
        }

        half4 MyFragmentSimple(MyVaryings input) : SV_Target
        {
            return 1;
        }

        half4 MyFragmentAlphaTest(MyVaryings input) : SV_Target
        {
            half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            clip(c.a - _Cutoff);
            return 1;
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Lighting Off

        Pass
        {
            Name "Opaque"

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex MyVert
            #pragma fragment MyFragmentSimple
            ENDHLSL
        }

        Pass
        {
            Name "Transparent"

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex MyVert
            #pragma fragment MyFragmentAlphaTest
            ENDHLSL
        }
    }
}
