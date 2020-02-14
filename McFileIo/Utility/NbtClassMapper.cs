using fNbt;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using McFileIo.Attributes;
using McFileIo.Interfaces;

namespace McFileIo.Utility
{
    public static class NbtClassMapper
    {
        private readonly static Dictionary<Type, NbtEnabledClassWrapper> _cache = new Dictionary<Type, NbtEnabledClassWrapper>();

        /// <summary>
        /// Fill fields and properties decorated with <see cref="NbtEntryAttribute"/> with information from Nbt storage.
        /// Raise exceptions accordingly.
        /// Use fields if possible, because getter/setter may not exist for properties even if they defined in the code.
        /// </summary>
        /// <param name="target">Object to be filled</param>
        /// <param name="root">Nbt storage</param>
        /// <param name="alsoFillBaseClass">Default <see langword="true"/>. Control if base classes are also filled.</param>
        public static void ReadFromNbt(INbtMapperCapable target, NbtCompound root, bool alsoFillBaseClass = true)
        {
            ReadFromNbt(target, target.GetType(), root, alsoFillBaseClass);
        }

        private static void ReadFromNbt(INbtMapperCapable target, Type type, NbtCompound root, bool alsoFillBaseClass = true)
        {
            if (target == null) throw new ArgumentException(nameof(target));

            if (alsoFillBaseClass)
            {
                var baseType = type.BaseType;
                if (baseType != null && !baseType.Equals(typeof(object))
                    && typeof(INbtMapperCapable).IsAssignableFrom(baseType))
                {
                    ReadFromNbt(target, baseType, root, true);
                }
            }

            if (!_cache.TryGetValue(type, out var processor))
                _cache[type] = processor = new NbtEnabledClassWrapper(type);

            processor.ReadFromNbtCompound(target, root);
        }

        private class NbtEnabledClassWrapper
        {
            private enum FieldType
            {
                Unknown = -1,
                End = 0,
                Byte,
                Short,
                Int,
                Long,
                Float,
                Double,
                ByteArray,
                String,
                List,
                Compound,
                IntArray,
                LongArray
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

            private readonly List<(FieldInfo field, FieldType type, NbtEntryAttribute attribute)>
                _fieldList = new List<(FieldInfo field, FieldType type, NbtEntryAttribute attribute)>();
            private readonly List<(PropertyInfo field, FieldType type, NbtEntryAttribute attribute)>
                _propertyList = new List<(PropertyInfo field, FieldType type, NbtEntryAttribute attribute)>();

            private FieldType DetermineType(Type type)
            {
                for (var i = 0; i < FieldTypeRef.Length; i++)
                    if (type.Equals(FieldTypeRef[i]))
                        return (FieldType)i;

                if (typeof(INbtMapperCapable).IsAssignableFrom(type))
                    return FieldType.Compound;

                return FieldType.Unknown;
            }

            public NbtEnabledClassWrapper(Type type)
            {
                var fields = type.GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);

                foreach(var field in fields)
                {
                    var nbtAttributes = field.GetCustomAttribute<NbtEntryAttribute>();
                    if (nbtAttributes != null)
                    {
                        var fieldType = DetermineType(field.FieldType);
                        if (fieldType != FieldType.Unknown)
                            _fieldList.Add((field, fieldType, nbtAttributes));
                    }
                }

                var properties = type.GetProperties(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var property in properties)
                {
                    var nbtAttributes = property.GetCustomAttribute<NbtEntryAttribute>();
                    if (nbtAttributes != null)
                    {
                        var propertyType = DetermineType(property.PropertyType);
                        if (propertyType != FieldType.Unknown)
                            _propertyList.Add((property, propertyType, nbtAttributes));
                    }
                }
            }

            private static object GetValue(NbtTag tag, FieldType targetType, Type fieldType = null)
            {
                try
                {
                    switch (targetType)
                    {
                        case FieldType.Byte:
                            return tag.ByteValue;
                        case FieldType.Short:
                            return tag.ShortValue;
                        case FieldType.Int:
                            return tag.IntValue;
                        case FieldType.Long:
                            return tag.LongValue;
                        case FieldType.Float:
                            return tag.FloatValue;
                        case FieldType.Double:
                            return tag.DoubleValue;
                        case FieldType.ByteArray:
                            return tag.ByteArrayValue;
                        case FieldType.String:
                            return tag.StringValue;
                        case FieldType.IntArray:
                            return tag.IntValue;
                        case FieldType.LongArray:
                            return tag.LongArrayValue;
                        case FieldType.Compound:
                            if (fieldType == null || !(tag is NbtCompound compound))
                                return null;

                            var newObj = (INbtMapperCapable)Activator.CreateInstance(fieldType, true);
                            ReadFromNbt(newObj, compound);
                            return newObj;
                        default:
                            throw new NotSupportedException();
                    }
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }

            public void ReadFromNbtCompound(INbtMapperCapable target, NbtCompound compound)
            {
                foreach(var (field, type, attr) in _fieldList)
                {
                    var result = compound.TryGet(attr.TagName ?? field.Name, out var tag);
                    if (!result)
                    {
                        if (!attr.Optional)
                        {
                            ExceptionHelper.ThrowParseMissingError(field.Name, ParseErrorLevel.Exception);
                        }

                        continue;
                    }

                    var resultObj = GetValue(tag, type, field.FieldType);
                    if (resultObj == null)
                    {
                        if (attr.Optional)
                        {
                            ExceptionHelper.ThrowParseError($"Type mismatch parsing optional {field.Name} in {target.GetType().FullName}", ParseErrorLevel.Information);
                        }
                        else
                        {
                            ExceptionHelper.ThrowParseError($"Type mismatch parsing required {field.Name} in {target.GetType().FullName}", ParseErrorLevel.Exception);
                        }

                        continue;
                    }

                    field.SetValue(target, resultObj);
                }

                foreach (var (property, type, attr) in _propertyList)
                {
                    var result = compound.TryGet(attr.TagName ?? property.Name, out var tag);
                    if (!result)
                    {
                        if (!attr.Optional)
                        {
                            ExceptionHelper.ThrowParseMissingError(property.Name, ParseErrorLevel.Exception);
                        }

                        continue;
                    }

                    var resultObj = GetValue(tag, type, property.PropertyType);
                    if (resultObj == null)
                    {
                        if (attr.Optional)
                        {
                            ExceptionHelper.ThrowParseError($"Type mismatch parsing optional {property.Name} in {target.GetType().FullName}", ParseErrorLevel.Information);
                        }
                        else
                        {
                            ExceptionHelper.ThrowParseError($"Type mismatch parsing required {property.Name} in {target.GetType().FullName}", ParseErrorLevel.Exception);
                        }

                        continue;
                    }

                    property.SetValue(target, resultObj, 
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);
                }
            }
        }
    }
}
