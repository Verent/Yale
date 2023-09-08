﻿using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Yale.Parser.Internal
{
    internal class ImplicitConverter
    {
        /// <summary>
        /// Table of results for binary operations using primitives
        /// </summary>
        private static readonly Type[,] OurBinaryResultTable;

        /// <summary>
        /// Primitive types we support
        /// </summary>
        private static readonly Type[] OurBinaryTypes;

        static ImplicitConverter()
        {
            // Create a table with all the primitive types
            Type[] types = {
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(Int64),
            typeof(UInt64),
            typeof(float),
            typeof(double)
        };
            OurBinaryTypes = types;
            Type[,] table = new Type[types.Length, types.Length];
            OurBinaryResultTable = table;
            FillIdentities(types, table);

            // Fill the table
            AddEntry(typeof(uint), typeof(UInt64), typeof(UInt64));
            AddEntry(typeof(int), typeof(long), typeof(long));
            AddEntry(typeof(uint), typeof(long), typeof(long));
            AddEntry(typeof(int), typeof(uint), typeof(long));
            AddEntry(typeof(uint), typeof(float), typeof(float));
            AddEntry(typeof(uint), typeof(double), typeof(double));
            AddEntry(typeof(int), typeof(float), typeof(float));
            AddEntry(typeof(int), typeof(double), typeof(double));
            AddEntry(typeof(long), typeof(float), typeof(float));
            AddEntry(typeof(long), typeof(double), typeof(double));
            AddEntry(typeof(UInt64), typeof(float), typeof(float));
            AddEntry(typeof(UInt64), typeof(double), typeof(double));
            AddEntry(typeof(float), typeof(double), typeof(double));

            // Byte
            AddEntry(typeof(byte), typeof(byte), typeof(Int32));
            AddEntry(typeof(byte), typeof(sbyte), typeof(Int32));
            AddEntry(typeof(byte), typeof(Int16), typeof(Int32));
            AddEntry(typeof(byte), typeof(UInt16), typeof(Int32));
            AddEntry(typeof(byte), typeof(Int32), typeof(Int32));
            AddEntry(typeof(byte), typeof(UInt32), typeof(UInt32));
            AddEntry(typeof(byte), typeof(Int64), typeof(Int64));
            AddEntry(typeof(byte), typeof(UInt64), typeof(UInt64));
            AddEntry(typeof(byte), typeof(float), typeof(float));
            AddEntry(typeof(byte), typeof(double), typeof(double));

            // SByte
            AddEntry(typeof(sbyte), typeof(sbyte), typeof(Int32));
            AddEntry(typeof(sbyte), typeof(Int16), typeof(Int32));
            AddEntry(typeof(sbyte), typeof(UInt16), typeof(Int32));
            AddEntry(typeof(sbyte), typeof(Int32), typeof(Int32));
            AddEntry(typeof(sbyte), typeof(UInt32), typeof(long));
            AddEntry(typeof(sbyte), typeof(Int64), typeof(Int64));
            //invalid -- AddEntry(GetType(SByte), GetType(UInt64), GetType(UInt64))
            AddEntry(typeof(sbyte), typeof(float), typeof(float));
            AddEntry(typeof(sbyte), typeof(double), typeof(double));

            // int16
            AddEntry(typeof(Int16), typeof(Int16), typeof(Int32));
            AddEntry(typeof(Int16), typeof(UInt16), typeof(Int32));
            AddEntry(typeof(Int16), typeof(Int32), typeof(Int32));
            AddEntry(typeof(Int16), typeof(UInt32), typeof(long));
            AddEntry(typeof(Int16), typeof(Int64), typeof(Int64));
            //invalid -- AddEntry(GetType(Int16), GetType(UInt64), GetType(UInt64))
            AddEntry(typeof(Int16), typeof(float), typeof(float));
            AddEntry(typeof(Int16), typeof(double), typeof(double));

            // Uint16
            AddEntry(typeof(UInt16), typeof(UInt16), typeof(Int32));
            AddEntry(typeof(UInt16), typeof(Int16), typeof(Int32));
            AddEntry(typeof(UInt16), typeof(Int32), typeof(Int32));
            AddEntry(typeof(UInt16), typeof(UInt32), typeof(UInt32));
            AddEntry(typeof(UInt16), typeof(Int64), typeof(Int64));
            AddEntry(typeof(UInt16), typeof(UInt64), typeof(UInt64));
            AddEntry(typeof(UInt16), typeof(float), typeof(float));
            AddEntry(typeof(UInt16), typeof(double), typeof(double));

            // Char
            AddEntry(typeof(char), typeof(char), typeof(Int32));
            AddEntry(typeof(char), typeof(UInt16), typeof(UInt16));
            AddEntry(typeof(char), typeof(Int32), typeof(Int32));
            AddEntry(typeof(char), typeof(UInt32), typeof(UInt32));
            AddEntry(typeof(char), typeof(Int64), typeof(Int64));
            AddEntry(typeof(char), typeof(UInt64), typeof(UInt64));
            AddEntry(typeof(char), typeof(float), typeof(float));
            AddEntry(typeof(char), typeof(double), typeof(double));
        }

        private ImplicitConverter()
        {
        }

        private static void FillIdentities(Type[] typeArray, Type[,] table)
        {
            for (int i = 0; i <= typeArray.Length - 1; i++)
            {
                Type type = typeArray[i];
                table[i, i] = type;
            }
        }

        private static void AddEntry(Type type1, Type type2, Type result)
        {
            int index1 = GetTypeIndex(type1);
            int index2 = GetTypeIndex(type2);
            OurBinaryResultTable[index1, index2] = result;
            OurBinaryResultTable[index2, index1] = result;
        }

        private static int GetTypeIndex(Type type)
        {
            return Array.IndexOf(OurBinaryTypes, type);
        }

        public static bool EmitImplicitConvert(Type sourceType, Type destinationType, YaleIlGenerator ilGenerator)
        {
            if (ReferenceEquals(sourceType, destinationType))
            {
                return true;
            }

            if (EmitOverloadedImplicitConvert(sourceType, destinationType, ilGenerator))
            {
                return true;
            }

            if (ImplicitConvertToReferenceType(sourceType, destinationType, ilGenerator))
            {
                return true;
            }

            return ImplicitConvertToValueType(sourceType, destinationType, ilGenerator);
        }

        private static bool EmitOverloadedImplicitConvert(Type sourceType, Type destinationType, YaleIlGenerator ilGenerator)
        {
            // Look for an implicit operator on the destination type
            System.Reflection.MethodInfo methodInfo = Utility.GetSimpleOverloadedOperator("Implicit", sourceType, destinationType);

            if (methodInfo == null)
            {
                return false;
            }

            ilGenerator?.Emit(OpCodes.Call, methodInfo);

            return true;
        }

        private static bool ImplicitConvertToReferenceType(Type sourceType, Type destinationType, YaleIlGenerator ilGenerator)
        {
            if (destinationType.IsValueType)
            {
                return false;
            }

            if (ReferenceEquals(sourceType, typeof(Null)))
            {
                // Null is always convertible to a reference type
                return true;
            }

            if (destinationType.IsAssignableFrom(sourceType) == false)
            {
                return false;
            }

            if (sourceType.IsValueType)
            {
                ilGenerator?.Emit(OpCodes.Box, sourceType);
            }

            return true;
        }

        private static bool ImplicitConvertToValueType(Type sourceType, Type destinationType, YaleIlGenerator ilGenerator)
        {
            // We only handle value types
            if (sourceType.IsValueType == false && destinationType.IsValueType == false)
            {
                return false;
            }

            // No implicit conversion to enum.  Have to do this check here since calling GetTypeCode on an enum will return the typecode
            // of the underlying type which screws us up.
            if (sourceType.IsEnum || destinationType.IsEnum)
            {
                return false;
            }

            return EmitImplicitNumericConvert(sourceType, destinationType, ilGenerator);
        }

        /// <summary>
        ///Emit an implicit conversion (if the ilg is not null) and returns a value that determines whether the implicit conversion
        /// succeeded
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="ilGenerator"></param>
        /// <returns></returns>
        public static bool EmitImplicitNumericConvert(Type sourceType, Type destinationType, YaleIlGenerator ilGenerator)
        {
            TypeCode sourceTypeCode = Type.GetTypeCode(sourceType);
            TypeCode destTypeCode = Type.GetTypeCode(destinationType);

            switch (destTypeCode)
            {
                case TypeCode.Int16:
                    return ImplicitConvertToInt16(sourceTypeCode);

                case TypeCode.UInt16:
                    return ImplicitConvertToUInt16(sourceTypeCode);

                case TypeCode.Int32:
                    return ImplicitConvertToInt32(sourceTypeCode);

                case TypeCode.UInt32:
                    return ImplicitConvertToUInt32(sourceTypeCode);

                case TypeCode.Double:
                    return ImplicitConvertToDouble(sourceTypeCode, ilGenerator);

                case TypeCode.Single:
                    return ImplicitConvertToSingle(sourceTypeCode, ilGenerator);

                case TypeCode.Int64:
                    return ImplicitConvertToInt64(sourceTypeCode, ilGenerator);

                case TypeCode.UInt64:
                    return ImplicitConvertToUInt64(sourceTypeCode, ilGenerator);

                default:
                    return false;
            }
        }

        private static bool ImplicitConvertToInt16(TypeCode sourceTypeCode)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                    return true;

                default:
                    return false;
            }
        }

        private static bool ImplicitConvertToUInt16(TypeCode sourceTypeCode)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                    return true;

                default:
                    return false;
            }
        }

        private static bool ImplicitConvertToInt32(TypeCode sourceTypeCode)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return true;

                default:
                    return false;
            }
        }

        private static bool ImplicitConvertToUInt32(TypeCode sourceTypeCode)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return true;

                default:
                    return false;
            }
        }

        private static bool ImplicitConvertToDouble(TypeCode sourceTypeCode, YaleIlGenerator ilGenerator)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.Int64:
                    EmitConvert(ilGenerator, OpCodes.Conv_R8);
                    break;

                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    EmitConvert(ilGenerator, OpCodes.Conv_R_Un);
                    EmitConvert(ilGenerator, OpCodes.Conv_R8);
                    break;

                case TypeCode.Double:
                    break;

                default:
                    return false;
            }

            return true;
        }

        private static bool ImplicitConvertToSingle(TypeCode sourceTypeCode, YaleIlGenerator ilGenerator)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    EmitConvert(ilGenerator, OpCodes.Conv_R4);
                    break;

                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    EmitConvert(ilGenerator, OpCodes.Conv_R_Un);
                    EmitConvert(ilGenerator, OpCodes.Conv_R4);
                    break;

                case TypeCode.Single:
                    break;

                default:
                    return false;
            }

            return true;
        }

        private static bool ImplicitConvertToInt64(TypeCode sourceTypeCode, YaleIlGenerator ilGenerator)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    EmitConvert(ilGenerator, OpCodes.Conv_I8);
                    break;

                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    EmitConvert(ilGenerator, OpCodes.Conv_U8);
                    break;

                case TypeCode.Int64:
                    break;

                default:
                    return false;
            }

            return true;
        }

        private static bool ImplicitConvertToUInt64(TypeCode sourceTypeCode, YaleIlGenerator ilGenerator)
        {
            switch (sourceTypeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    EmitConvert(ilGenerator, OpCodes.Conv_U8);
                    break;

                case TypeCode.UInt64:
                    break;

                default:
                    return false;
            }

            return true;
        }

        private static void EmitConvert(YaleIlGenerator ilg, OpCode convertOpcode)
        {
            ilg?.Emit(convertOpcode);
        }

        /// <summary>
        /// Get the result type for a binary operation
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static Type GetBinaryResultType(Type t1, Type t2)
        {
            int index1 = GetTypeIndex(t1);
            int index2 = GetTypeIndex(t2);

            if (index1 == -1 || index2 == -1)
            {
                return null;
            }

            return OurBinaryResultTable[index1, index2];
        }

        public static int GetImplicitConvertScore(Type sourceType, Type destinationType)
        {
            if (ReferenceEquals(sourceType, destinationType))
            {
                return 0;
            }

            if (ReferenceEquals(sourceType, typeof(Null)))
            {
                return GetInverseDistanceToObject(destinationType);
            }

            if (Utility.GetSimpleOverloadedOperator("Implicit", sourceType, destinationType) != null)
            {
                // Implicit operator conversion, score it at 1 so it's just above the minimum
                return 1;
            }

            if (sourceType.IsValueType)
            {
                if (destinationType.IsValueType)
                {
                    // Value type -> value type
                    int sourceScore = GetValueTypeImplicitConvertScore(sourceType);
                    int destinationScore = GetValueTypeImplicitConvertScore(destinationType);

                    return destinationScore - sourceScore;
                }

                // Value type -> reference type
                return GetReferenceTypeImplicitConvertScore(sourceType, destinationType);
            }

            if (destinationType.IsValueType)
            {
                // Reference type -> value type
                // Reference types can never be implicitly converted to value types
                Debug.Fail("No implicit conversion from reference type to value type");
            }
            else
            {
                // Reference type -> reference type
                return GetReferenceTypeImplicitConvertScore(sourceType, destinationType);
            }
            return 0;
        }

        private static int GetValueTypeImplicitConvertScore(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Byte:
                    return 1;

                case TypeCode.SByte:
                    return 2;

                case TypeCode.Char:
                    return 3;

                case TypeCode.Int16:
                    return 4;

                case TypeCode.UInt16:
                    return 5;

                case TypeCode.Int32:
                    return 6;

                case TypeCode.UInt32:
                    return 7;

                case TypeCode.Int64:
                    return 8;

                case TypeCode.UInt64:
                    return 9;

                case TypeCode.Single:
                    return 10;

                case TypeCode.Double:
                    return 11;

                case TypeCode.Decimal:
                    return 11;

                case TypeCode.Boolean:
                    return 12;

                case TypeCode.DateTime:
                    return 13;

                default:
                    return -1;
            }
        }

        private static int GetReferenceTypeImplicitConvertScore(Type sourceType, Type destinationType)
        {
            return destinationType.IsInterface ? 100 : GetInheritanceDistance(sourceType, destinationType);
        }

        private static int GetInheritanceDistance(Type sourceType, Type destinationType)
        {
            int count = 0;
            Type current = sourceType;

            while (!ReferenceEquals(current, destinationType))
            {
                count += 1;
                current = current.BaseType;
            }

            return count * 1000;
        }

        private static int GetInverseDistanceToObject(Type t)
        {
            int score = 1000;
            Type current = t.BaseType;

            while (current != null)
            {
                score -= 100;
                current = current.BaseType;
            }

            return score;
        }
    }
}