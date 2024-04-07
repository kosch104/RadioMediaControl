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

namespace RadioMediaControl
{
	public class RMC : IMod
	{
		private static readonly ILog log = LogManager.GetLogger($"{nameof(RadioMediaControl)}").SetShowsErrorsInUI(false);

		internal static string ResourcesIcons { get; private set; }

		private Harmony harmony;
		private string pathToCustomRadiosFolder;
		public void OnLoad(UpdateSystem updateSystem)
		{
			Debug.Log(nameof(OnLoad));

			if (!GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset)) return;

			log.Info($"Current mod asset at {asset.path}");

			harmony = new($"{nameof(RadioMediaControl)}.{nameof(RMC)}");
			harmony.PatchAll(typeof(RMC).Assembly);
			var patchedMethods = harmony.GetPatchedMethods().ToArray();
			log.Info(($"Plugin RadioMediaControl made patches! Patched methods: " + patchedMethods.Length));
			foreach (var patchedMethod in patchedMethods)
			{
				log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			}
			LocalRadio.OnLoad();

			pathToCustomRadiosFolder = $"{new FileInfo(asset.path).DirectoryName}\\CustomRadios";
			ExtendedRadio.CustomRadios.RegisterCustomRadioDirectory(pathToCustomRadiosFolder);


		}



		public void OnDispose()
		{
			Debug.Log((nameof(OnDispose)));
			ExtendedRadio.CustomRadios.UnRegisterCustomRadioDirectory(pathToCustomRadiosFolder);
			harmony.UnpatchAll($"{nameof(RadioMediaControl)}.{nameof(RMC)}");
		}
	}
}
