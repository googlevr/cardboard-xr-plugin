Shader "Cardboard XR/Reticle Vertex Color" {
    Properties{
      _InnerRing("Inner Ring", float) = 1
      _OuterRing("Outer Ring", float) = 2.0
      _Distance("Distance", float) = 5.0
    }

    SubShader{
        Tags {
            "Queue" = "Overlay+1"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            AlphaTest Off
            Cull Back
            Lighting Off
            ZWrite Off
            ZTest Always

            Fog { Mode Off }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float _InnerRing;
            uniform float _OuterRing;
            uniform float _Distance;

            struct appData {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appData v) {
                float scale = lerp(_OuterRing, _InnerRing, v.vertex.z);
                float3 vert = float3(v.vertex.x * scale, v.vertex.y * scale, _Distance);

                v2f o;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(vert);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 ret = i.color;
                return ret;
            }
        ENDCG
        }
    }
}