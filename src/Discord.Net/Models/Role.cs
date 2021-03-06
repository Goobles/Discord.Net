﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Discord
{
	public sealed class Role
	{
		private readonly DiscordClient _client;

		/// <summary> Returns the unique identifier for this role. </summary>
		public string Id { get; }
		/// <summary> Returns the name of this role. </summary>
		public string Name { get; internal set; }

		/// <summary> Returns the the permissions contained by this role. </summary>
		public PackedPermissions Permissions { get; }

		/// <summary> Returns the id of the server this role is a member of. </summary>
		public string ServerId { get; }
		/// <summary> Returns the server this role is a member of. </summary>
		[JsonIgnore]
		public Server Server => _client.Servers[ServerId];

		/// <summary> Returns true if this is the role representing all users in a server. </summary>
		public bool IsEveryone { get; }
		/// <summary> Returns a list of the ids of all members in this role. </summary>
		public IEnumerable<string> MemberIds { get { return IsEveryone ? Server.UserIds : Server.Members.Where(x => x.RoleIds.Contains(Id)).Select(x => x.UserId); } }
		/// <summary> Returns a list of all members in this role. </summary>
		public IEnumerable<Member> Members { get { return IsEveryone ? Server.Members : Server.Members.Where(x => x.RoleIds.Contains(Id)); } }

		internal Role(DiscordClient client, string id, string serverId, bool isEveryone)
		{
			_client = client;
			Id = id;
			ServerId = serverId;
			Permissions = new PackedPermissions(true);
			IsEveryone = isEveryone;
        }

		internal void Update(API.RoleInfo model)
		{
			Name = model.Name;
			Permissions.RawValue = (uint)model.Permissions;

			foreach (var member in Members)
				member.UpdatePermissions();
		}

		public override string ToString() => Name;
	}
}
