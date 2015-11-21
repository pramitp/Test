using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public static class SerializationExtensionMethods
    {
        public static void WritePrimitive(this BinaryWriter bwWriter, object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Boolean:
                    bwWriter.Write((bool)obj);
                    break;
                case TypeCode.Byte:
                    bwWriter.Write((byte)obj);
                    break;
                case TypeCode.Char:
                    bwWriter.Write((char)obj);
                    break;
                case TypeCode.Decimal:
                    bwWriter.Write((decimal)obj);
                    break;
                case TypeCode.Double:
                    bwWriter.Write((double)obj);
                    break;
                case TypeCode.Single:
                    bwWriter.Write((float)obj);
                    break;
                case TypeCode.Int16:
                    bwWriter.Write((short)obj);
                    break;
                case TypeCode.Int32:
                    bwWriter.Write((int)obj);
                    break;
                case TypeCode.Int64:
                    bwWriter.Write((long)obj);
                    break;
                case TypeCode.String:
                    bwWriter.Write((string)obj);
                    break;
                case TypeCode.SByte:
                    bwWriter.Write((sbyte)obj);
                    break;
                case TypeCode.UInt16:
                    bwWriter.Write((ushort)obj);
                    break;
                case TypeCode.UInt32:
                    bwWriter.Write((uint)obj);
                    break;
                case TypeCode.UInt64:
                    bwWriter.Write((ulong)obj);
                    break;
                default:
                    if (obj.GetType() == typeof(byte[]))
                    {
                        bwWriter.Write((byte[])obj);
                    }
                    else if (obj.GetType() == typeof(char[]))
                    {
                        bwWriter.Write((char[])obj);
                    }
                    else
                    {
                        throw new ArgumentException("Type not supported");
                    }
                    break;
            
            }
        }

        public static object ReadPrimitive(this BinaryReader br, Type type)
        {
            // Support for Nullable types
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);

            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return br.ReadBoolean();
                    break;
                case TypeCode.Byte:
                    return br.ReadByte();
                    break;
                case TypeCode.Char:
                    return br.ReadChar();
                    break;
                case TypeCode.Decimal:
                    return br.ReadDecimal();
                    break;
                case TypeCode.Double:
                    return br.ReadDouble();
                    break;
                case TypeCode.Single:
                    return br.ReadSingle();
                    break;
                case TypeCode.Int16:
                    return br.ReadInt16();
                    break;
                case TypeCode.Int32:
                    return br.ReadInt32();
                    break;
                case TypeCode.Int64:
                    return br.ReadInt64();
                    break;
                case TypeCode.String:
                    return br.ReadString();
                    break;
                case TypeCode.SByte:
                    return br.ReadSByte();
                    break;
                case TypeCode.UInt16:
                    return br.ReadUInt16();
                    break;
                case TypeCode.UInt32:
                    return br.ReadInt32();
                    break;
                case TypeCode.UInt64:
                    return br.ReadUInt64();
                    break;
                default:
                    //if (fieldInfo.FieldType == typeof(byte[]))
                    //{
                    //    return br.ReadBytes(10);
                    //}
                    //else if (fieldInfo.FieldType == typeof(char[]))
                    //{
                    //    return br.ReadChars(10);
                    //}
                    //else
                    {
                        throw new ArgumentException("Type not supported");
                    }

                    break;
            }
        }

        public static void Serialize(this object obj, Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, (Encoding)new UTF8Encoding(false, true), true))
            {
                IDictionary<object, int> refIdDictionary = new Dictionary<object, int>();
                writer.Write(obj == null);
                if (obj == null)
                    return;
                writer.Write(obj.GetHashCode());
                refIdDictionary.Add(obj, obj.GetHashCode());
                Serialize(obj, writer, ref refIdDictionary);
            }
        }

        public static T DeSerialize<T>(this Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, (Encoding)new UTF8Encoding(), true))
            {
                IDictionary<int, object> idRefDictionary = new Dictionary<int, object>();
                if (reader.ReadBoolean())
                    throw new NullReferenceException();
                object obj = FormatterServices.GetUninitializedObject(typeof(T));
                //object obj = Activator.CreateInstance(typeof(T));
                idRefDictionary.Add(reader.ReadInt32(), obj);
                DeSerialize(obj, reader, ref idRefDictionary);
                return (T)obj;
            }
        }

        private static void Serialize(object obj, BinaryWriter bw, ref IDictionary<object, int> refIdDict)
        {
            var binding = BindingFlags.Instance
                 | BindingFlags.NonPublic
                 | BindingFlags.Public
                 | BindingFlags.Static;
            
            var type = obj.GetType();
            var fields = type.GetFields(binding).Cast<MemberInfo>();
            var props = type.GetProperties().Cast<MemberInfo>();
            var fieldsAndProperties = fields.Union(props);

            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsLiteral || fieldInfo.IsNotSerialized)
                    continue;
                //if (obj.GetType().Assembly.FullName.Contains("mscorlib") && fieldInfo.IsPrivate)
                //    continue;
                Type objectType = fieldInfo.FieldType;
                object objectValue = fieldInfo.GetValue(obj);
                if (IsTypeValueOrPrimitive(objectType))
                    bw.WriteValueOrPrimitive(ref refIdDict, objectType, objectValue);
                else // if (fieldInfo.FieldType.IsByRef)
                    bw.WriteComplexObject(ref refIdDict, objectType, objectValue);
            }
        }

        private static bool IsTypeValueOrPrimitive(Type objectType)
        {
            return objectType.IsValueType || objectType.IsPrimitive ||
                   objectType == typeof(string);
        }

        private static void WriteValueOrPrimitive(this BinaryWriter bw, ref IDictionary<object, int> refIdDict, Type objectType,
            object objectValue)
        {
            if (objectType == typeof(string) ||
                (objectType.IsGenericType &&
                 objectType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                bw.WritePrimitive(objectValue == null);
            }

            if (objectValue == null)
                return;
            if (objectValue.GetType().IsPrimitive || objectValue is string)
            {
                bw.WritePrimitive(objectValue);
            }
            else //if (objectType.IsValueType)
            {
                Serialize(objectValue, bw, ref refIdDict);
            }
        }

        private static void WriteComplexObject(this BinaryWriter bw, ref IDictionary<object, int> refIdDict, Type objectType, object objectValue)
        {
            bool isNull = objectValue == null;
            bw.WritePrimitive(isNull);
            if (isNull)
                return;
            if (refIdDict.ContainsKey(objectValue))
            {
                bw.Write(refIdDict[objectValue]);
            }
            else
            {
                refIdDict.Add(objectValue, objectValue.GetHashCode());
                bw.Write(refIdDict[objectValue]);
                string typeName;
                if (objectType.IsClass || objectType.IsInterface)
                {
                    if (objectType.IsGenericType)
                        typeName = objectValue.GetType().Name;
                    else
                        typeName = objectValue.GetType().Name;
                    
                    bw.WritePrimitive(typeName);
                }
                if (objectType.IsArray)
                {
                    Array arr = (Array)objectValue;
                    bw.Write(arr.Length);
                    foreach (var o in arr)
                    {
                        Type elementType = arr.GetType().GetElementType();
                        if (IsTypeValueOrPrimitive(elementType))
                        {
                            bw.WriteValueOrPrimitive(ref refIdDict, elementType, o);
                        }
                        else
                        {
                            bw.WriteComplexObject(ref refIdDict, elementType, o);
                        }
                    }
                }
                //else if (objectType.IsGenericType 
                //    && (objectType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                //    objectType.GetInterfaces().Where(i => i.IsGenericType &&
                //        i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).Count() == 1))
                //{
                //    dynamic dictionary = objectValue;
                //    bw.Write(dictionary.Count);
                //    foreach (var kvp in dictionary)
                //    {
                //        Type keyType = kvp.Key.GetType();
                //        if (IsTypeValueOrPrimitive(keyType))
                //        {
                //            bw.WriteValueOrPrimitive(ref refIdDict, keyType, (object) kvp.Key);
                //        }
                //        else
                //        {
                //            bw.WriteComplexObject(ref refIdDict, keyType, (object) kvp.Key);
                //        }
                //        Type valueType = kvp.Value.GetType();
                //        if (IsTypeValueOrPrimitive(valueType))
                //        {
                //            bw.WriteValueOrPrimitive(ref refIdDict, valueType, (object) kvp.Value);
                //        }
                //        else
                //        {
                //            bw.WriteComplexObject(ref refIdDict, valueType, (object) kvp.Value);
                //        }
                //    }
                //}
                else if (objectType.IsClass || objectType.IsInterface)
                {
                    Serialize(objectValue, bw, ref refIdDict);
                }
            }
        }

        private static void DeSerialize(object obj, BinaryReader br, ref IDictionary<int, object> refDict)
        {
            
            var binding = BindingFlags.Instance
                 | BindingFlags.NonPublic
                 | BindingFlags.Public
                 | BindingFlags.Static;

            var type = obj.GetType();
            var fields = type.GetFields(binding).Cast<MemberInfo>();
            var props = type.GetProperties().Cast<MemberInfo>();
            var fieldsAndProperties = fields.Union(props);

            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsLiteral || fieldInfo.IsNotSerialized)
                    continue;
                //if (obj.GetType().Assembly.FullName.Contains("mscorlib") && fieldInfo.IsPrivate)
                //    continue;
                Type fieldType = fieldInfo.FieldType;
                if (IsTypeValueOrPrimitive(fieldType))
                {
                    object objValue = br.ReadValueOrPrimitive(ref refDict, fieldType);
                    fieldInfo.SetValue(obj, objValue);
                }
                else
                {
                    object objValue = br.ReadComplexObject(ref refDict, fieldType);
                    fieldInfo.SetValue(obj, objValue);
                }
            }
        }

        private static object ReadValueOrPrimitive(this BinaryReader br, ref IDictionary<int, object> refDict, Type fieldType)
        {
            object objValue;
            bool isNull = false;
            if (fieldType == typeof(string) ||
                (fieldType.IsGenericType &&
                 fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                isNull = br.ReadBoolean();
            }
            if (isNull)
            {
                return null;
            }

            if (fieldType.IsPrimitive || fieldType == typeof(string) ||
                (Nullable.GetUnderlyingType(fieldType) != null && Nullable.GetUnderlyingType(fieldType).IsPrimitive))
            {
                objValue = br.ReadPrimitive(fieldType);
            }
            else //if (fieldType.IsValueType)
            {
                object newObj = FormatterServices.GetUninitializedObject(fieldType);
                //object newObj = Activator.CreateInstance(fieldInfo.FieldType);
                DeSerialize(newObj, br, ref refDict);
                objValue = newObj;

            }
            return objValue;
        }

        private static object ReadComplexObject(this BinaryReader br, ref IDictionary<int, object> refDict, Type fieldType)
        {
            object objectValue = null;
            bool isNull = false;
            isNull = br.ReadBoolean();
            if (isNull)
            {
                return null;
            }
            int objHash = br.ReadInt32();
            if (refDict.ContainsKey(objHash))
            {
                objectValue = refDict[objHash];
            }
            else
            {
                string typeName = "";
                if (fieldType.IsClass || fieldType.IsInterface)
                {
                    typeName = (string)br.ReadString();
                }
                if (fieldType.IsArray)
                {
                    int arrSize = br.ReadInt32();
                    Array objArray = Array.CreateInstance(fieldType.GetElementType(), arrSize);
                    refDict.Add(objHash, objArray);
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        Type elementType = objArray.GetType().GetElementType();
                        if (IsTypeValueOrPrimitive(elementType))
                        {
                            object objValue = br.ReadValueOrPrimitive(ref refDict, elementType);
                            objArray.SetValue(objValue, i);
                        }
                        else
                        {
                            object objValue = br.ReadComplexObject(ref refDict, elementType);
                            objArray.SetValue(objValue, i);
                        }
                    }
                    objectValue = objArray;
                }
                //else if (fieldType.IsGenericType
                //    && (fieldType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                //    fieldType.GetInterfaces().Where(i => i.IsGenericType &&
                //        i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).Count() == 1))
                //{
                //    int arrSize = br.ReadInt32();
                //    Type[] tyepArgs = fieldType.GetGenericArguments();
                //    var d1 = Type.GetType(fieldType.Namespace + "." + typeName);
                //    dynamic dict = Activator.CreateInstance(d1.MakeGenericType(tyepArgs));
                //    refDict.Add(objHash, dict);
                //    for (int i = 0; i < arrSize; i++)
                //    {
                //        dynamic key;
                //        Type keyType = tyepArgs[0];
                //        if (IsTypeValueOrPrimitive(keyType))
                //        {
                //            object objValue = br.ReadValueOrPrimitive(ref refDict, keyType);
                //            key = objValue;
                //        }
                //        else
                //        {
                //            object objValue = br.ReadComplexObject(ref refDict, keyType);
                //            key = objValue;
                //        }

                //        dynamic value;
                //        Type valueType = tyepArgs[1];
                //        if (IsTypeValueOrPrimitive(valueType))
                //        {
                //            object objValue = br.ReadValueOrPrimitive(ref refDict, valueType);
                //            value = objValue;
                //        }
                //        else
                //        {
                //            object objValue = br.ReadComplexObject(ref refDict, valueType);
                //            value = objValue;
                //        }
                //        dict.Add(key, value);
                //    }
                //    objectValue = dict;
                //}
                else if (fieldType.IsClass || fieldType.IsInterface)
                {
                    object newObj;
                    if (fieldType.IsGenericType)
                    {
                        Type[] tyepArgs = fieldType.GetGenericArguments();
                        var d1 = Type.GetType(fieldType.Namespace + "." + typeName);
                        newObj = Activator.CreateInstance(d1.MakeGenericType(tyepArgs));
                    }
                    else
                    {
                        newObj = FormatterServices.GetUninitializedObject(Type.GetType(fieldType.Namespace + "." + typeName));
                    }
                    //object newObj = Activator.CreateInstance(Type.GetType(fieldType.Namespace + "." + typeName));
                    refDict.Add(objHash, newObj);
                    DeSerialize(refDict[objHash], br, ref refDict);
                    objectValue = refDict[objHash];
                }
            }
            return objectValue;
        }
    }
}
