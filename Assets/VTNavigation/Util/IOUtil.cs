using System.IO;
using UnityEngine;
using VTNavigation.Geometry;

namespace VTNavigation.Util
{
    public class IOUtil
    {
        public static void WriteInt(BinaryWriter bw, int val)
        {
            bw.Write(val);
        }
        public static void WriteFloat(BinaryWriter binaryWriter, float value)
        {
            binaryWriter.Write(value);
        }

        public static void WriteVector3(BinaryWriter binaryWriter, Vector3 value)
        {
            WriteFloat(binaryWriter, value.x);
            WriteFloat(binaryWriter, value.y);
            WriteFloat(binaryWriter, value.z);
        }

        public static void WriteVector2(BinaryWriter binaryWriter, Vector2 value)
        {
            WriteFloat(binaryWriter, value.x);
            WriteFloat(binaryWriter, value.y);
        }

        public static void WriteBounds(BinaryWriter binaryWriter, Bounds value)
        {
            WriteVector3(binaryWriter, value.center);
            WriteVector3(binaryWriter, value.size);
        }

        public static float ReadFloat(BinaryReader binaryReader)
        {
            return binaryReader.ReadSingle();
        }

        public static Vector3 ReadVector3(BinaryReader binaryReader)
        {
            return new Vector3(ReadFloat(binaryReader), ReadFloat(binaryReader), ReadFloat(binaryReader));
        }
        
        public static Vector2 ReadVector2(BinaryReader binaryReader)
        {
            return new Vector2(ReadFloat(binaryReader), ReadFloat(binaryReader));
        }

        public static Bounds ReadBounds(BinaryReader binaryReader)
        {
            return new Bounds(ReadVector3(binaryReader), ReadVector3(binaryReader));
        }

        public static int ReadInt(BinaryReader binaryReader)
        {
            return binaryReader.ReadInt32();
        }
    }
}