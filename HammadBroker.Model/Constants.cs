using System;

namespace HammadBroker.Model;

public static class Constants
{
	public const string ApplicationName = "HammadBroker";

	public static readonly TimeSpan MigrationTimeout = TimeSpan.FromMinutes(2);

	public static class Configuration
	{
		public const string CspTrustedDomainsKey = "CspTrustedDomains";
		public const string CspTrustedConnectDomainsKey = "CspTrustedConnectDomains";
	}

	public static class Authorization
	{
		public const string SystemPolicy = "RequireSystemRole";
		public const string AdministrationPolicy = "RequireAdministratorRole";
		public const string MemberPolicy = "RequireMemberRole";

#if DEBUG
		public const string SystemId = "super@localhost";
		public const string AdministratorId = "admin@localhost";
#else
		public const string SystemId = "super@hammadBroker.com";
		public const string AdministratorId = "admin@hammadBroker.com";
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

	public static class Building
	{
		public const int IdentifierLength = 10;
	}
}
