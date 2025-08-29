/***********************************
 * These Code is Auto Generated.
 * Date: 2025-08-08  0:10:19
 ***********************************/
using System;
namespace VTCodeSpace
{
	public static class VTCode
	{
		public static readonly UInt32 INVALID_CODE = 0;

		private static UInt32 ONE = 1;

		public static int MAX_LAYER = 10;

		public static int DIMENSION = 3;

		private static UInt32 COORDINATE_MAX_VALUE = 0x000003FF;

		private static UInt32 COORDINATE_CLIP_MASK = 0x000003FF;

		private static UInt32[] ENCODE_MASK =
		{
			0x030000FF,
			0x0300F00F,
			0x030C30C3,
			0x09249249
		};

		private static int[] ENCODE_OFFSET =
		{
			16,
			8,
			4,
			2
		};

		private static UInt32 EncodeCoordinate(UInt32 value)
		{
			value &= COORDINATE_CLIP_MASK;
			value = (value | value << ENCODE_OFFSET[0]) & ENCODE_MASK[0];
			value = (value | value << ENCODE_OFFSET[1]) & ENCODE_MASK[1];
			value = (value | value << ENCODE_OFFSET[2]) & ENCODE_MASK[2];
			value = (value | value << ENCODE_OFFSET[3]) & ENCODE_MASK[3];

			return value;
		}

		private static UInt32 DecodeCoordinate(UInt32 value)
		{
			value &= ENCODE_MASK[3];
			value = (value | value >> ENCODE_OFFSET[3]) & ENCODE_MASK[2];
			value = (value | value >> ENCODE_OFFSET[2]) & ENCODE_MASK[1];
			value = (value | value >> ENCODE_OFFSET[1]) & ENCODE_MASK[0];
			value = (value | value >> ENCODE_OFFSET[0]) & COORDINATE_CLIP_MASK;
			return value;
		}

		public static UInt32 Encode(UInt32 x, UInt32 y, UInt32 z, int layer)
		{
			UInt32 CoordinateEncode = (EncodeCoordinate(y) << 2) | (EncodeCoordinate(z) << 1) | (EncodeCoordinate(x) << 0);
			return (CoordinateEncode << (1 + layer * DIMENSION)) | (ONE << layer);
		}

		public static int DecodeLayer(UInt32 code)
		{
			return (int)Math.Log(RightMostBit(code), 2);
		}

		private static UInt32 RightMostBit(UInt32 v)
		{
			return (v & (v - 1)) ^ v;
		}

		public static (UInt32, UInt32, UInt32, int) Decode(UInt32 code)
		{
			int layer = DecodeLayer(code);
			UInt32 coordinate = code >> (1 + layer * DIMENSION);
			UInt32 x = DecodeCoordinate(coordinate);
			UInt32 z = DecodeCoordinate(coordinate >> 1);
			UInt32 y = DecodeCoordinate(coordinate >> 2);
			return (x, y, z, layer);
		}

		public static UInt32 DecodeX(UInt32 code)
		{
			int layer = DecodeLayer(code);
			UInt32 coordinate = code >> (1 + layer * DIMENSION);
			return DecodeCoordinate(coordinate);
		}

		public static UInt32 DecodeZ(UInt32 code)
		{
			int layer = DecodeLayer(code);
			UInt32 coordinate = code >> (1 + layer * DIMENSION);
			return DecodeCoordinate(coordinate >> 1);
		}

		public static UInt32 DecodeY(UInt32 code)
		{
			int layer = DecodeLayer(code);
			UInt32 coordinate = code >> (1 + layer * DIMENSION);
			return DecodeCoordinate(coordinate >> 2);
		}

		public static UInt32 EncodeToLower(UInt32 code, UInt32 xoffset, UInt32 yoffset, UInt32 zoffset)
		{
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			x = (UInt32)((x << 1) | xoffset);
			z = (UInt32)((z << 1) | zoffset);
			y = (UInt32)((y << 1) | yoffset);
			return Encode(x, y, z, layer - 1);
		}

		public static UInt32 EncodeToUpper(UInt32 code)
		{
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			return Encode(x >> 1, y >> 1, z >> 1, layer + 1);
		}

		private static UInt32 CalcNegativeAdjValue(UInt32 v, UInt32 maxValue)
		{
			if (v == 0)
			{
				return maxValue;
			}
			return v - 1;
		}
		private static UInt32 CalcPositiveAdjValue(UInt32 v, UInt32 maxValue)
		{
			if (v == maxValue)
			{
				return 0;
			}
			return v + 1;
		}
		public static (bool, UInt32) EncodeToNegativeAdjX(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newX = CalcNegativeAdjValue(x, COORDINATE_MAX_VALUE);
			if (newX > x)
			{
				overflow = true;
			}
			x = newX;
			return (overflow, Encode(x, y, z, layer));
		}
		public static (bool, UInt32) EncodeToPositiveAdjX(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newX = CalcPositiveAdjValue(x, COORDINATE_MAX_VALUE);
			if (newX < x)
			{
				overflow = true;
			}
			x = newX;
			return (overflow, Encode(x, y, z, layer));
		}
		public static (bool, UInt32) EncodeToNegativeAdjZ(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newZ = CalcNegativeAdjValue(z, COORDINATE_MAX_VALUE);
			if (newZ > z)
			{
				overflow = true;
			}
			z = newZ;
			return (overflow, Encode(x, y, z, layer));
		}
		public static (bool, UInt32) EncodeToPositiveAdjZ(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newZ = CalcPositiveAdjValue(z, COORDINATE_MAX_VALUE);
			if (newZ < z)
			{
				overflow = true;
			}
			z = newZ;
			return (overflow, Encode(x, y, z, layer));
		}
		public static (bool, UInt32) EncodeToNegativeAdjY(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newY = CalcNegativeAdjValue(y, COORDINATE_MAX_VALUE);
			if (newY > y)
			{
				overflow = true;
			}
			y = newY;
			return (overflow, Encode(x, y, z, layer));
		}
		public static (bool, UInt32) EncodeToPositiveAdjY(UInt32 code)
		{
			bool overflow = false;
			(UInt32 x, UInt32 y, UInt32 z, int layer) = Decode(code);
			UInt32 newY = CalcPositiveAdjValue(y, COORDINATE_MAX_VALUE);
			if (newY < y)
			{
				overflow = true;
			}
			y = newY;
			return (overflow, Encode(x, y, z, layer));
		}
	}
}
