﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.WebSockets
{
	public class WebSocketMessage
	{
		[JsonProperty("op")]
		public int Operation;
		[JsonProperty("d")]
		public object Payload;
		[JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
		public string Type;
		[JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
		public int? Sequence;
	}
	internal abstract class WebSocketMessage<T> : WebSocketMessage
		where T : new()
	{
		public WebSocketMessage() { Payload = new T(); }
		public WebSocketMessage(int op) { Operation = op; Payload = new T(); }
		public WebSocketMessage(int op, T payload) { Operation = op; Payload = payload; }

		[JsonIgnore]
		public new T Payload
		{
			get { if (base.Payload is JToken) { base.Payload = (base.Payload as JToken).ToObject<T>(); } return (T)base.Payload; }
			set { base.Payload = value; }
		}
	}
}
