// [23/10] Ma nữ LỘC — xem Editor/LocMaNu.cs. // LOC/MaNuHon - shader "hon ma" cho URP (Unity 6): trong suot, vien sang, chan tan thanh khoi.
// Dung cho tat ca material cua ma nu (da, vay, tay ao, toc). Toc: bat _UseAlphaTex.
Shader "LOC/MaNuHon"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Mau nen", Color) = (1,1,1,1)
        _Alpha ("Do dac (nhin thang)", Range(0,1)) = 0.45
        [Toggle] _UseAlphaTex ("Dung alpha cua texture (toc)", Float) = 0
        _Cutoff ("Cat alpha (toc)", Range(0,1)) = 0.05

        [Toggle] _BloodOpaque ("Mau/vet thuong luon dac (mat)", Float) = 0
        _Contrast ("Do tuong phan (mat: lam noi mat, mau, hoc mat)", Range(0.5, 2.5)) = 1
        _RedBoost ("Tang do cho mau", Range(0, 2)) = 0

        [Toggle] _BackDark ("Sau gay toi + dac (dau)", Float) = 0
        _BackZ0 ("Sau gay: bat dau (z, m)", Float) = 0.06
        _BackZ1 ("Sau gay: toi han (z, m)", Float) = 0.03

        [Toggle] _HideLegs ("An chan trong vay (than)", Float) = 0
        _LegX ("An chan: |x| nho hon (m)", Float) = 0.17
        _LegY ("An chan: cao duoi (m)", Float) = 0.95

        [Header(Vien sang)]
        _RimColor ("Mau vien", Color) = (0.45,0.75,0.95,1)
        _RimStrength ("Do sang vien", Range(0,4)) = 0.75
        _RimPower ("Do gat vien", Range(0.5,8)) = 2.2

        [Header(Tan thanh khoi o chan)]
        _FadeStart ("Cao do bat dau hien (m)", Float) = 0.15
        _FadeEnd ("Cao do dac hoan toan (m)", Float) = 0.95
        _NoiseScale ("Do min nhieu", Float) = 7
        _NoiseAmp ("Do rach mep", Range(0,0.6)) = 0.22

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull (dau/than: Back, vai/toc: Off)", Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Ghi chieu sau (than/mat: On, toc: Off)", Float) = 1

        [Header(Chap chon)]
        _FlickerSpeed ("Toc do chap chon", Float) = 6
        _FlickerAmount ("Muc chap chon", Range(0,0.5)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Hon"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha, _UseAlphaTex, _Cutoff, _BloodOpaque, _BackDark, _Contrast, _RedBoost;
                float _BackZ0, _BackZ1;
                half _HideLegs; float _LegX, _LegY;
                half4 _RimColor;
                half _RimStrength, _RimPower;
                float _FadeStart, _FadeEnd, _NoiseScale, _NoiseAmp;
                float _FlickerSpeed, _FlickerAmount;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; float3 nrmOS : NORMAL; float2 uv : TEXCOORD0; float2 rest : TEXCOORD1; };   // rest = (cao, truoc/sau) luc dung thang — dung cho moi tu the
            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float3 nrmWS : TEXCOORD2;
                float heightOS : TEXCOORD3;
                float3 posOS : TEXCOORD4;
            };

            float hash31(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1); p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            float vnoise(float3 x)
            {
                float3 i = floor(x), f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(hash31(i), hash31(i + float3(1,0,0)), f.x),
                                 lerp(hash31(i + float3(0,1,0)), hash31(i + float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash31(i + float3(0,0,1)), hash31(i + float3(1,0,1)), f.x),
                                 lerp(hash31(i + float3(0,1,1)), hash31(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.posOS.xyz);
                o.posCS = p.positionCS;
                o.posWS = p.positionWS;
                o.nrmWS = TransformObjectToWorldNormal(v.nrmOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.posOS = float3(v.posOS.x, v.rest.x, v.rest.y);
                // cao do tinh tu goc (chan) cua doi tuong, theo don vi the gioi
                // do cao luc dung thang (UV2.x) -> tan khoi/gau vay dung ca khi ma gap nguoi, bo tran
                o.heightOS = v.rest.x;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // chan/than duoi nam trong vay: bo han (vay trang dac che, gau vay tan khoi thi cung khong lo chan)
                if (_HideLegs > 0.5 && abs(i.posOS.x) < _LegX && i.posOS.y < _LegY) clip(-1);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                // lam noi khuon mat: tang tuong phan (da sang hon, hoc mat/mieng toi hon) + mau do dam hon
                tex.rgb = saturate((tex.rgb - 0.45) * _Contrast + 0.45);
                half doMau = saturate((tex.r - 0.5 * (tex.g + tex.b)) * 3.0);
                tex.rgb = lerp(tex.rgb, half3(tex.r * 1.15, tex.g * 0.35, tex.b * 0.35), doMau * saturate(_RedBoost));
                tex *= _BaseColor;
                float3 V = normalize(GetWorldSpaceViewDir(i.posWS));
                float3 N = normalize(i.nrmWS);
                // mat sau cua tam: lat phap tuyen de vien van dung
                if (dot(N, V) < 0) N = -N;
                half rim = pow(saturate(1.0 - dot(N, V)), _RimPower);

                float t = _Time.y;
                float n = vnoise(float3(i.posWS.xz * _NoiseScale, i.posWS.y * _NoiseScale * 0.6 - t * 0.35));
                float h = i.heightOS + (n - 0.5) * 2.0 * _NoiseAmp;
                half fade = saturate((h - _FadeStart) / max(_FadeEnd - _FadeStart, 0.001));

                half flicker = 1.0 + (vnoise(float3(t * _FlickerSpeed, 3.7, 1.3)) - 0.5) * 2.0 * _FlickerAmount;

                half a = (_Alpha + rim * 0.4);
                if (_BloodOpaque > 0.5)
                {
                    // vung do (mau, vet thuong) dac len, khong bi mo theo hon
                    half blood = saturate((tex.r - 0.5 * (tex.g + tex.b)) * 3.5);
                    a = max(a, blood);
                }
                a *= fade * flicker;
                if (_UseAlphaTex > 0.5)
                {
                    clip(tex.a - _Cutoff);
                    a *= tex.a;
                }
                a = saturate(a);
                half rimMul = (_UseAlphaTex > 0.5) ? 0.12 : 1.0;   // toc giu mau den
                half3 col = tex.rgb + _RimColor.rgb * rim * _RimStrength * rimMul;
                if (_BackDark > 0.5)
                {
                    // sau gay (mat truoc cua dau la +Z): toi nhu da dau duoi toc va gan dac -> khong nhin xuyen thay mat/mat do tu phia sau
                    half k = smoothstep(_BackZ0, _BackZ1, i.posOS.z);
                    col = lerp(col, half3(0.012, 0.012, 0.014), k);
                    a = lerp(a, 0.97 * fade, k);
                }
                return half4(col, a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
