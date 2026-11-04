Shader "LivingTable/ChessAlignmentGhost" {
 Properties { _Opacity("Opacity",Range(0,1))=1  [MainColor] _BaseColor("Color",Color)=(0.2,0.8,1,0.25) }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;half _Opacity;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.w=TransformObjectToWorld(i.p.xyz);o.n=TransformObjectToWorldNormal(i.n);return o;}
 half4 frag(V i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float edge=pow(1-saturate(abs(dot(normalize(i.n),normalize(GetWorldSpaceViewDir(i.w))))),2);return half4(_BaseColor.rgb,saturate(_Opacity*(1+edge*.5)));}
 ENDHLSL
 } }
}
