Shader "Custom/OceanShader" {
	Properties{
		_MainTex("-" , 2D) = "black" {}
	}

		SubShader{

			Pass {
				CGPROGRAM
				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
		StructuredBuffer<float2> d_ht;
		uint N;
		float halfN;
		float dx;
		float dz;

		float Get_d_ht_real(uint x,uint z)
		{
			float2 h = d_ht[x % 256 + z % 256 * 256];
			return h.x;
		}

		struct VSOut {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		VSOut vert(uint id : SV_VertexID)
		{
			VSOut output;
			uint sqx = ((id + 1) % 4 / 2);
			uint sqz = (1 - id % 4 / 2);
			sqx += (id / 4) % 256;
			sqz += (id / 4) / 256;
			output.pos.y = Get_d_ht_real(sqx, sqz);
			output.pos.xz = float2((sqx - halfN) * dx, (sqz - halfN) * dz);
			output.pos.w = 1;
			output.pos = mul(UNITY_MATRIX_VP, output.pos);
			float rN1 = 1.0 / N;
			output.uv = float2(sqx, sqz) * rN1 + float2(0.5, 0.5) * rN1;
			return output;
		}

		fixed4 frag(VSOut i) : COLOR
		{
			fixed4 col;
			col = tex2D(_MainTex, i.uv);
			col.w = 1;
			col.xyz = normalize(col.xyz);
			return col;
		}
		ENDCG
	 }
	}
}