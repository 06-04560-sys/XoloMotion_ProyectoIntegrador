// Vegetacion 2D (billboard) iluminada para URP Forward+.
//
// Reemplaza a Sprite-Unlit-Default en los arboles y plantas de Nivel1:
// - Normal map generado a partir del color del sprite (Assets/Sprites/Vegetación 2D/Normales).
//   El espacio tangente es el del quad: X = derecha del objeto, Y = arriba, Z = hacia la camara (-Z local).
// - Luz del sol con sombras, luz envolvente (wrap) y translucidez cuando el sol esta detras de la hoja.
// - Luz indirecta de las light probes (GI horneada) y SSAO.
// - Viento: balanceo suave proporcional a la altura sobre el pivote.
// - Sombra estable: en el pase de sombras el quad se orienta hacia el sol (no hacia la camara),
//   asi la sombra no gira cuando el jugador camina.
// - Alpha to coverage con MSAA para bordes suaves sin ordenar transparencias.
Shader "Tsakel/Vegetacion Billboard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [NoScaleOffset][Normal] _NormalMap ("Normal map (del color)", 2D) = "bump" {}
        _NormalStrength ("Fuerza del normal map", Range(0, 2)) = 1
        _Cutoff ("Corte alfa", Range(0.05, 0.95)) = 0.5
        _Tint ("Tinte", Color) = (1, 1, 1, 1)
        _Wrap ("Luz envolvente", Range(0, 1)) = 0.45
        _Translucency ("Translucidez", Range(0, 2)) = 0.6
        _TranslucencyColor ("Color de translucidez", Color) = (0.85, 1, 0.45, 1)
        _AmbientStrength ("Luz indirecta", Range(0, 3)) = 1.2
        _WindStrength ("Viento (m por m de altura)", Range(0, 0.1)) = 0.012
        _WindSpeed ("Velocidad del viento", Range(0, 5)) = 1.1
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);

        CBUFFER_START(UnityPerMaterial)
            half _NormalStrength;
            half _Cutoff;
            half4 _Tint;
            half _Wrap;
            half _Translucency;
            half4 _TranslucencyColor;
            half _AmbientStrength;
            float _WindStrength;
            float _WindSpeed;
        CBUFFER_END

        float3 ObjectPivotWS() { return TransformObjectToWorld(float3(0, 0, 0)); }
        float ObjectScaleX() { return length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)); }
        float ObjectScaleY() { return length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)); }

        // Desplazamiento de viento en mundo, a lo largo de 'rightWS'.
        float3 Wind(float3 pivotWS, float heightWS, float3 rightWS)
        {
            float h = max(heightWS, 0.0);
            float t = _Time.y * _WindSpeed + dot(pivotWS.xz, float2(0.37, 0.71));
            float wave = sin(t) * 0.7 + sin(t * 2.3 + 1.7) * 0.3;
            return rightWS * wave * _WindStrength * pow(h, 1.3);
        }
        ENDHLSL

        // ------------------------------------------------------------------ color
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float3 positionWS : TEXCOORD1;
                half3 rightWS : TEXCOORD2;
                half3 upWS : TEXCOORD3;
                half3 frontWS : TEXCOORD4;
                half fogFactor : TEXCOORD5;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 rightWS = normalize(TransformObjectToWorldDir(float3(1, 0, 0)));
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 pivot = ObjectPivotWS();
                posWS += Wind(pivot, posWS.y - pivot.y, rightWS);

                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                o.uv = v.uv;
                o.color = v.color * _Tint;
                o.rightWS = rightWS;
                o.upWS = normalize(TransformObjectToWorldDir(float3(0, 1, 0)));
                o.frontWS = normalize(TransformObjectToWorldDir(float3(0, 0, -1)));
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Varyings i, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                // alpha to coverage: borde nitido de ~1 px, sin ordenar transparencias
                half a = (albedo.a - _Cutoff) / max(fwidth(albedo.a), 1e-4) + 0.5;
                clip(albedo.a - _Cutoff * 0.5);

                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalStrength);
                half3 front = frontFace ? i.frontWS : -i.frontWS;
                half3 n = normalize(nTS.x * i.rightWS + nTS.y * i.upWS + nTS.z * front);

                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light sun = GetMainLight(shadowCoord);
                half shadow = sun.shadowAttenuation * sun.distanceAttenuation;

                half ndl = dot(n, sun.direction);
                half diffuse = saturate((ndl + _Wrap) / (1.0 + _Wrap));

                // translucidez: el sol detras de la hoja, visto a traves de ella
                half3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                half back = pow(saturate(dot(v, -sun.direction)), 3.0) * saturate(dot(-front, sun.direction) + 0.3);
                half3 trans = back * _Translucency * _TranslucencyColor.rgb;

                half3 direct = sun.color * shadow * (diffuse + trans);
                half3 ambient = SampleSH(n) * _AmbientStrength;

                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(i.positionCS));
                    ambient *= ao.indirectAmbientOcclusion;
                    direct *= ao.directAmbientOcclusion;
                #endif

                half3 color = albedo.rgb * (direct + ambient);
                color = MixFog(color, i.fogFactor);
                return half4(color, saturate(a));
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------ sombras (orientadas al sol)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half alpha : TEXCOORD1; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 pivot = ObjectPivotWS();
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 toLight = _LightPosition - pivot;
                #else
                    float3 toLight = _LightDirection;
                #endif
                float2 lh = toLight.xz;
                lh = dot(lh, lh) > 1e-6 ? normalize(lh) : float2(0, 1);
                float3 faceN = float3(lh.x, 0, lh.y);           // el quad mira hacia el sol
                float3 rightWS = float3(-lh.y, 0, lh.x);
                float3 posWS = pivot + rightWS * (v.positionOS.x * ObjectScaleX()) + float3(0, 1, 0) * (v.positionOS.y * ObjectScaleY());
                posWS += Wind(pivot, posWS.y - pivot.y, rightWS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - posWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                float4 cs = TransformWorldToHClip(ApplyShadowBias(posWS, faceN, lightDir));
                #if UNITY_REVERSED_Z
                    cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = cs;
                o.uv = v.uv;
                o.alpha = v.color.a * _Tint.a;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a * i.alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------ profundidad y normales (SSAO)
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half alpha : TEXCOORD1; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 rightWS = normalize(TransformObjectToWorldDir(float3(1, 0, 0)));
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 pivot = ObjectPivotWS();
                posWS += Wind(pivot, posWS.y - pivot.y, rightWS);
                o.positionCS = TransformWorldToHClip(posWS);
                o.uv = v.uv;
                o.alpha = v.color.a * _Tint.a;
                return o;
            }

            half Frag(Varyings i) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a * i.alpha - _Cutoff);
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half alpha : TEXCOORD1;
                half3 rightWS : TEXCOORD2; half3 upWS : TEXCOORD3; half3 frontWS : TEXCOORD4;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 rightWS = normalize(TransformObjectToWorldDir(float3(1, 0, 0)));
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 pivot = ObjectPivotWS();
                posWS += Wind(pivot, posWS.y - pivot.y, rightWS);
                o.positionCS = TransformWorldToHClip(posWS);
                o.uv = v.uv;
                o.alpha = v.color.a * _Tint.a;
                o.rightWS = rightWS;
                o.upWS = normalize(TransformObjectToWorldDir(float3(0, 1, 0)));
                o.frontWS = normalize(TransformObjectToWorldDir(float3(0, 0, -1)));
                return o;
            }

            half4 Frag(Varyings i, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a * i.alpha - _Cutoff);
                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv), _NormalStrength);
                half3 front = frontFace ? i.frontWS : -i.frontWS;
                float3 n = normalize(nTS.x * i.rightWS + nTS.y * i.upWS + nTS.z * front);
                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 oct = PackNormalOctQuadEncode(n);
                    return half4(PackFloat2To888(saturate(oct * 0.5 + 0.5)), 0.0);
                #else
                    return half4(n, 0.0);
                #endif
            }
            ENDHLSL
        }
    }
    FallBack Off
}
