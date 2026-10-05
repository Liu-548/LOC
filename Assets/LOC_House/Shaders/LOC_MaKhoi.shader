// [3/10] Thân con ma LỘC: khối đen như khói — lõi đặc, viền tan ra (theo góc nhìn), lấm tấm nhiễu nhảy theo thời gian,
// đỉnh hơi run. Không nhận ánh sáng (luôn đen), trong suốt, hai mặt (Cull Off) nên không lộ khe khi lưới bị lật.
Shader "LOC/MaKhoi"
{
    Properties
    {
        _Color ("Màu", Color) = (0, 0, 0, 1)
        _Alpha ("Độ đặc", Range(0, 1)) = 0.8
        _Vien ("Độ tan viền", Range(0.05, 1)) = 0.55
        _Nhieu ("Nhiễu", Range(0, 1)) = 0.45
        _Rung ("Run đỉnh (m)", Range(0, 0.05)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color; half _Alpha, _Vien, _Nhieu, _Rung;
            CBUFFER_END

            struct A { float4 pos : POSITION; float3 n : NORMAL; };
            struct V { float4 cs : SV_POSITION; float3 nw : TEXCOORD0; float3 pw : TEXCOORD1; };

            float h(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }

            V vert(A i)
            {
                V o;
                float3 pw = TransformObjectToWorld(i.pos.xyz);
                float3 nw = TransformObjectToWorldNormal(i.n);
                float t = _Time.y;
                pw += nw * _Rung * (sin(t * 23.0 + pw.y * 41.0 + pw.x * 17.0) + 0.5 * sin(t * 7.0 + pw.z * 29.0));
                o.pw = pw; o.nw = nw; o.cs = TransformWorldToHClip(pw);
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                float3 v = normalize(GetCameraPositionWS() - i.pw);
                float f = abs(dot(normalize(i.nw), v));                 // 1 = nhìn thẳng mặt, 0 = viền
                float a = smoothstep(0.02, _Vien, f);                  // viền tan
                float n = h(floor(i.pw * 38.0) + floor(_Time.y * 14.0));
                a *= 1.0 - _Nhieu * n;
                return half4(_Color.rgb, saturate(a * _Alpha));
            }
            ENDHLSL
        }
    }
}
