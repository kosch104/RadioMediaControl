using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using UnityEngine;
using WindowsInput;

namespace RadioMediaControl
{
	public class RMC : IMod
	{
		private static readonly ILog log = LogManager.GetLogger($"{nameof(RadioMediaControl)}").SetShowsErrorsInUI(false);

		internal static string ResourcesIcons { get; private set; }

		private Harmony harmony;
		private static InputSimulator simulator;
		public void OnLoad(UpdateSystem updateSystem)
		{
			Debug.Log(nameof(OnLoad));

			if (!GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset)) return;

			log.Info($"Current mod asset at {asset.path}");

			FileInfo fileInfo = new(asset.path);

			ResourcesIcons = Path.Combine(fileInfo.DirectoryName, "Icons");


			harmony = new($"{nameof(RadioMediaControl)}.{nameof(RMC)}");
			harmony.PatchAll(typeof(RMC).Assembly);
			var patchedMethods = harmony.GetPatchedMethods().ToArray();
			log.Info(($"Plugin RadioMediaControl made patches! Patched methods: " + patchedMethods.Length));
			foreach (var patchedMethod in patchedMethods)
			{
				log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			}

			simulator = new InputSimulator();
			ExtendedRadio.ExtendedRadio.OnRadioPaused += RadioPlayPause;
			ExtendedRadio.ExtendedRadio.OnRadioUnPaused += RadioPlayPause;
			ExtendedRadio.ExtendedRadio.OnRadioPreviousSong += RadioPrevious;
			ExtendedRadio.ExtendedRadio.OnRadioNextSong += RadioNext;

			/*try
			{
				Logger.Info("Creating Input Simulator");
				InputSimulator simulator = new InputSimulator();
				Logger.Info("Simulator Created");
				simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PLAY_PAUSE);
				Logger.Info("Button pressed");
			}
			catch (Exception x)
			{
				Logger.Info("Error: " + x);
			}*/
		}

		private void RadioPlayPause()
		{
			log.Info("Hello there");
			simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PLAY_PAUSE);
		}

		private void RadioNext()
		{
			simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_NEXT_TRACK);
		}

		private void RadioPrevious()
		{
			simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.MEDIA_PREV_TRACK);
		}

		public void OnDispose()
		{
			Debug.Log((nameof(OnDispose)));
			harmony.UnpatchAll($"{nameof(RadioMediaControl)}.{nameof(RMC)}");
		}
	}
}
