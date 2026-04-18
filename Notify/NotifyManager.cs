using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModCore.Events;
using ModCore.Helpers;
using ModCore.Models;
using ModCore.Services;

namespace VArenaNotify
{
	public static class NotifyManager
	{
		private static List<Timer> Timers = new ();

		public static void Initialize ()
		{
			GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
			GameEvents.OnPlayerConnected += HandleOnPlayerWelcomed;
			GameEvents.OnPlayerFirstSpawn += HandleOnNewPlayer;
			GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;

			SetupAutoAnnouncements();
		}

		public static void Dispose ()
		{
			GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
			GameEvents.OnPlayerConnected -= HandleOnPlayerWelcomed;
			GameEvents.OnPlayerFirstSpawn -= HandleOnNewPlayer;
			GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;

			foreach (var timer in Timers)
			{
				timer?.Dispose();
			}

			Timers.Clear();
		}

		public static void HandleOnPlayerWelcomed (Player player)
		{
			SendWelcomeMessage(player);
		}

		public static void HandleOnNewPlayer (Player player)
		{
			SendPlayerNewAnnouncement(player);
		}

		public static void HandleOnPlayerConnected (Player player)
		{
			SendPlayerConnectedAnnouncement(player);
		}

		public static void HandleOnPlayerDisconnected (Player player)
		{
			SendPlayerDisconnectedAnnouncement(player);
		}

		private static float totalInterval;

		public static void SetupAutoAnnouncements ()
		{
			if (VArenaNotifyConfig.Config.autoAnnouncementNotifications.Count == 0)
				return;
			
			totalInterval = VArenaNotifyConfig.Config.autoAnnouncementNotifications.Max(x => x.delayInSeconds);
			foreach (AutoAnnouncement autoAnnouncement in VArenaNotifyConfig.Config.autoAnnouncementNotifications)
			{
				AutoAnnouncement tmp = autoAnnouncement;
				Timer timer = ActionScheduler.RunActionOnceAfterDelay(() => PrepareAutoAnnouncementWithInterval(tmp),
					tmp.delayInSeconds);
				Timers.Add(timer);
			}
		}

		public static void PrepareAutoAnnouncementWithInterval (AutoAnnouncement _announcement)
		{
			SendAutoAnnouncement(_announcement);
			Timer timer =
				ActionScheduler.RunActionEveryInterval(() => SendAutoAnnouncement(_announcement), totalInterval);
			Timers.Add(timer);
		}

		public static void SendWelcomeMessage (Player welcomedPlayer)
		{
			if (VArenaNotifyConfig.Config.welcomePlayerNotification.Count == 0)
				return;
            
			foreach (string message in VArenaNotifyConfig.Config.welcomePlayerNotification)
				welcomedPlayer.ReceiveMessage(message);
		}

		public static void SendAutoAnnouncement (AutoAnnouncement _autoAnnouncement)
		{
			foreach (string message in _autoAnnouncement.announcement)
				Helper.SendSystemMessageToAllClients(message);
		}

		public static void SendAutoAnnouncement (int _index) =>
			SendAutoAnnouncement(VArenaNotifyConfig.Config.autoAnnouncementNotifications[_index]);

		public static void SendPlayerDisconnectedAnnouncement (Player disconnectedPlayer)
		{
			if (VArenaNotifyConfig.Config.playerDisconnectedNotification != "")
				Helper.SendSystemMessageToAllClients(
					$"{disconnectedPlayer.Name.ToString().Colorify(ExtendedColor.ClanNameColor)} " +
					VArenaNotifyConfig.Config.playerDisconnectedNotification);
		}

		public static void SendPlayerConnectedAnnouncement (Player connectedPlayer)
		{
			if (VArenaNotifyConfig.Config.playerConnectedNotification != "")
				Helper.SendSystemMessageToAllClients(
					$"{connectedPlayer.Name.ToString().Colorify(ExtendedColor.ClanNameColor)} " +
					VArenaNotifyConfig.Config.playerConnectedNotification);
		}

		public static void SendPlayerNewAnnouncement (Player newPlayer)
		{
			if (VArenaNotifyConfig.Config.playerNewNotification != "")
				Helper.SendSystemMessageToAllClients(VArenaNotifyConfig.Config.playerNewNotification);
		}
	}
}