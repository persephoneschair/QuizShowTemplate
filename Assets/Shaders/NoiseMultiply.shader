Shader "ATB/Noise Multiply"
{
    Properties
    {
    	[HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleA ("Noise scale (A)", Vector) = (1.0, 1.0, 1.0)
		_OffsetA ("Noise offset (A)", Vector) = (1.0, 1.0, 1.0)
		_TimeScaleA ("Time scale (A)", Vector) = (1.0, 0.0, 0.0)
		_ColorA("Color (A)", Color) = (1.0, 1.0, 1.0, 1.0)
		_ScaleB("Noise scale (B)", Vector) = (1.0, 1.0, 1.0)
		_OffsetB("Noise offset (B)", Vector) = (1.0, 1.0, 1.0)
		_TimeScaleB("Time scale (B)", Vector) = (1.0, 0.0, 0.0)
		_ColorB("Color (B)", Color) = (1.0, 1.0, 1.0, 1.0)
	}
    SubShader
    {
		Blend SrcAlpha OneMinusSrcAlpha

        // No culling or depth
        Cull Off ZWrite Off ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			struct appdata {			
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 _ScaleA;
			float3 _OffsetA;
			float3 _TimeScaleA;
			fixed4 _ColorA;
			float3 _ScaleB;
			float3 _OffsetB;
			float3 _TimeScaleB;
			fixed4 _ColorB;

            float hash( float n )
			{
			    return frac(sin(n) * 43758.5453);
			}

			float noise( float3 x )
			{
			    // The noise function returns a value in the range -1.0f -> 1.0f

			    float3 p = floor(x);
			    float3 f = frac(x);

			    f       = f*f*(3.0-2.0*f);
			    float n = p.x + p.y*57.0 + 113.0*p.z;

			    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
			                   lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
			               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
			                   lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
			}

            v2f vert (appdata v)
            {
				v2f o;

				o.normal = v.normal;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;

				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 texCol = i.color * tex2D(_MainTex, i.uv);

				float noiseValA = 1.0-noise(_ScaleA * (_OffsetA+float3(i.uv,0.0)) + (_TimeScaleA * _Time.y));
				fixed4 noiseColA = _ColorA * noiseValA;

				float noiseValB = 1.0 - noise(_ScaleB * (_OffsetB + float3(i.uv, 0.0)) + (_TimeScaleB * _Time.y));
				fixed4 noiseColB = _ColorB * noiseValB;

				return texCol * (noiseColA + noiseColB);
            }
            ENDCG
        }
    }
}
