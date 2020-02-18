using fNbt;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using System.Linq;
using System.Collections;

namespace McFileIo.Utility
{
    // TODO: Support Nullable<T>, List<T>

    public static class NbtClassIo
    {
        private readonly static Dictionary<Type, NbtClassReaderWriter> _cache = new Dictionary<Type, NbtClassReaderWriter>();

        /// <summary>
        /// Fill fields and properties decorated with <see cref="NbtEntryAttribute"/> with information from Nbt storage.
        /// Raise exceptions accordingly.
        /// Use fields if possible, because getter/setter may not exist for properties even if they defined in the code.
        /// </summary>
        /// <param name="target">Object to be filled</param>
        /// <param name="root">Nbt storage</param>
        public static void ReadFromNbt(INbtIoCapable target, NbtCompound root, INbtIoConfig config = null)
        {
            ReadFromNbt(target, target.GetType(), root, config);
        }

        public static T CreateAndReadFromNbt<T>(NbtCompound root, INbtIoConfig config = null) where T : INbtIoCapable, new()
        {
            var obj = new T();
            ReadFromNbt(obj, typeof(T), root, config);
            return obj;
        }

        public static List<T> ToFrameworkList<T>(this NbtList list, INbtIoConfig config = null) where T : INbtIoCapable, new()
        {
            return new List<T>(list.OfType<NbtCompound>().Select(c => CreateAndReadFromNbt<T>(c, config)));
        }

        public static object CreateAndReadFromNbt(Type type, NbtCompound root, INbtIoConfig config = null)
        {
            if (type == typeof(NbtCompound))
                return root;

            var obj = (INbtIoCapable)Activator.CreateInstance(type, true);
            ReadFromNbt(obj, type, root, config);
            return obj;
        }

        private static NbtClassReaderWriter GetProcessor(Type type)
        {
            if (!_cache.TryGetValue(type, out var processor))
                _cache[type] = processor = new NbtClassReaderWriter(type);
            return processor;
        }

        private static void ReadFromNbt(INbtIoCapable target, Type type, NbtCompound root, INbtIoConfig config = null)
        {
            if (target == null) throw new ArgumentException(nameof(target));

            if (target is INbtCustomReader reader)
            {
                reader.Read(null, root);
                return;
            }

            var baseType = type.BaseType;
            if (baseType != null && !baseType.Equals(typeof(object))
                && typeof(INbtIoCapable).IsAssignableFrom(baseType))
            {
                ReadFromNbt(target, baseType, root, config);
            }

            GetProcessor(type).ReadFromNbtCompound(target, root);
            if (target is INbtPostRead postreader)
                postreader.PostRead(null, root);
        }

        private enum FieldType
        {
            Field,
            Property
        }

        private class NbtEntryType
        {
            private NbtEntryType()
            {

            }

            public NbtTagType Type = NbtTagType.Unknown;
            public bool IsNullable = false;
            public bool IsList = false;
            public NbtEntryType NestedType = null;
            public Type FwType = null;

            public static NbtEntryType CreateFromFrameworkType(Type innerType)
            {
                var opType = innerType;

                var nullable = IsNullable(opType);

                if (innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var inlistType = innerType.GetGenericArguments()[0];
                    var wrapped = CreateFromFrameworkType(inlistType);
                    if (wrapped == null)
                        return null;

                    return new NbtEntryType
                    {
                        Type = NbtTagType.List,
                        NestedType = wrapped,
                        IsNullable = true,
                        IsList = true,
                        FwType = opType
                    };
                }

                var innerNullable = Nullable.GetUnderlyingType(opType);
                if (innerNullable != null)
                    opType = innerNullable;

                var field = DetermineType(opType);
                if (field == NbtTagType.Unknown)
                    return null;

                return new NbtEntryType
                {
                    Type = field,
                    NestedType = null,
                    IsNullable = nullable,
                    IsList = false,
                    FwType = opType
                };
            }
        }

        private struct NbtEntry
        {
            public MemberInfo Field;
            public FieldType MemberKind;
            public NbtEntryAttribute Attribute;
            public NbtEntryType ValueType;
        }

        private static readonly Type[] FieldTypeRef = new Type[] {
                null,
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(byte[]),
                typeof(string),
                null,
                null,
                typeof(int[]),
                typeof(long[])
            };

        private static NbtTagType DetermineType(Type type)
        {
            for (var i = 0; i < FieldTypeRef.Length; i++)
                if (type.Equals(FieldTypeRef[i]))
                    return (NbtTagType)i;

            if (typeof(INbtIoCapable).IsAssignableFrom(type))
                return NbtTagType.Compound;

            if (typeof(bool) == type)
                return NbtTagType.Byte;

            return NbtTagType.Unknown;
        }

        internal static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true;
            return false;
        }

        private static object CreateListFromNbt(NbtList list, NbtEntryType entryType)
        {
            switch (entryType.NestedType.Type)
            {
                case NbtTagType.Byte:
                    if (entryType.NestedType.FwType == typeof(bool))
                    {
                        return new List<bool>(list.OfType<NbtByte>().Select(x => x.Value == 1));
                    }
                    else
                    {
                        return new List<byte>(list.OfType<NbtByte>().Select(x => x.Value));
                    }
                case NbtTagType.Short:
                    return new List<short>(list.OfType<NbtShort>().Select(x => x.Value));
                case NbtTagType.Int:
                    return new List<int>(list.OfType<NbtInt>().Select(x => x.Value));
                case NbtTagType.Long:
                    return new List<long>(list.OfType<NbtLong>().Select(x => x.Value));
                case NbtTagType.Float:
                    return new List<float>(list.OfType<NbtFloat>().Select(x => x.Value));
                case NbtTagType.Double:
                    return new List<double>(list.OfType<NbtDouble>().Select(x => x.Value));
                case NbtTagType.ByteArray:
                    return new List<byte[]>(list.OfType<NbtByteArray>().Select(x => x.Value));
                case NbtTagType.String:
                    return new List<string>(list.OfType<NbtString>().Select(x => x.Value));
                case NbtTagType.IntArray:
                    return new List<int[]>(list.OfType<NbtIntArray>().Select(x => x.Value));
                case NbtTagType.LongArray:
                    return new List<long[]>(list.OfType<NbtLongArray>().Select(x => x.Value));
                case NbtTagType.List:
                case NbtTagType.Compound:
                    var inst = (IList)Activator.CreateInstance(entryType.FwType);
                    foreach (var it in list.Select(t => CreateValueFromNbtTag(t, entryType.NestedType)))
                        inst.Add(it);
                    return inst;
                default:
                    throw new NotSupportedException();
            }
        }

        private static object CreateValueFromNbtTag(NbtTag tag, NbtEntryType entryType)
        {
            try
            {
                switch (entryType.Type)
                {
                    case NbtTagType.Byte:
                        if(entryType.FwType == typeof(bool))
                        {
                            return tag.ByteValue == 1;
                        }
                        else
                        {
                            return tag.ByteValue;
                        }
                    case NbtTagType.Short:
                        return tag.ShortValue;
                    case NbtTagType.Int:
                        return tag.IntValue;
                    case NbtTagType.Long:
                        return tag.LongValue;
                    case NbtTagType.Float:
                        return tag.FloatValue;
                    case NbtTagType.Double:
                        return tag.DoubleValue;
                    case NbtTagType.ByteArray:
                        return tag.ByteArrayValue;
                    case NbtTagType.String:
                        return tag.StringValue;
                    case NbtTagType.IntArray:
                        return tag.IntArrayValue;
                    case NbtTagType.LongArray:
                        return tag.LongArrayValue;
                    case NbtTagType.List:
                        if (!(tag is NbtList list))
                            return null;
                        return CreateListFromNbt(list, entryType);
                    case NbtTagType.Compound:
                        if (!(tag is NbtCompound compound))
                            return null;
                        return CreateAndReadFromNbt(entryType.FwType, compound);
                    default:
                        throw new NotSupportedException();
                }
            }
            catch (InvalidCastException)
            {
                return null;
            }
            catch (TargetInvocationException)
            {
                return null;
            }
        }

        private static void SetMemberValue(NbtEntry entry, object target, object value)
        {
            switch (entry.MemberKind)
            {
                case FieldType.Field:
                    (entry.Field as FieldInfo).SetValue(target, value);
                    break;
                case FieldType.Property:
                    (entry.Field as PropertyInfo).SetValue(target, value);
                    break;
            }
        }

        private static void ThrowTypeMismatch(NbtEntry entry, object target)
        {
            if (entry.Attribute.Optional)
            {
                ExceptionHelper.ThrowParseError($"Type mismatch parsing optional {entry.Field.Name} in {target.GetType().FullName}", ParseErrorLevel.Information);
            }
            else
            {
                ExceptionHelper.ThrowParseError($"Type mismatch parsing required {entry.Field.Name} in {target.GetType().FullName}", ParseErrorLevel.Exception);
            }
        }

        private static void SetValueToClassMember(NbtTag tag, NbtEntry entry, object target)
        {
            var resultObj = CreateValueFromNbtTag(tag, entry.ValueType);
            if (resultObj == null)
            {
                ThrowTypeMismatch(entry, target);
                return;
            }

            SetMemberValue(entry, target, resultObj);
        }

        private class NbtClassReaderWriter
        {
            private readonly List<NbtEntry> _nbtEntries = new List<NbtEntry>();

            private void CreateFromMemberInfo(MemberInfo memberinfo, NbtEntryAttribute baseAttribute)
            {
                FieldType fieldType;
                Type innerType;

                switch (memberinfo)
                {
                    case PropertyInfo property:
                        fieldType = FieldType.Property;
                        innerType = property.PropertyType;
                        break;
                    case FieldInfo field:
                        fieldType = FieldType.Field;
                        innerType = field.FieldType;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var type = NbtEntryType.CreateFromFrameworkType(innerType);
                if (type == null) return;

                _nbtEntries.Add(new NbtEntry
                {
                    Field = memberinfo,
                    MemberKind = fieldType,
                    ValueType = type,
                    Attribute = baseAttribute
                });
            }

            public NbtClassReaderWriter(Type type)
            {
                var fields = type.GetMembers(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);

                foreach(var field in fields)
                {
                    if (!(field is PropertyInfo) && !(field is FieldInfo)) continue;

                    var nbtAttributes = field.GetCustomAttribute<NbtEntryAttribute>();
                    if (nbtAttributes != null)
                    {
                        CreateFromMemberInfo(field, nbtAttributes);
                    }
                }
            }

            public void ReadFromNbtCompound(INbtIoCapable target, NbtCompound compound)
            {
                foreach(var entry in _nbtEntries)
                {
                    var result = compound.TryGet(entry.Attribute.TagName ?? entry.Field.Name, out var tag);
                    if (!result)
                    {
                        if (!entry.Attribute.Optional)
                            ExceptionHelper.ThrowParseMissingError(entry.Field.Name, ParseErrorLevel.Exception);

                        continue;
                    }

                    SetValueToClassMember(tag, entry, target);
                }
            }
        }
    }
}
