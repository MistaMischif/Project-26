Shader "LivingTable/CharacterCard"
{
 Properties { _MainTex("Artwork",2D)="white"{} }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
 Pass { Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
 float4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);clip(c.a-.3);return c;}
 ENDHLSL
 } }
}
