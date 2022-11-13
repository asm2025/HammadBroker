using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace HammadBroker.Model;

public class SimpleRouteData : Dictionary<string, object>
{
	/// <inheritdoc />
	public SimpleRouteData()
		: base(StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <inheritdoc />
	public SimpleRouteData([NotNull] IDictionary<string, object> dictionary)
		: base(dictionary, StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <inheritdoc />
	public SimpleRouteData([NotNull] IEnumerable<KeyValuePair<string, object>> collection)
		: base(collection, StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <inheritdoc />
	public SimpleRouteData(int capacity)
		: base(capacity, StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <inheritdoc />
	protected SimpleRouteData([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public string Area
	{
		get =>
			TryGetValue(nameof(Area), out object value)
				? (string)value
				: null;
		set => this[nameof(Area)] = value;
	}

	public string Controller
	{
		get =>
			TryGetValue(nameof(Controller), out object value)
				? (string)value
				: null;
		set => this[nameof(Controller)] = value;
	}

	public string Action
	{
		get =>
			TryGetValue(nameof(Action), out object value)
				? (string)value
				: null;
		set => this[nameof(Action)] = value;
	}
}