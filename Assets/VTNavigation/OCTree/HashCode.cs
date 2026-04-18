using System;
using UnityEngine;
using VTCodeSpace;

namespace VTNavigation.Tree
{
    public struct HashCode:IEquatable<HashCode>
    {
        private UInt32 m_VTCode;

        public UInt32 Code
        {
            get
            {
                return m_VTCode;
            }
        }

        public UInt32 X
        {
            get
            {
                return VTCode.DecodeX(m_VTCode);
            }
        }

        public UInt32 Y
        {
            get
            {
                return VTCode.DecodeY(m_VTCode);
            }
        }

        public UInt32 Z
        {
            get
            {
                return VTCode.DecodeZ(m_VTCode);
            }
        }

		public int Layer
		{
			get
			{
				return VTCode.DecodeLayer(m_VTCode);
			}
		}

        public bool IsValide
        {
            get
            {
                return m_VTCode != VTCode.INVALID_CODE;
            }
        }

        public HashCode(UInt32 code)
        {
            m_VTCode = code;
        }

		public static int MaxLayer
		{
			get
			{
				return VTCode.MAX_LAYER;
			}
		}

		public static float MaxSize
		{
			get
			{
				return VTCode.POW_MAPPING[VTCode.MAX_LAYER];
			}
		}
        
        //************
        //  获得子节点编码
        //  输入参数为三个维度的偏移，0表示左，下，后的方向，1表示右，上，前方向
        //************
        public HashCode ToChild(UInt32 xoffset,UInt32 yoffset,UInt32 zoffset)
        {
            return new HashCode(VTCode.EncodeToLower(m_VTCode, xoffset, yoffset, zoffset));
        }
        
        //************
        //  获得子节点编码
        //  输入参数为0-7的 3 bits 整数。offset顺序是 y z x
        //************
        public HashCode ToChild(int childOffset)
        {
            UInt32 xoffset = (UInt32)(childOffset & 1);
            UInt32 zoffset = (UInt32)((childOffset >> 1) & 1);
            UInt32 yoffset = (UInt32)((childOffset >> 2) & 1);
            return ToChild(xoffset, yoffset, zoffset);
        }
        

        //************
        //  获得父节点编码
        //************
        public HashCode ToParent()
        {
            return new HashCode(VTCode.EncodeToUpper(m_VTCode));
        }
        
        //************
        //  解码该编码，获得x,y,z和layer
        //************
        public void Decode(out  UInt32 x, out UInt32 y, out UInt32 z, out int layer)
        {
            (x,y,z,layer) = VTCode.Decode(m_VTCode);
        }
        
        //************
        //  将x,y,z编码
        //************
        public static HashCode Encode(UInt32 x, UInt32 y, UInt32 z, int layer)
        {
            return new HashCode(VTCode.Encode(x, y, z, layer));
        }

		//************
		//  获取根节点编码
		//************
		public static HashCode RootNode()
		{
			return new HashCode(VTCode.Encode(0, 0, 0, VTCode.MAX_LAYER));
		}
        
        //************
        //  编码转AABB包围盒，后续会把它移出去
        //************
        public Bounds DecodeBounds()
        {
            Decode(out UInt32 x, out UInt32 y, out UInt32 z, out int layer);

            float sizeInLayer = VTCode.POW_MAPPING[Layer];
            Vector3 min = new Vector3(x* sizeInLayer, y* sizeInLayer, z* sizeInLayer);
            Vector3 max = min + Vector3.one*sizeInLayer;
            Vector3 center = (min + max) * 0.5f;
            Vector3 size = new Vector3(sizeInLayer, sizeInLayer, sizeInLayer);
            return new Bounds(center, size);
        }

		//************
		//  判断当前节点是否是指定节点的孩子
		//************
		public bool IsChildOf(HashCode parentCode)
        {
            return ToParent().Equals(parentCode);
        }

		//************
		//  解码当前HashCode相对于父节点的偏移
		//************
		public void DecodeOffset(out UInt32 xoffset, out UInt32 yoffset, out UInt32 zoffset)
        {
            xoffset = X & 1;
            yoffset = Y & 1;
            zoffset = Z & 1;
        }

		public int DecodeMergedOffset()
		{
			DecodeOffset(out UInt32 xoffset, out UInt32 yoffset, out UInt32 zoffset);
			return (int)((yoffset << 2) | (zoffset << 1) | (xoffset));
		}

		//************
		//  相等判断接口
		//************
		public bool Equals(HashCode other)
		{
			return other.m_VTCode == m_VTCode;
		}
		
		public HashCode ToLeft()
		{
			(bool overflow, UInt32 code) = VTCode.EncodeToNegativeAdjX(m_VTCode);
			return !overflow ? new HashCode(code) : INVALID_CODE;
		}

		public HashCode ToRight()
		{
			(bool overflow, UInt32 code) = VTCode.EncodeToPositiveAdjX(m_VTCode);
			return !overflow ? new HashCode(code) : INVALID_CODE;
		}

		public HashCode ToBottom()
		{
			(bool overflow,  UInt32 code) = VTCode.EncodeToNegativeAdjY(m_VTCode);
			return !overflow? new HashCode(code) : INVALID_CODE;
		}

		public HashCode ToTop()
		{
			(bool overflow, UInt32 code) = VTCode.EncodeToPositiveAdjY(m_VTCode);
			return !overflow? new HashCode(code) : INVALID_CODE;
		}

		public HashCode ToFront()
		{
			(bool overflow, UInt32 code) = VTCode.EncodeToPositiveAdjZ(m_VTCode);
			return !overflow? new HashCode(code) : INVALID_CODE;
		}
		
		public HashCode ToBack()
		{
			(bool overflow, UInt32 code) = VTCode.EncodeToNegativeAdjZ(m_VTCode);
			return !overflow? new HashCode(code) : INVALID_CODE;
		}

		public static HashCode INVALID_CODE = new HashCode(VTCode.INVALID_CODE);
		
		public static HashCode GetHashCodeWithPoint(Vector3 point, int layer = 0)
		{
			UInt32 size = VTCode.POW_MAPPING[layer];
			UInt32 x = (UInt32)Mathf.FloorToInt(point.x/size);
			UInt32 y = (UInt32)Mathf.FloorToInt(point.y/size);
			UInt32 z = (UInt32)Mathf.FloorToInt(point.z/size);
			return HashCode.Encode(x, y, z, layer);
		}
		
		public static HashCode PositionToHashCode(Vector3 position, int layer = 0)
		{
			float layerSize = VTCode.POW_MAPPING[layer];
			UInt32 x = (UInt32)(position.x / layerSize);
			UInt32 y = (UInt32)(position.y / layerSize);
			UInt32 z = (UInt32)(position.z / layerSize);
			return new HashCode(VTCode.Encode(x,y,z,layer));
		}

		public static float LayerToSize(int layer)
		{
			if (layer < 0) layer = 0;
			if (layer > VTCode.MAX_LAYER) return Mathf.Pow(2, layer);
			return VTCode.POW_MAPPING[layer];
		}

		public static int LayerCoordinateCount(int layer)
		{
			if (layer < 0) layer = 0;
			return (int)Mathf.Pow(2, VTCode.MAX_LAYER - layer);
		}
	}
}

