Shader "Unlit/Programming"
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

            StructuredBuffer<float> floatArrayBuffer;

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
            float _TimeSince;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float mix(float x, float y, float a){return x*(1-a)+y*a;}
            float smin(float a, float b, float k) {
                float h = clamp(0.5 + 0.5*(a-b)/k, 0.0, 1.0);
                return mix(a, b, h) - k*h*(1.0-h);
            }
            float sdfCircle(float2 p, float2 center, float radius){
                return length(p-center)-radius;
            }
            /*float sdfRectRound(float2 p, float2 dim, float r){
                float2 d = abs(p) - dim + float2(r);
                return length(max(d, 0.0)) - r;
            }*/
            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = (0,0,0,0);
                float r = step(smin(sdfCircle(i.uv,float2(0,0),.5+sin(_TimeSince)/10),sdfCircle(i.uv,float2(1,1),.5+cos(_TimeSince)/5),.1),.2);
                //col = fixed4(0,0,1-r,1);
                //Background
                if(length(float2(col[0],col[1]))==0&&length(float2(col[2],col[3])==0)){
                    if(i.uv.x<.08){
                        col = fixed4(0,0,0,1);
                    }else if(i.uv.x<.2){
                        col = fixed4(.2,.2,.5,1);
                    }else{
                        col = fixed4(1,1,1,1);
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}
