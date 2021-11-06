Shader "Custom/Terrain"
{
	Properties
	{
		_MainTexture("Texture", 2D) = "white"{}
		_AmbientColor("AmbientColor", Color) = (0,0,0,1)
		_LightColor("LightColor", Color) = (1,1,1,1)



	}
		SubShader
		{
			Tags { "RenderType" = "Opaque"}
			Pass
			{
				//Cull Off

				CGPROGRAM


				#pragma vertex vertexFunc
				#pragma fragment fragmentFunc


				#include "UnityCG.cginc"


				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv0 : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
					float3 tangent : TANGENT;

					float3 normal : NORMAL;
				};

				struct v2f
				{
					float4 position : SV_POSITION;
					float2 uv0 : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
					float3 tangent : TANGENT;
					float3 normal : NORMAL;
				};

				fixed4 _AmbientColor;
				fixed4 _LightColor;




				sampler2D _MainTexture;


				v2f vertexFunc(appdata IN)
				{
					v2f OUT;
					OUT.position = UnityObjectToClipPos(IN.vertex);
					OUT.uv0 = IN.uv0;
					OUT.uv1 = IN.uv1;
					OUT.tangent = IN.tangent;
					OUT.normal = IN.normal;

					return OUT;
				}
				float RoundT(float x)
				{
					return round(x * 5) / 5;
				}

				fixed4 fragmentFunc(v2f IN) : SV_Target
				{

					
					float3 lightDir = _WorldSpaceLightPos0;


					 float3 normal = IN.normal;
					 float2 uv = IN.uv0;

					 normal = float3(RoundT(normal.x), RoundT(normal.y), RoundT(normal.z));

					 //diffiuse light
					 float3 lightColor = _LightColor;
					 float lightFallof = max(0, dot(normal, lightDir));

					 float3 directDiffiuseLight = (lightColor * lightFallof);


					 //ambient light
					 float3 ambientLight = _AmbientColor;


					 //texture
					 float3 color = tex2D(_MainTexture, IN.uv0);

					 //compositign

					 float3 diffiuseLight = ambientLight + directDiffiuseLight;

					 float3 final = diffiuseLight * color.rgb;



					  fixed4 col = fixed4( final, 1);
					 return col;
				 }
				 ENDCG

			 }

		}

		FallBack "Diffuse"
}