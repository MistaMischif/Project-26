Shader "LivingTable/DepthText" {
 Properties { _MainTex("Font",2D)="white"{} }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V v;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);v.p=TransformObjectToHClip(a.p.xyz);v.uv=a.uv;v.c=a.c;return v;}
 half4 frag(V v):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);return half4(v.c.rgb,v.c.a*SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,v.uv).a);}
 ENDHLSL
 } }
}
