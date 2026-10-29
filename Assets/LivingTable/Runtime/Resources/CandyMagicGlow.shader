Shader "LivingTable/CandyMagicGlow"
{
 Properties { [MainColor] _BaseColor("Glow color",Color)=(1,0,0,1) _Strength("Strength",Float)=1 _Square("Square interior",Float)=0 _Saturated("Saturated square",Float)=0 [HideInInspector] _DstBlend("Destination blend",Float)=1 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent"}
 Pass { Blend SrcAlpha [_DstBlend] ZWrite Off ZTest LEqual Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;float _Strength;float _Square;float _Saturated;
 CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
 half4 frag(V i):SV_Target
 {
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
  float2 p=abs(i.uv*2-1);float r=length(p);
  float rays=exp(-p.x*6-p.y*55)+exp(-p.y*6-p.x*55);
  float halo=exp(-r*r*9)*.3,core=exp(-r*r*220);
  float star=saturate(rays+halo+core)*smoothstep(1,.65,r);
  float interior=smoothstep(1,.72,max(p.x,p.y));
  float alpha=lerp(star,interior*.72,_Square)*_Strength;
  return half4(lerp(_BaseColor.rgb*1.4+core*(1-_Square)*.45,_BaseColor.rgb*1.25,_Saturated),saturate(alpha));
 }
 ENDHLSL
 } }
}
