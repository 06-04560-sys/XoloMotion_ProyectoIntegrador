// Particulas simples para URP (polen y hojas de Nivel1).
// Polen: aditivo (Blend One One, niebla hacia negro). Hojas: recorte alfa con niebla normal
// y una luz minima del sol para que no brillen igual en la sombra que al sol.
Shader "Tsakel/Particula"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
        [Toggle] _ZWrite ("ZWrite", Float) = 0
        _Cutoff ("Corte alfa (0 = sin corte)", Range(0, 1)) = 0
        _Intensity ("Intensidad", Range(0, 8)) = 1
        _SunShading ("Sombreado del sol (0 = sin luz)", Range(0, 1)) = 0
        [Toggle] _FogToBlack ("Niebla a negro (aditivo)", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull Off

        Pass
        {
            Name "Particula"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half _Cutoff;
                half _Intensity;
                half _SunShading;
                half _FogToBlack;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR;
                float3 positionWS : TEXCOORD1; half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                if (_Cutoff > 0) clip(c.a - _Cutoff);
                c.rgb *= _Intensity;

                if (_SunShading > 0)
                {
                    Light sun = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                    half3 lit = sun.color * (0.55 * sun.shadowAttenuation) + SampleSH(half3(0, 1, 0));
                    c.rgb *= lerp(1, lit, _SunShading);
                }

                if (_FogToBlack > 0) { c.rgb = MixFogColor(c.rgb, half3(0, 0, 0), i.fogFactor); c.rgb *= c.a; }
                else c.rgb = MixFog(c.rgb, i.fogFactor);
                return c;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
