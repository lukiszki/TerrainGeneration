Shader "Custom/Water"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _FoamDistance("Foam Distance", Float) = 0.4
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}

        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0,1)) = 0.27
        _Smoothness("Smothness", Range(0,1)) = 0.5
        _NormalAffect("NormalAffect", float) = 0.5

    }
        SubShader
    {
        Pass
        {
            CGPROGRAM
            #define SMOOTHSTEP_AA 0.01
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;

            float _DepthMaxDistance;

            sampler2D _CameraDepthTexture;

            float _FoamDistance;

            float2 _SurfaceNoiseScroll;

            float _NormalAffect;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;
            float _SurfaceDistortionAmount;

            float _Smoothness;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 noiseUV : TEXCOORD0;
                float4 screenPosition : TEXCOORD2;
                float2 distortUV : TEXCOORD1;
                float3 normal : NORMAL;
                float3 hitPos : TEXCOORD3;
                float3 origin : TEXCOORD4;
            };

            // Above the vertex shader.
            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            float _SurfaceNoiseCutoff;
            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.normal = v.normal;
                o.origin = _WorldSpaceCameraPos;
                o.hitPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float calculateSpecular(float3 normal,float3 viewDir, float3 dirToSun, float smoothness)
            {
                float specularAngle = acos(dot(normalize(dirToSun - viewDir), normal));
                float specularExponent = specularAngle / smoothness;
                float specularHighlight = exp(-specularExponent * specularExponent);
                return specularHighlight;
            }
            float4 frag(v2f i) : SV_Target
            {

                //ALL THAT WATER STUFF
                float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);

                float depthDiffrence = existingDepthLinear - i.screenPosition.w;

                float waterDepthDiffrence01 = (depthDiffrence / _DepthMaxDistance);
                float4 waterColor = saturate(lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDiffrence01));

                float foamDepthDifference01 = saturate(depthDiffrence / _FoamDistance);
                float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x,( i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y)+distortSample.y);

                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

                float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

                //SPECULAR LIGHT

                float smoothness = _Smoothness;

                float3 lightDir = _WorldSpaceLightPos0;

                float3 dir = normalize(i.hitPos - i.origin);


                float specularLight = calculateSpecular(normalize(i.normal+ float3(distortSample.xy,0)*_NormalAffect), dir, lightDir, smoothness);

                float3 watCol = waterColor + surfaceNoise;

                float4 col = normalize(float4(watCol.x + specularLight, watCol.y + specularLight, watCol.z + specularLight, 1));

                    return col;
                return waterColor + surfaceNoise;
            }
            ENDCG
        }
    }
}