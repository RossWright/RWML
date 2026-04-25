using System.ComponentModel;
using System.Reflection;

namespace RossWright;

/// <summary>
/// Extension methods for reflection types: <see cref="MemberInfo"/>, <see cref="Type"/>, <see cref="FieldInfo"/>, and <see cref="PropertyInfo"/>.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Gets the value of a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/> member from an object.
    /// </summary>
    /// <param name="memberInfo">The field or property to read. Must be a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.</param>
    /// <param name="obj">The object instance to read from.</param>
    /// <returns>The current value of the member on <paramref name="obj"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="memberInfo"/> is neither a field nor a property.</exception>
    public static object? GetValue(this MemberInfo memberInfo, object obj)
    {
        if (memberInfo is FieldInfo) return ((FieldInfo)memberInfo).GetValue(obj);
        if (memberInfo is PropertyInfo) return ((PropertyInfo)memberInfo).GetValue(obj);
        throw new NotSupportedException($"{memberInfo.MemberType} is not supported by {nameof(GetValue)}");
    }

    /// <summary>
    /// Sets the value of a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/> member on an object.
    /// </summary>
    /// <param name="memberInfo">The field or property to write. Must be a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.</param>
    /// <param name="obj">The object instance to write to.</param>
    /// <param name="value">The value to assign.</param>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="memberInfo"/> is neither a field nor a property.</exception>
    public static void SetValue(this MemberInfo memberInfo, object? obj, object? value)
    {
        if (memberInfo is FieldInfo) ((FieldInfo)memberInfo).SetValue(obj, value);
        else if (memberInfo is PropertyInfo) ((PropertyInfo)memberInfo).SetValue(obj, value);
        else throw new NotSupportedException($"{memberInfo.MemberType} is not supported by {nameof(SetValue)}");
    }

    /// <summary>
    /// Returns <see langword="true"/> if the type is decorated with the specified attribute.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="attributeType">The attribute type to search for.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute(this Type type, Type attributeType, bool inherit = true) =>
        type.GetCustomAttribute(attributeType, inherit) != null;

    /// <summary>
    /// Returns <see langword="true"/> if the type is decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to search for.</typeparam>
    /// <param name="type">The type to inspect.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute<TAttribute>(this Type type, bool inherit = true) where TAttribute : Attribute =>
        type.GetCustomAttribute<TAttribute>(inherit) != null;

    /// <summary>
    /// Returns <see langword="true"/> if the field is decorated with the specified attribute.
    /// </summary>
    /// <param name="fieldInfo">The field to inspect.</param>
    /// <param name="attributeType">The attribute type to search for.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute(this FieldInfo fieldInfo, Type attributeType, bool inherit = true) =>
        fieldInfo.GetCustomAttribute(attributeType, inherit) != null;

    /// <summary>
    /// Returns <see langword="true"/> if the field is decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to search for.</typeparam>
    /// <param name="fieldInfo">The field to inspect.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute<TAttribute>(this FieldInfo fieldInfo, bool inherit = true) where TAttribute : Attribute =>
        fieldInfo.GetCustomAttribute<TAttribute>(inherit) != null;

    /// <summary>
    /// Returns <see langword="true"/> if the property is decorated with the specified attribute.
    /// </summary>
    /// <param name="propInfo">The property to inspect.</param>
    /// <param name="attributeType">The attribute type to search for.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute(this PropertyInfo propInfo, Type attributeType, bool inherit = true) =>
        propInfo.GetCustomAttribute(attributeType, inherit) != null;

    /// <summary>
    /// Returns <see langword="true"/> if the property is decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to search for.</typeparam>
    /// <param name="propInfo">The property to inspect.</param>
    /// <param name="inherit">Whether to search the inheritance chain. Defaults to <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool HasAttribute<TAttribute>(this PropertyInfo propInfo, bool inherit = true) where TAttribute : Attribute =>
        propInfo.GetCustomAttribute<TAttribute>(inherit) != null;

    /// <summary>
    /// Converts a string to an instance of the specified type using <see cref="System.ComponentModel.TypeDescriptor"/>.
    /// </summary>
    /// <param name="type">The target type.</param>
    /// <param name="valueStr">The string representation to parse.</param>
    /// <returns>The converted value, or <see langword="null"/> if conversion returns null.</returns>
    public static object? Parse(this Type type, string valueStr) =>
        TypeDescriptor.GetConverter(type).ConvertFromString(valueStr);
    
    /// <summary>
    /// Attempts to convert a value to the specified type. Returns the default for value types
    /// when <paramref name="value"/> is <see langword="null"/>, or the original value when the type already matches.
    /// </summary>
    /// <param name="type">The target type.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or a type-appropriate default for null inputs.</returns>
    public static object? TryConvert(this Type type, object? value)
    {
        if (value == null)
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        if (value.GetType() == type)
            return value;
        if (value is string valueStr)
            return type.Parse(valueStr);
        return value;
    }

    /// <summary>
    /// Gets the field type, property type, or method return type from a <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The member to inspect. Must be a field, property, or method.</param>
    /// <returns>The return or value type of the member.</returns>
    /// <exception cref="NotSupportedException">Thrown for unsupported member kinds.</exception>
    public static Type? GetReturnType(this MemberInfo memberInfo) => memberInfo is FieldInfo
        ? ((FieldInfo)memberInfo).FieldType
        : (memberInfo is PropertyInfo
            ? ((PropertyInfo)memberInfo).PropertyType
            : memberInfo is MethodInfo
                ? ((MethodInfo)memberInfo).ReturnType
                : throw new NotSupportedException($"{memberInfo.MemberType} is not supported by {nameof(MethodInfo.ReturnType)}"));

    /// <summary>
    /// Returns <see langword="true"/> if the type can be converted from a <see cref="string"/> using <see cref="System.ComponentModel.TypeDescriptor"/>.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <returns><see langword="true"/> for simple types such as primitives and enums; otherwise <see langword="false"/>.</returns>
    public static bool IsSimpleType(this Type type) => TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

    /// <summary>
    /// Returns <see langword="true"/> if the type is neither abstract nor an interface.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <returns><see langword="true"/> for concrete, instantiable types; otherwise <see langword="false"/>.</returns>
    public static bool IsConcrete(this Type type) => !type.IsAbstract && !type.IsInterface;

    /// <summary>
    /// Returns a human-readable generic type name, such as <c>"List&lt;string&gt;"</c>.
    /// </summary>
    /// <param name="type">The type whose name to format.</param>
    /// <returns>The type name with fully resolved generic argument names.</returns>
    public static string GetFullGenericName(this Type type) => !type.IsGenericType || type.IsGenericTypeDefinition
        ? !type.IsGenericTypeDefinition ? type.Name : type.Name.Remove(type.Name.IndexOf('`'))
        : $"{type.GetGenericTypeDefinition().GetFullGenericName()}<{string.Join(',', type.GetGenericArguments().Select(_ => _.GetFullGenericName()))}>";
}
