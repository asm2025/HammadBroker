using System.Collections.Generic;

namespace HammadBroker.Model;

public class MarketingLinks
{
	public bool HasLinks => Links is { Count: > 0 };
	public ICollection<MarketingLink> Links { get; set; }
}