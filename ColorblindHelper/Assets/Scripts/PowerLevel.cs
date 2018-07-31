using System.ComponentModel;

public enum PowerLevel
{
	[Description("streamer")] Streamer,
	[Description("streamer-only")] StreamerOnly,
	[Description("superuser")] Superuser,
	[Description("superuser-only")] SuperUserOnly,
	[Description("admin")] Admin,
	[Description("admin-only")] AdminOnly,
	[Description("mod")] Mod,
	[Description("mod-only")] ModOnly,
	[Description("defuser")] Defuser,
	[Description("defuser-only")] DefuserOnly

}