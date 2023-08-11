Shader"ShaderToy/FireShader"
{
    Properties
    {
        _MainTex ("iChannel0", 2D) = "white" {}
        _SecondTex ("iChannel1", 2D) = "white" {}
        _ThirdTex ("iChannel2", 2D) = "white" {}
        _FourthTex ("iChannel3", 2D) = "white" {}
        _Mouse ("Mouse", Vector) = (0.5, 0.5, 0.5, 0.5)
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

            // Built-in properties
sampler2D _MainTex;
float4 _MainTex_TexelSize;
sampler2D _SecondTex;
float4 _SecondTex_TexelSize;
sampler2D _ThirdTex;
float4 _ThirdTex_TexelSize;
sampler2D _FourthTex;
float4 _FourthTex_TexelSize;
float4 _Mouse;
float _GammaCorrect;
float _Resolution;

            // GLSL Compatability macros
#define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
#define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
#define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
#define iResolution float3(_Resolution, _Resolution, _Resolution)
#define iFrame (floor(_Time.y / 60))
#define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
#define iDate float4(2020, 6, 18, 30)
#define iSampleRate (44100)
#define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)

            // Global access to uv data
static v2f vertex_output;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.547);
}

float hermite(float t)
{
    return t * t * (3. - 2. * t);
}

float noise(float2 co, float frequency)
{
    float2 v = float2(co.x * frequency, co.y * frequency);
    float ix1 = floor(v.x);
    float iy1 = floor(v.y);
    float ix2 = floor(v.x + 1.);
    float iy2 = floor(v.y + 1.);
    float fx = hermite(frac(v.x));
    float fy = hermite(frac(v.y));
    float fade1 = lerp(rand(float2(ix1, iy1)), rand(float2(ix2, iy1)), fx);
    float fade2 = lerp(rand(float2(ix1, iy2)), rand(float2(ix2, iy2)), fx);
    return lerp(fade1, fade2, fy);
}

float pnoise(float2 co, float freq, int steps, float persistence)
{
    float value = 0.;
    float ampl = 1.;
    float sum = 0.;
    for (int i = 0; i < steps; i++)
    {
        sum += ampl;
        value += noise(co, freq) * ampl;
        freq *= 2.;
        ampl *= persistence;
    }
    return value / sum;
}

float4 frag(v2f __vertex_output) : SV_Target
{
    vertex_output = __vertex_output;
    float4 fragColor = 0;
    float2 fragCoord = vertex_output.uv * _Resolution;
    float2 uv = fragCoord.xy / iResolution.xy;
    float gradient = 1. - uv.y;
    float gradientStep = 0.2;
    float2 pos = fragCoord.xy / iResolution.x;
    pos.y -= _Time.y * 0.3125;
    float4 brighterColor = float4(1., 0.65, 0.1, 0.25);
    float4 darkerColor = float4(1., 0., 0.15, 0.0625);
    float4 middleColor = lerp(brighterColor, darkerColor, 0.5);
    float noiseTexel = pnoise(pos, 10., 5, 0.5);
    float firstStep = smoothstep(0., noiseTexel, gradient);
    float darkerColorStep = smoothstep(0., noiseTexel, gradient - gradientStep);
    float darkerColorPath = firstStep - darkerColorStep;
    float4 color = lerp(brighterColor, darkerColor, darkerColorPath);
    float middleColorStep = smoothstep(0., noiseTexel, gradient - 0.2 * 2.);
    color = lerp(color, middleColor, darkerColorStep - middleColorStep);
    color = lerp(((float4) 0.), color, firstStep);
    fragColor = color;
    if (_GammaCorrect)
        fragColor.rgb = pow(fragColor.rgb, 2.2);
    return fragColor;
}
            ENDCG
        }
    }
}
