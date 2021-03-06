﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord.Collections
{
	public sealed class Users : AsyncCollection<User>
	{
		internal Users(DiscordClient client, object writerLock)
			: base(client, writerLock) { }

		internal User GetOrAdd(string id) => GetOrAdd(id, () => new User(_client, id));
		internal new User TryRemove(string id) => base.TryRemove(id);

		protected override void OnCreated(User item) { }
		protected override void OnRemoved(User item) { }

		internal User this[string id] => Get(id);

		internal IEnumerable<User> Find(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			if (name.StartsWith("@"))
			{
				string name2 = name.Substring(1);
				return this.Where(x =>
					string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Name, name2, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				return this.Where(x =>
					string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
			}
		}
	}
}
