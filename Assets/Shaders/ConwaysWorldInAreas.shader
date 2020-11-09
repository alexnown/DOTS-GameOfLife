Shader "Unlit/ConwaysWorldInAreas"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_BackgroundColor("BackgroundColor", Color) = (0,0,0,0)
		_AliveColor("AliveColor", Color) = (0,1,0,0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

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

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _Width;
				float _Height;
				fixed4 _AliveColor;
				fixed4 _BackgroundColor;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float2 size = float2(_Width, _Height);
					float2 pos = i.uv * size;
					float2 cellPos = frac(pos);
					float2 centeredUv = floor(pos) / size + float2(0.5 / _Width, 0.5 / _Height);
					fixed4 col = tex2D(_MainTex, centeredUv);
					if (cellPos.x < 0.25) {
						if (cellPos.y < 0.33) {
							if (frac(col.g * 32) >= 0.5) return _AliveColor;
						}
						else if (cellPos.y < 0.66) {
							if (frac(col.b * 128) >= 0.5) return _AliveColor;
						}
						else {
							if (frac(col.b * 2) >= 0.5) return _AliveColor;
						}
					}
					else if (cellPos.x < 0.5) {
						if (cellPos.y < 0.33) {
							if (frac(col.g * 64) >= 0.5) return _AliveColor;
						}
						else if (cellPos.y < 0.66) {
							if (frac(col.g) >= 0.5) return _AliveColor;
						}
						else {
							if (frac(col.b * 4) >= 0.5) return _AliveColor;
						}
					}
					else if (cellPos.x < 0.75) {
						if (cellPos.y < 0.33) {
							if (frac(col.g * 128) >= 0.5) return _AliveColor;
						}
						else if (cellPos.y < 0.66) {
							if (frac(col.g * 2) >= 0.5) return _AliveColor;
						}
						else {
							if (frac(col.b * 8) >= 0.5) return _AliveColor;
						}
					}
					else {
						if (cellPos.y < 0.33) {
							if (frac(col.r) >= 0.5) return _AliveColor;
						}
						else if (cellPos.y < 0.66) {
							if (frac(col.g * 4) >= 0.5) return _AliveColor;
						}
						else {
							if (frac(col.b * 16) >= 0.5) return _AliveColor;
						}
					}
					return _BackgroundColor;
				}
				ENDCG
			}
		}
}
