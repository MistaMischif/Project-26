Shader "LivingTable/DepthText"
{
 Properties { _MainTex("Font",2D)="white"{} }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Cull Off ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
 float4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;return float4(i.color.rgb,i.color.a*a);}
 ENDHLSL
 } }
}
