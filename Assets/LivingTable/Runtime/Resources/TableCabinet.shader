Shader "LivingTable/TableCabinet"
{
 Properties { [MainColor] _BaseColor("Color",Color)=(1,1,1,1) _Finish("0 burl oak, 1 brushed gold, 2 candy",Float)=0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" } Pass { Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float3 o:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO};
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float _Finish;
 CBUFFER_END
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 V vert(A a){V v;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);v.w=TransformObjectToWorld(a.p.xyz);v.p=TransformWorldToHClip(v.w);v.n=TransformObjectToWorldNormal(a.n);v.o=a.p.xyz;return v;}
 half4 frag(V v):SV_Target{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);
 float3 n=normalize(v.n),view=GetWorldSpaceNormalizeViewDir(v.w),light=normalize(float3(-.35,.8,-.55));
 float diffuse=.65+.35*saturate(dot(n,light)),spec=pow(saturate(dot(n,normalize(light+view))),64);
 float3 c;
 if(_Finish<.5){
 float2 p=v.o.xz*16+v.o.xy*float2(2,9);float warp=noise(p*.7)*2+noise(p*1.9)*.6;
 float grain=.5+.5*sin((length(p+float2(warp,noise(p+4)))*7+warp*8)*4);
 float pores=noise(p*30);float burl=noise(p*1.8+warp);
 c=lerp(float3(.018,.003,.001),float3(.17,.045,.012),saturate(.18+grain*.28+burl*.6));c*=.88+.12*pores;c=c*diffuse+spec*.21;
 }else if(_Finish<1.5){
 float brush=noise(float2(v.o.x*2+v.o.z*3,v.o.y*1100+v.o.z*750));
 float fres=pow(1-saturate(dot(n,view)),3);float reflection=.5+.5*sin(dot(reflect(-view,n),float3(.7,.9,.4))*9);c=lerp(float3(.31,.14,.023),float3(.64,.39,.085),reflection)*diffuse*(.93+.07*brush)+float3(1,.88,.48)*(spec*.8+fres*.18);
 }else{
 float stripe=.5+.5*sin((v.o.x+v.o.y+v.o.z)*30);c=_BaseColor.rgb*diffuse*(.92+.08*stripe)+spec*.55;return half4(c,1);
 }
 return half4(c*_BaseColor.rgb,1);
 }
 ENDHLSL
 } }
}
