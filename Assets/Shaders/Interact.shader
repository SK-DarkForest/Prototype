Shader "Custom/TransparentCapsule"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _num ("_num", float) = 0.0
        _TimeSince ("TimeSince", Float) = 0.0
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _num;
            float _TimeSince;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            float rectangle(float2 samplePosition, float2 halfSize){
                float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
                float outsideDistance = length(max(componentWiseEdgeDistance, 0));
                float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
                return outsideDistance + insideDistance;
            }
            float sdfCircle(float2 p, float2 center, float radius){
                return length(p-center)-radius;
            }
            float sdCapsule( float2 p, float2 a, float2 b, float r ){
                float2 pa = p - a, ba = b - a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                return length( pa - ba*h ) - r;
            }
            fixed4 frag (v2f i) : SV_Target{
                // sample the texture
                float dist = 5;
                // Fix UV calculation
                float2 uv = i.uv;
                float invert = 0;
                uv.x /= (_ScreenParams.y / _ScreenParams.x);
                for(float j = 0; j < _num; j++){
                    dist = min(dist, sdCapsule(uv, float2(1.1, .08 + j / 7), float2(1.7, .08 + j / 7), .065));//+float2(sin(_TimeSince+uv.y*25)/500,cos(_TimeSince+uv.x*25)/500)
                    invert = max(invert, step(.1,sdfCircle(i.uv,float2(1.5,.08 + j / 7),.4)));
                }
                fixed4 col = fixed4(_num/5, .5*(.5+sin(_TimeSince+uv.x)/2), .5, 1- step(1 - dist, 1));
                col = col*(1-invert)+fixed4(col.g,col.b,col.r,col.a)*invert;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}