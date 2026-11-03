Shader "LivingTable/GlossButton"
{
 Properties { _Tint("Tint",Color)=(.04,.16,.2,1) _Hover("Hover",Float)=0 _Pressed("Pressed",Float)=0 _SquareButton("Square",Float)=0 _Circle("Circle",Float)=0 _Sparkle("Sparkle",Float)=0 _GoldTrim("Gold trim",Float)=0 _GoldFace("Gold face",Float)=0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
 Pass { Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 CBUFFER_START(UnityPerMaterial)
 float4 _Tint;float _Hover;float _Pressed;float _Circle;float _SquareButton;float _Sparkle;float _GoldTrim;float _GoldFace;
 CBUFFER_END
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
 float4 frag(V i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 p=(i.uv-.5)*float2(lerp(3.8,1,_SquareButton),1);
 float2 q=abs(p)-float2(lerp(1.70,.30,_SquareButton),.30);
 float d=length(max(q,0))+min(max(q.x,q.y),0)-.20;
 if(_Circle>.5)d=length(i.uv-.5)-.5;
 clip(-d);
 float3 c;
 if(d>-.025)c=float3(.06,.07,.07);
 else if(d>-.09)c=lerp(lerp(float3(.32,.34,.35),float3(.94,.96,.97),smoothstep(.1,.85,i.uv.y)),lerp(float3(.29,.15,.025),float3(1,.87,.43),smoothstep(.1,.85,i.uv.y)),_GoldTrim);
 else if(d>-.125)c=float3(.015,.025,.015);
 else {
 c=_Tint.rgb*(.78+.35*i.uv.y);
 float shine=smoothstep(.50,.54,i.uv.y)*(.18+.38*i.uv.y);
 c=lerp(c,float3(.9,1,.9),shine);
 if(_GoldFace>.5){float band=smoothstep(.46,.52,i.uv.y);c=lerp(float3(.28,.12,.018),float3(.72,.46,.12),i.uv.y);c+=band*.14;c+=pow(saturate(1-abs(i.uv.y-.84)*17),5)*float3(.25,.22,.12);}
 c+=_Hover*float3(.18,.25,.16);
 if(_Sparkle>.5){
 for(int k=0;k<10;k++){
 float a=k*2.39996+_Time.y*.3;
 float r=.12+.19*frac(k*.618);
 float2 star=.5+float2(cos(a),sin(a))*r;
 float2 v=abs(i.uv-star);
 float sparkle=pow(saturate(1-min(v.x*100+v.y*22,v.x*22+v.y*100)),3);
 c+=sparkle*(.4+.6*pow(sin(_Time.y*1.7+k)*.5+.5,2));
 }
 c+=.035*(sin(_Time.y*2)*.5+.5);
 }
 c*=lerp(1,.33,_Pressed);
 }
 return float4(c,1);
 }
 ENDHLSL
 } }
}
