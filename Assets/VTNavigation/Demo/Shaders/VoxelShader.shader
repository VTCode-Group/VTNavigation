Shader "VTPartition/VoxelShader"
{
    Properties
    {
        _MainColor("Main Color", Color)=(1,0,0,0)
		_EdgeColor("Edge Color", Color)=(0,1,0,0)
		_SplitRectWidth("Split Rect Radius", Range(0.3, 0.6))=0.5
		_EdgeWidth("Edge Width", Range(0.1,0.3)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			#pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 positionOS :TEXCOORD0;
            };

            struct InstanceMatries
            {
                float4x4 objectToWorld;
            };

            StructuredBuffer<InstanceMatries> _Matries;

            v2f vert (appdata v, uint instanceID:SV_InstanceID)
            {
                v2f o;
                float4 worldPos = mul(_Matries[instanceID].objectToWorld, v.vertex);
                o.vertex = UnityWorldToClipPos(worldPos);
				o.positionOS = v.vertex;
                return o;
            }

			float4 _MainColor;
			float4 _EdgeColor;
			float _SplitRectWidth;
			float _EdgeWidth;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float3 positionOS = i.positionOS.xyz;

				float3 coordinateCube = abs(positionOS)*2;

				float2 coordinatePlane = float2(0,0);
				
				if(coordinateCube.x >= 0.95f)
				{
					coordinatePlane = float2(coordinateCube.z, coordinateCube.y);
				}else if(coordinateCube.y >= 0.95f)
				{
					coordinatePlane = float2(coordinateCube.x, coordinateCube.z);
				}else if(coordinateCube.z >= 0.95f)
				{
					coordinatePlane = float2(coordinateCube.x, coordinateCube.y);
				}

				float edgeStart = 1.0f - _EdgeWidth;

				if(coordinatePlane.x >= edgeStart || coordinatePlane.y >= edgeStart)
				{
					return float4(_EdgeColor);
				}else 
				{
					float2 innerRectCenter = float2(edgeStart - _SplitRectWidth, edgeStart - _SplitRectWidth);
					if(coordinatePlane.x >= innerRectCenter.x && coordinatePlane.y >= innerRectCenter.y && length(coordinatePlane - innerRectCenter) >= _SplitRectWidth)
					{
						return float4(_EdgeColor);
					}
				}

                return float4(_MainColor);
            }
            ENDHLSL
        }
    }
}
