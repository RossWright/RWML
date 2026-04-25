using System.ComponentModel;
using System.Reflection;

namespace RossWright;

/// <summary>
/// Extension methods for shallow-copying objects within the same type or across
/// different types. Members are matched by name (case-sensitive). Unmatched members
/// are left at their default values. Reference-type members are <em>not</em>
/// deep-copied; nested objects share the same instance as the original.
/// </summary>
public static class CloneAsExtensions
{
    /// <summary>
    /// Creates a shallow copy of <paramref name="copyFromObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of object; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromObject">The original object to copy.</param>
    /// <param name="init">
    /// Optional callback invoked on the new instance after all members are copied.
    /// Use this to set computed or derived fields.
    /// </param>
    /// <returns>A new <typeparamref name="T"/> with the same member values.</returns>
    /// <example>
    /// <code>
    /// var original = order.Clone();
    /// // ... edit order ...
    /// if (order.HasChangedFrom(original)) await SaveAsync(order);
    /// </code>
    /// </example>
    public static T Clone<T>(this T copyFromObject, Action<T>? init = null)
        where T : new() =>
        copyFromObject!.CloneAs<T>(init);

    /// <summary>
    /// Maps a collection of objects to a new array of <typeparamref name="T"/>,
    /// copying matching members from each element.
    /// </summary>
    /// <typeparam name="T">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollection">The source collection.</param>
    /// <returns>An array of mapped <typeparamref name="T"/> instances.</returns>
    public static T[] CloneAs<T>(this IEnumerable<object> copyFromCollection) 
        where T : new() =>
        copyFromCollection.Select(_ => _.CloneAs<T>()).ToArray();

    /// <summary>
    /// Maps a collection of objects to a new array of <typeparamref name="T"/>,
    /// invoking <paramref name="init"/> on each mapped instance.
    /// </summary>
    /// <typeparam name="T">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollection">The source collection.</param>
    /// <param name="init">A callback invoked on each new <typeparamref name="T"/> after mapping.</param>
    /// <returns>An array of mapped <typeparamref name="T"/> instances.</returns>
    public static T[] CloneAs<T>(this IEnumerable<object> copyFromCollection, Action<T> init)
        where T : new() =>
        copyFromCollection.Select(source => source.CloneAs<T>(clone => init(clone))).ToArray();

    /// <summary>
    /// Maps a collection of objects to a new array of <typeparamref name="T"/>,
    /// passing each source and its mapped clone to <paramref name="init"/>.
    /// </summary>
    /// <typeparam name="T">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollection">The source collection.</param>
    /// <param name="init">
    /// A callback receiving the source object and its mapped clone. Use this to
    /// populate members that require access to the original source.
    /// </param>
    /// <returns>An array of mapped <typeparamref name="T"/> instances.</returns>
    public static T[] CloneAs<T>(this IEnumerable<object> copyFromCollection, Action<object, T> init) 
        where T : new() =>
        copyFromCollection.Select(source => source.CloneAs<T>(clone => init(source, clone))).ToArray();

    /// <summary>
    /// Strongly-typed collection mapping from <typeparamref name="DBO"/> to <typeparamref name="DTO"/>.
    /// </summary>
    /// <typeparam name="DBO">The source type.</typeparam>
    /// <typeparam name="DTO">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollection">The source collection.</param>
    /// <param name="init">
    /// Optional callback receiving each source and its mapped clone for post-mapping initialization.
    /// </param>
    /// <returns>An array of mapped <typeparamref name="DTO"/> instances.</returns>
    /// <example>
    /// <code>
    /// var dtos = users.CloneAs&lt;User, UserDto&gt;((src, dto) =&gt;
    ///     dto.DisplayName = $"{src.First} {src.Last}");
    /// </code>
    /// </example>
    public static DTO[] CloneAs<DBO, DTO>(this IEnumerable<DBO> copyFromCollection, Action<DBO, DTO>? init = null)
        where DBO : notnull 
        where DTO : new() =>
        copyFromCollection.Select(source => init != null 
            ? source.CloneAs<DTO>(clone => init(source, clone))
            : source.CloneAs<DTO>()
        ).ToArray();

    /// <summary>
    /// Maps a single object to a new instance of <typeparamref name="T"/>,
    /// copying all public, writable members that share a matching name.
    /// </summary>
    /// <typeparam name="T">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromObject">The source object.</param>
    /// <param name="init">Optional callback invoked on the new instance after all members are copied.</param>
    /// <returns>A new <typeparamref name="T"/> with mapped member values.</returns>
    /// <example>
    /// <code>
    /// var dto = dbUser.CloneAs&lt;UserDto&gt;();
    /// </code>
    /// </example>
    public static T CloneAs<T>(this object copyFromObject, Action<T>? init = null) 
        where T : new()
    {
        var clone = new T();
        copyFromObject.CopyTo(clone);
        if (init != null) init(clone);
        return clone!;
    }

    /// <summary>
    /// Copies all public, writable members from <paramref name="copyFromObject"/> into an
    /// existing <paramref name="copyToObject"/>. Members decorated with <see cref="IgnoreAttribute"/>
    /// are skipped.
    /// </summary>
    /// <param name="copyFromObject">The source object to read from.</param>
    /// <param name="copyToObject">The target object to write into.</param>
    public static void CopyTo(this object copyFromObject, object copyToObject)
    {
        RunThroughDataMembers(copyFromObject, copyToObject, (destMember, srcValue) =>
        {
            destMember.SetValue(copyToObject, srcValue);
            return null;
        });
    }

    /// <summary>
    /// Returns <see langword="true"/> if any public property or field value on
    /// <paramref name="workingObject"/> differs from the corresponding value on
    /// <paramref name="originalObject"/>. Useful for dirty-checking before saving.
    /// </summary>
    /// <typeparam name="T">The type of the objects being compared.</typeparam>
    /// <param name="workingObject">The potentially modified object.</param>
    /// <param name="originalObject">The original baseline object.</param>
    /// <returns>
    /// <see langword="true"/> if any member value has changed;
    /// <see langword="false"/> if all values are equal.
    /// </returns>
    /// <example>
    /// <code>
    /// if (editedUser.HasChangedFrom(originalUser))
    ///     await SaveAsync(editedUser);
    /// </code>
    /// </example>
    public static bool HasChangedFrom<T>(this T? workingObject, T? originalObject)
    {
        if (originalObject is null != workingObject is null) return true; // if one is null, it changed
        if (originalObject is null && workingObject is null) return false; // if both are null, it didn't

        return RunThroughDataMembers(originalObject!, workingObject!, (destMember, srcValue) =>
        {
            var working = destMember.GetValue(workingObject!);
            if (srcValue is null != working is null ||
                (srcValue is not null && !srcValue.Equals(working)))
            {
                return true;
            }
            return null;
        }) == true;
    }

    private static bool? RunThroughDataMembers(object copyFromObject, object copyToObject, Func<MemberInfo, object?, bool?> action)
    {
        if (copyFromObject == null) 
            throw new NullReferenceException("copyFromObject reference not set to an instance of an object.");
        if (copyToObject == null)
            throw new NullReferenceException("copyToObject reference not set to an instance of an object.");

        // Get all public writable fields and properties on the copyFrom object without the ignore attribute
        var destMembers = copyToObject
            .GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(_ => !_.IsInitOnly)
            .Select(_ => (MemberInfo)_)
            .Concat(copyToObject
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(_ => _.SetMethod?.IsPublic ?? false)
                .Select(_ => (MemberInfo)_))
            .Where(destField => destField.GetCustomAttribute<IgnoreAttribute>() == null)
            .ToList();
        var destMembersByName = destMembers.ToDictionary(_ => _.Name);

        //Add any Aka Attribute aliases to the found members to the search
        foreach (MemberInfo destMember in destMembers)
        {
            var aka = destMember.GetCustomAttribute<AkaAttribute>();
            if (aka != null)
            {
                if (destMembersByName.TryGetValue(aka.Alias, out var conflict))
                    throw new MetalCoreException($"{aka.Alias} cannot be an alias of {conflict.Name} declared on {conflict.DeclaringType}");
                destMembersByName.Add(aka.Alias, destMember);
            }
        }

        // for each readable public property and field on the copyTo object without the ignore attribute
        foreach (var sourceMember in copyFromObject.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .Concat(copyFromObject
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(_ => _.GetMethod?.IsPublic == true)
                .Cast<MemberInfo>())
            .Where(destField => destField.GetCustomAttribute<IgnoreAttribute>() == null))
        {
            // if no name match is found, see if there is an alias match, otherwise skip it
            if (!destMembersByName.TryGetValue(sourceMember.Name, out var destMember))
            {
                var aka = sourceMember.GetCustomAttribute<AkaAttribute>();
                if (aka == null || !destMembersByName.TryGetValue(aka.Alias, out destMember)) continue;
            }

            var realDestType = destMember.GetReturnType();
            var realSrcType = sourceMember.GetReturnType();
            if (realDestType == null || realSrcType == null) continue;

            if (realSrcType.IsGenericType && realSrcType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                realSrcType = realSrcType.GetGenericArguments()[0];
            }

            bool destTypeIsNullable = !realDestType.IsValueType ||
                (realDestType.IsGenericType && realDestType.GetGenericTypeDefinition() == typeof(Nullable<>));
            if (realDestType.IsGenericType && realDestType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                realDestType = realDestType.GetGenericArguments()[0];
            }

            var value = sourceMember.GetValue(copyFromObject);
            if (value == null)
            {
                if (!destTypeIsNullable) continue;
            }

            // If the source type is not assignable to the dest type, then check for convertability
            else if (! realDestType.IsAssignableFrom(realSrcType))
            {
                var srcConverter = TypeDescriptor.GetConverter(realSrcType);
                if (!srcConverter.CanConvertTo(realDestType)) continue;
                value = srcConverter.ConvertTo(value, realDestType);
            }

            var result = action(destMember, value);
            if (result.HasValue) return result.Value;
        }
        return null;
    }

    /// <summary>
    /// Awaits a task returning <typeparamref name="DBO"/> and maps the result to
    /// <typeparamref name="DTO"/>. Returns <see langword="default"/> when the source is
    /// <see langword="null"/>.
    /// </summary>
    /// <typeparam name="DBO">The source type.</typeparam>
    /// <typeparam name="DTO">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="sourceTask">The task whose result will be mapped.</param>
    /// <param name="init">Optional callback for post-mapping initialization.</param>
    /// <returns>A mapped <typeparamref name="DTO"/>, or <see langword="default"/> if the source was <see langword="null"/>.</returns>
    public static async Task<DTO?> ThenCloneAs<DBO, DTO>(this Task<DBO?> sourceTask, Action<DBO, DTO>? init = null)
        where DBO : notnull
        where DTO : new()
    {
        var source = await sourceTask;
        if (source == null) return default;
        var clone = source.CloneAs<DTO>();
        if (init != null) init(source, clone);
        return clone;
    }

    /// <summary>
    /// Awaits a task returning a <see cref="List{DBO}"/> and maps each element to
    /// <typeparamref name="DTO"/>, returning a <see cref="List{DTO}"/>.
    /// </summary>
    /// <typeparam name="DBO">The source type.</typeparam>
    /// <typeparam name="DTO">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollectionTask">The task whose result collection will be mapped.</param>
    /// <param name="init">Optional per-element callback for post-mapping initialization.</param>
    /// <returns>A list of mapped <typeparamref name="DTO"/> instances.</returns>
    public static async Task<List<DTO>> ThenCloneAs<DBO, DTO>(this Task<List<DBO>> copyFromCollectionTask, Action<DBO, DTO>? init = null)
        where DBO : notnull
        where DTO : new() =>
        init == null ? (await copyFromCollectionTask).Select(_ => _.CloneAs<DTO>()).ToList()
            : (await copyFromCollectionTask).Select(source => source.CloneAs<DTO>(clone => init(source, clone))).ToList();

    /// <summary>
    /// Awaits a task returning a <typeparamref name="DBO"/> array and maps each element to
    /// <typeparamref name="DTO"/>.
    /// </summary>
    /// <typeparam name="DBO">The source type.</typeparam>
    /// <typeparam name="DTO">The destination type; must have a public parameterless constructor.</typeparam>
    /// <param name="copyFromCollectionTask">The task whose result array will be mapped.</param>
    /// <param name="init">Optional per-element callback for post-mapping initialization.</param>
    /// <returns>An array of mapped <typeparamref name="DTO"/> instances.</returns>
    public static async Task<DTO[]> ThenCloneAs<DBO, DTO>(this Task<DBO[]> copyFromCollectionTask, Action<DBO, DTO>? init = null)
        where DBO : notnull
        where DTO : new() =>
        init == null ? (await copyFromCollectionTask).Select(_ => _.CloneAs<DTO>()).ToArray()
            : (await copyFromCollectionTask).Select(source => source.CloneAs<DTO>(clone => init(source, clone))).ToArray();
}
