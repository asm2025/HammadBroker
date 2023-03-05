using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace HammadBroker.Model.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class RequireAnyOf : ValidationAttribute
{
	private const int REQUIRED_MAX = 10;

	private static readonly ConcurrentDictionary<string, MemberInfo[]> __membersCache = new ConcurrentDictionary<string, MemberInfo[]>();
	private static readonly ConcurrentDictionary<string, string> __membersNames = new ConcurrentDictionary<string, string>();

	private readonly ISet<string> _targets;
	private readonly string _targetsKey;

	public RequireAnyOf([NotNull] params string[] targets)
	{
		_targets = new HashSet<string>(targets.SkipNullOrEmptyTrim().Take(REQUIRED_MAX), StringComparer.Ordinal);
		if (_targets.Count == 0) throw new ArgumentException("Property is missing or contains invalid entries.", nameof(targets));
		_targetsKey = string.Join("_", _targets.OrderBy(e => e));
	}

	/// <inheritdoc />
	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		if (value is not null) return ValidationResult.Success;

		object instance = validationContext.ObjectInstance;
		Type type = instance.GetType();
		string key = string.Join("_", type.AssemblyQualifiedName, type.FullName, _targetsKey);
		MemberInfo[] members = __membersCache.GetOrAdd(key, _ => instance.GetType()
																												.GetMembers()
																												.Where(e => e.MemberType is MemberTypes.Property or MemberTypes.Field && _targets.Contains(e.Name))
																												.ToArray());
		bool anyValue = members.Any(e =>
		{
			if (e.MemberType == MemberTypes.Field)
			{
				FieldInfo field = (FieldInfo)e;
				return field.GetValue(instance) is not null;
			}

			PropertyInfo property = (PropertyInfo)e;
			return property.GetValue(instance) is not null;
		});
		if (anyValue) return ValidationResult.Success;

		string targetsNames = __membersNames.GetOrAdd(key, _ => string.Join(", ", members.Select(e => e.GetDisplayName(e.Name))));
		string message = FormatErrorMessage(targetsNames);
		return new ValidationResult(message);
	}
}