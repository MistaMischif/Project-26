Shader "LivingTable/CandyRewardItem"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass
  {
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 world:TEXCOORD1;half4 c:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
   V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;return o;}
   half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float3 n=normalize(i.n),light=normalize(float3(-.35,.8,-.55));float diffuse=.60+.40*saturate(dot(n,light));float shine=pow(saturate(dot(n,normalize(light+GetWorldSpaceNormalizeViewDir(i.world)))),32)*.27;return half4(i.c.rgb*diffuse+shine,1);}
   ENDHLSL
  }
 }
}
