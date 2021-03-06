﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Discord.WebSockets.Data
{
    internal partial class DataWebSocket : WebSocket
    {
		private int _lastSeq;

		public string SessionId => _sessionId;
		private string _sessionId;

		public DataWebSocket(DiscordSimpleClient client)
			: base(client)
		{
		}

        public async Task Login(string token)
		{
			await Connect().ConfigureAwait(false);
			
			LoginCommand msg = new LoginCommand();
			msg.Payload.Token = token;
			msg.Payload.Properties["$device"] = "Discord.Net";
			QueueMessage(msg);
        }
		private async Task Redirect(string server)
		{
			await DisconnectInternal(isUnexpected: false).ConfigureAwait(false);
			await Connect().ConfigureAwait(false);

			var resumeMsg = new ResumeCommand();
			resumeMsg.Payload.SessionId = _sessionId;
			resumeMsg.Payload.Sequence = _lastSeq;
			QueueMessage(resumeMsg);
		}
		public async Task Reconnect(string token)
		{
			try
			{
				var cancelToken = ParentCancelToken.Value;
				await Task.Delay(_client.Config.ReconnectDelay, cancelToken).ConfigureAwait(false);
				while (!cancelToken.IsCancellationRequested)
				{
					try
					{
						await Login(token).ConfigureAwait(false);
						break;
					}
					catch (OperationCanceledException) { throw; }
					catch (Exception ex)
					{
						RaiseOnLog(LogMessageSeverity.Error, $"Reconnect failed: {ex.GetBaseException().Message}");
						//Net is down? We can keep trying to reconnect until the user runs Disconnect()
						await Task.Delay(_client.Config.FailedReconnectDelay, cancelToken).ConfigureAwait(false);
					}
				}
			}
			catch (OperationCanceledException) { }
		}

		protected override async Task ProcessMessage(string json)
		{
			var msg = JsonConvert.DeserializeObject<WebSocketMessage>(json);
			if (msg.Sequence.HasValue)
				_lastSeq = msg.Sequence.Value;
			
			switch (msg.Operation)
			{
				case 0:
					{
						JToken token = msg.Payload as JToken;
						if (msg.Type == "READY")
						{
							var payload = token.ToObject<ReadyEvent>();
							_sessionId = payload.SessionId;
							_heartbeatInterval = payload.HeartbeatInterval;
							QueueMessage(new UpdateStatusCommand());
						}
						else if (msg.Type == "RESUMED")
						{
							var payload = token.ToObject<ResumedEvent>();
							_heartbeatInterval = payload.HeartbeatInterval;
							QueueMessage(new UpdateStatusCommand());
						}
						RaiseReceivedEvent(msg.Type, token);
						if (msg.Type == "READY" || msg.Type == "RESUMED")
							CompleteConnect();
					}
					break;
				case 7: //Redirect
					{
						var payload = (msg.Payload as JToken).ToObject<RedirectEvent>();
						Host = payload.Url;
						if (_logLevel >= LogMessageSeverity.Info)
							RaiseOnLog(LogMessageSeverity.Info, "Redirected to " + payload.Url);
						await Redirect(payload.Url);
					}
					break;
				default:
					if (_logLevel >= LogMessageSeverity.Warning)
						RaiseOnLog(LogMessageSeverity.Warning, $"Unknown Opcode: {msg.Operation}");
					break;
			}
		}

		protected override object GetKeepAlive()
		{
			return new KeepAliveCommand();
		}

		public void SendJoinVoice(string serverId, string channelId)
		{
			var joinVoice = new JoinVoiceCommand();
			joinVoice.Payload.ServerId = serverId;
			joinVoice.Payload.ChannelId = channelId;
			QueueMessage(joinVoice);
		}
		public void SendLeaveVoice(string serverId)
		{
			var leaveVoice = new JoinVoiceCommand();
			leaveVoice.Payload.ServerId = serverId;
			QueueMessage(leaveVoice);
		}
	}
}
