using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HarmonyLib;
using ProjectM;
using ModCore.Data;
using ModCore.Events;
using ModCore.Helpers;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Entities;

namespace ModCore.Services;

[HarmonyPatch(typeof(VivoxConnectionSystem), nameof(VivoxConnectionSystem.OnUpdate))] //this can be any system that updates each frame
public static class ActionScheduler
{
	public static int CurrentFrameCount = 0;
	public static ConcurrentQueue<Action> actionsToExecuteOnMainThread = new ConcurrentQueue<Action>();
	private static List<Timer> activeTimers = new List<Timer>();
	
	public static void Postfix()
	{
		CurrentFrameCount++;
		GameEvents.OnGameFrameUpdate?.Invoke();
		while (actionsToExecuteOnMainThread.TryDequeue(out Action action))
		{
			action?.Invoke();
		}
	}

	public static Timer RunActionEveryInterval(Action action, double intervalInSeconds)
	{
		actionsToExecuteOnMainThread.Enqueue(action);
		return new Timer(_ =>
		{
			actionsToExecuteOnMainThread.Enqueue(action);
		}, null, TimeSpan.FromSeconds(intervalInSeconds), TimeSpan.FromSeconds(intervalInSeconds));
	}

	public static Timer RunActionOnceAfterDelay(Action action, double delayInSeconds)
	{
		Timer timer = null;

		timer = new Timer(_ =>
		{
			// Enqueue the action to be executed on the main thread
			actionsToExecuteOnMainThread.Enqueue(() =>
			{
				action.Invoke();  // Execute the action
				timer?.Dispose(); // Dispose of the timer after the action is executed
			});
		}, null, TimeSpan.FromSeconds(delayInSeconds), Timeout.InfiniteTimeSpan); // Prevent periodic signaling

		return timer;
	}

	public static Timer RunActionOnceAfterFrames(Action action, int frameDelay)
	{
		int startFrame = CurrentFrameCount;
		Timer timer = null;

		timer = new Timer(_ =>
		{
			if (CurrentFrameCount - startFrame >= frameDelay)
			{
				// Enqueue the action to be executed on the main thread
				actionsToExecuteOnMainThread.Enqueue(() =>
				{
					action.Invoke();  // Execute the action
				});
				timer?.Dispose();
			}
		}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(8));

		return timer;
	}

	public static Timer RunActionAtTime(Action action, DateTime scheduledTime)
	{
		// Calculate the delay in milliseconds from now until the scheduled time
		var now = DateTime.Now;
		var delay = scheduledTime - now;

		// If the scheduled time is in the past, execute immediately or adjust according to your needs
		if (delay.TotalMilliseconds < 0)
		{
			return null;
		}
		else
		{
			return RunActionOnceAfterDelay(action, delay.TotalSeconds);
		}
	}


	public static void RunActionOnMainThread(Action action)
	{
		// Enqueue the action to be executed on the main thread
		actionsToExecuteOnMainThread.Enqueue(() =>
		{
			action.Invoke();  // Execute the action
		});
	}
}
