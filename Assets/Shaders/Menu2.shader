Shader "Unlit/Menu2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeSince ("TimeSince", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TimeSince; // Declare _TimeSince here

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, float2(0.5, 0.5));

            // Use distance to calculate displacement
            float displacement = .5+sin(dist * (_TimeSince%3)*50)/2;

            // Use displacement to calculate color
            float r = 0.0;
            float g = 0.0;
            float b = floor(1.1 - displacement);

            return fixed4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
}
