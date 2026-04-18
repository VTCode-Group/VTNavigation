/***********************************
 * These Code is Auto Generated.
 * Date: 2026-03-27  1:49:28
 ***********************************/
using System;
using System.Collections.Generic;
namespace VTCodeSpace
{
	public static class VTCode
	{
		public static readonly UInt32 INVALID_CODE = UInt32.MaxValue;
		public static readonly int MAX_LAYER = 10;
		public static readonly UInt32 COORDINATE_MAX_VALUE = 0x000003FF;
		public static readonly UInt32 ONE = 1;
		public static readonly UInt32[] X_COORDINATE_MASK = 
		{
			0x000007FE, // 0
			0x000007FC, // 1
			0x000007F8, // 2
			0x000007F0, // 3
			0x000007E0, // 4
			0x000007C0, // 5
			0x00000780, // 6
			0x00000700, // 7
			0x00000600, // 8
			0x00000400, // 9
			0x00000000, // 10
		};

		public static readonly int[] X_COORDINATE_OFFSET = 
		{
			1, // 0
			2, // 1
			3, // 2
			4, // 3
			5, // 4
			6, // 5
			7, // 6
			8, // 7
			9, // 8
			10, // 9
			-1, // 10
		};
		public static readonly UInt32[] Z_COORDINATE_MASK = 
		{
			0x001FF800, // 0
			0x001FF000, // 1
			0x001FE000, // 2
			0x001FC000, // 3
			0x001F8000, // 4
			0x001F0000, // 5
			0x001E0000, // 6
			0x001C0000, // 7
			0x00180000, // 8
			0x00100000, // 9
			0x00000000, // 10
		};

		public static readonly int[] Z_COORDINATE_OFFSET = 
		{
			11, // 0
			12, // 1
			13, // 2
			14, // 3
			15, // 4
			16, // 5
			17, // 6
			18, // 7
			19, // 8
			20, // 9
			-1, // 10
		};
		public static readonly UInt32[] Y_COORDINATE_MASK = 
		{
			0x7FE00000, // 0
			0x7FC00000, // 1
			0x7F800000, // 2
			0x7F000000, // 3
			0x7E000000, // 4
			0x7C000000, // 5
			0x78000000, // 6
			0x70000000, // 7
			0x60000000, // 8
			0x40000000, // 9
			0x00000000, // 10
		};

		public static readonly int[] Y_COORDINATE_OFFSET = 
		{
			21, // 0
			22, // 1
			23, // 2
			24, // 3
			25, // 4
			26, // 5
			27, // 6
			28, // 7
			29, // 8
			30, // 9
			-1, // 10
		};

		private static readonly Dictionary<UInt32, int> LOG_MAPPING = new Dictionary<UInt32, int>()
		{
			{ 1, 0 }, // 2^0 = 1, log2(1) = 0
			{ 2, 1 }, // 2^1 = 2, log2(2) = 1
			{ 4, 2 }, // 2^2 = 4, log2(4) = 2
			{ 8, 3 }, // 2^3 = 8, log2(8) = 3
			{ 16, 4 }, // 2^4 = 16, log2(16) = 4
			{ 32, 5 }, // 2^5 = 32, log2(32) = 5
			{ 64, 6 }, // 2^6 = 64, log2(64) = 6
			{ 128, 7 }, // 2^7 = 128, log2(128) = 7
			{ 256, 8 }, // 2^8 = 256, log2(256) = 8
			{ 512, 9 }, // 2^9 = 512, log2(512) = 9
			{ 1024, 10 }, // 2^10 = 1024, log2(1024) = 10
			{ 2048, 11 }, // 2^11 = 2048, log2(2048) = 11
			{ 4096, 12 }, // 2^12 = 4096, log2(4096) = 12
			{ 8192, 13 }, // 2^13 = 8192, log2(8192) = 13
			{ 16384, 14 }, // 2^14 = 16384, log2(16384) = 14
			{ 32768, 15 }, // 2^15 = 32768, log2(32768) = 15
			{ 65536, 16 }, // 2^16 = 65536, log2(65536) = 16
			{ 131072, 17 }, // 2^17 = 131072, log2(131072) = 17
			{ 262144, 18 }, // 2^18 = 262144, log2(262144) = 18
			{ 524288, 19 } // 2^19 = 524288, log2(524288) = 19
		};
		public static readonly Dictionary<int, UInt32> POW_MAPPING = new Dictionary<int, UInt32>()
		{
			{ 0, 1 }, // pow(2, 0) = 1
			{ 1, 2 }, // pow(2, 1) = 2
			{ 2, 4 }, // pow(2, 2) = 4
			{ 3, 8 }, // pow(2, 3) = 8
			{ 4, 16 }, // pow(2, 4) = 16
			{ 5, 32 }, // pow(2, 5) = 32
			{ 6, 64 }, // pow(2, 6) = 64
			{ 7, 128 }, // pow(2, 7) = 128
			{ 8, 256 }, // pow(2, 8) = 256
			{ 9, 512 }, // pow(2, 9) = 512
			{ 10, 1024 }, // pow(2, 10) = 1024
			{ 11, 2048 }, // pow(2, 11) = 2048
			{ 12, 4096 }, // pow(2, 12) = 4096
			{ 13, 8192 }, // pow(2, 13) = 8192
			{ 14, 16384 }, // pow(2, 14) = 16384
			{ 15, 32768 }, // pow(2, 15) = 32768
			{ 16, 65536 }, // pow(2, 16) = 65536
			{ 17, 131072 }, // pow(2, 17) = 131072
			{ 18, 262144 }, // pow(2, 18) = 262144
			{ 19, 524288 } // pow(2, 19) = 524288
		};
		public static UInt32 Encode(UInt32 x,UInt32 y,UInt32 z,int layer)
		{
			if(layer > MAX_LAYER)
			{
				return INVALID_CODE;
			}
			return ((y << (Y_COORDINATE_OFFSET[layer])) & Y_COORDINATE_MASK[layer])| ((z << (Z_COORDINATE_OFFSET[layer])) & Z_COORDINATE_MASK[layer])| ((x << (X_COORDINATE_OFFSET[layer])) & X_COORDINATE_MASK[layer])| (ONE << layer);
		}
		public static UInt32 EncodeToLower(UInt32 code,UInt32 xoffset,UInt32 yoffset,UInt32 zoffset)
		{
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			if(layer <= 0)
			{
				return INVALID_CODE;
			}
			return Encode((x << 1) | xoffset, (y << 1) | yoffset, (z << 1) | zoffset, layer - 1);
		}
		public static UInt32 EncodeToUpper(UInt32 code)
		{
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			if(layer >= MAX_LAYER)
			{
				return INVALID_CODE;
			}
			return Encode(x >> 1, y >> 1, z >> 1, layer + 1);
		}
		private static UInt32 RightMostBit(UInt32 v)
		{
			return (v & (v - 1)) ^ v;
		}
		public static int DecodeLayer(UInt32 code)
		{
			return LOG_MAPPING[RightMostBit(code)];
		}
		public static UInt32 DecodeX(UInt32 code)
		{
			int layer = DecodeLayer(code);
			if(layer > MAX_LAYER)
			{
				return INVALID_CODE;
			}
			return (code & (X_COORDINATE_MASK[layer])) >> (X_COORDINATE_OFFSET[layer]);
		}
		public static UInt32 DecodeZ(UInt32 code)
		{
			int layer = DecodeLayer(code);
			if(layer > MAX_LAYER)
			{
				return INVALID_CODE;
			}
			return (code & (Z_COORDINATE_MASK[layer])) >> (Z_COORDINATE_OFFSET[layer]);
		}
		public static UInt32 DecodeY(UInt32 code)
		{
			int layer = DecodeLayer(code);
			if(layer > MAX_LAYER)
			{
				return INVALID_CODE;
			}
			return (code & (Y_COORDINATE_MASK[layer])) >> (Y_COORDINATE_OFFSET[layer]);
		}
		public static (UInt32,UInt32,UInt32,int) Decode(UInt32 code)
		{
			int layer = DecodeLayer(code);
			if(layer > MAX_LAYER)
			{
				return (INVALID_CODE, INVALID_CODE, INVALID_CODE, layer);
			}
			return ((code & (X_COORDINATE_MASK[layer])) >> (X_COORDINATE_OFFSET[layer]), (code & (Y_COORDINATE_MASK[layer])) >> (Y_COORDINATE_OFFSET[layer]), (code & (Z_COORDINATE_MASK[layer])) >> (Z_COORDINATE_OFFSET[layer]), layer);
		}
		private static UInt32 CalcNegativeAdjValue(UInt32 v,UInt32 maxValue)
		{
			if(v == 0)
			{
				return maxValue;
			}
			return v - 1;
		}
		private static UInt32 CalcPositiveAdjValue(UInt32 v,UInt32 maxValue)
		{
			if(v == maxValue)
			{
				return 0;
			}
			return v + 1;
		}
		public static (bool,UInt32) EncodeToNegativeAdjX(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = X_COORDINATE_MASK[layer] >> X_COORDINATE_OFFSET[layer];
			UInt32 newX = CalcNegativeAdjValue(x, maxValue);
			if(newX > x)
			{
				overflow = true;
			}
			x = newX;
			return (overflow,Encode(x,y,z,layer));
		}
		public static (bool,UInt32) EncodeToPositiveAdjX(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = X_COORDINATE_MASK[layer] >> X_COORDINATE_OFFSET[layer];
			UInt32 newX = CalcPositiveAdjValue(x, maxValue);
			if(newX < x)
			{
				overflow = true;
			}
			x = newX;
			return (overflow,Encode(x,y,z,layer));
		}
		public static (bool,UInt32) EncodeToNegativeAdjZ(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = Z_COORDINATE_MASK[layer] >> Z_COORDINATE_OFFSET[layer];
			UInt32 newZ = CalcNegativeAdjValue(z, maxValue);
			if(newZ > z)
			{
				overflow = true;
			}
			z = newZ;
			return (overflow,Encode(x,y,z,layer));
		}
		public static (bool,UInt32) EncodeToPositiveAdjZ(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = Z_COORDINATE_MASK[layer] >> Z_COORDINATE_OFFSET[layer];
			UInt32 newZ = CalcPositiveAdjValue(z, maxValue);
			if(newZ < z)
			{
				overflow = true;
			}
			z = newZ;
			return (overflow,Encode(x,y,z,layer));
		}
		public static (bool,UInt32) EncodeToNegativeAdjY(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = Y_COORDINATE_MASK[layer] >> Y_COORDINATE_OFFSET[layer];
			UInt32 newY = CalcNegativeAdjValue(y, maxValue);
			if(newY > y)
			{
				overflow = true;
			}
			y = newY;
			return (overflow,Encode(x,y,z,layer));
		}
		public static (bool,UInt32) EncodeToPositiveAdjY(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 maxValue = Y_COORDINATE_MASK[layer] >> Y_COORDINATE_OFFSET[layer];
			UInt32 newY = CalcPositiveAdjValue(y, maxValue);
			if(newY < y)
			{
				overflow = true;
			}
			y = newY;
			return (overflow,Encode(x,y,z,layer));
		}
	}
}
