Shader "Cardboard XR/Reticle" {
    Properties{
      _Color("Color", Color) = (1, 1, 1, 1)
      _InnerRing("Inner Ring", Range(0, 10.0)) = 1.5
      _OuterRing("Outer Ring", Range(0.00872665, 10.0)) = 2.0
      _Distance("Distance", Range(0.0, 100.0)) = 2.0
    }

    SubShader{
        Tags {
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstAlpha One
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

            uniform float4 _Color;
            uniform float _InnerRing;
            uniform float _OuterRing;
            uniform float _Distance;

            struct appData {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appData v) {
                float scale = lerp(_OuterRing, _InnerRing, v.vertex.z);
                float3 vert = float3(v.vertex.x * scale, v.vertex.y * scale, _Distance);

                v2f o;
                o.vertex = UnityObjectToClipPos(vert);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 ret = _Color;
                return ret;
            }
        ENDCG
        }
    }
}