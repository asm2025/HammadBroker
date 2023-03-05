using System;

namespace HammadBroker.Model;

public static class Constants
{
	public const string ApplicationName = "AqaratHammad";

	public static readonly TimeSpan MigrationTimeout = TimeSpan.FromMinutes(2);

	public static class Configuration
	{
		public const string CspTrustedDomainsKey = "CspTrustedDomains";
		public const string CspTrustedConnectDomainsKey = "CspTrustedConnectDomains";
	}

	public static class Authorization
	{
		public const string AdministrationPolicy = "RequireAdministratorRole";
		public const string MemberPolicy = "RequireMemberRole";

#if DEBUG
		public const string AdministratorId = "admin@localhost";
#else
		public const string AdministratorId = "admin@aqarathammad.com";
#endif
	}

	public static class Images
	{
		public const string Extensions = ".emf,.jfif,.jpg,.jpeg,.png,.svg,.wmf";
		public const long FileSizeMax = 0xA00000;
		public const int DimensionMax = 386;
		public const string AssetImagesPath = "data/assets/";
		public const int MaxImagesUpload = 30;
	}

	public static class Buildings
	{
		public const int IdentifierLength = 10;
	}
}
