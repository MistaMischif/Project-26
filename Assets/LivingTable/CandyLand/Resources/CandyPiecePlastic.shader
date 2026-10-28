Shader "LivingTable/MoldedPlastic"
{
    Properties { [MainColor] _BaseColor("Plastic color", Color) = (0.1,0.6,0.2,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float3 positionWS:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
            Varyings vert(Attributes input)
            {
                Varyings output;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS=TransformWorldToHClip(output.positionWS);
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);return output;
            }
            half4 frag(Varyings input):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 normal=normalize(input.normalWS),light=normalize(float3(-0.35,0.8,-0.55));
                float diffuse=0.52+0.48*saturate(dot(normal,light));
                float3 halfway=normalize(light+GetWorldSpaceNormalizeViewDir(input.positionWS));
                float shine=pow(saturate(dot(normal,halfway)),48)*0.35;
                return half4(_BaseColor.rgb*diffuse+shine,1);
            }
            ENDHLSL
        }
    }
}
