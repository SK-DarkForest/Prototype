Shader "Unlit/Starfield"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeSince ("TimeSince", Float) = 0.0
        _Amplitude ("Amplitude",Float) = 0.0
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
            float _TimeSince;
            float _Amplitude;

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
                float2 uv = i.uv;
                uv.y -= ((_TimeSince/5)%24) * 0.0002; // This will create the moving effect
                uv.x -= ((_TimeSince/5)%24) * 0.0002; // This will create the moving effect

                // Create a starfield
                float starDensity = 300.0;
                /*uv.y += _TimeSince/25+sin(uv.y*20)/250;
                uv.x += sin(uv.x*20)/250;*/
                float2 starUV = frac(uv * starDensity);
                float dist = sin(uv.x*200)-cos(uv.y*200);
                float star = (step(length(starUV-.5*sin(uv.x)), 0.03*(_Amplitude*8+.75+sin(((_TimeSince/5)%24))/2+uv.x/4-uv.y/4-tan(uv.y/2+sin(((_TimeSince/5)%24)/20)*20))/10)) * 1.0;
                star = step(length(starUV-.5*sin(uv.x)), _Amplitude/4- .2-sin(uv.x*100)/5-sin(uv.y*50+2)/5-tan(uv.y+sin(((_TimeSince/20)%24)/20)*20)/25);//
                return fixed4(star*sin(uv.x), star*cos(uv.y), star*tan(((_TimeSince/5)%24)), 1.0);
                
            }
            ENDCG
        }
    }
}
//-tan(uv.y+sin(((_TimeSince/5)%24)/20)*20))/5
