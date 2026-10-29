Shader "LivingTable/CharacterCard"
{
 Properties { [MainTexture] _MainTex("Artwork",2D)="white"{} _Dissolve("Dissolve",Range(0,1))=0 _SparkColor("Spark color",Color)=(1,1,1,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
 Pass { Cull Off
 HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 CBUFFER_START(UnityPerMaterial)
 float _Dissolve;float4 _SparkColor;
 CBUFFER_END
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
 float4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);clip(c.a-.3);if(_Dissolve<=0)return c;float n=frac(52.9829189*frac(dot(floor(i.uv*160),float2(.06711056,.00583715))));clip(n-_Dissolve*1.05+.001);if(_Dissolve>0)c.rgb=lerp(c.rgb,_SparkColor.rgb*2,1-smoothstep(0,.08,n-_Dissolve*1.05));return c;}
 ENDHLSL
 } }
}
