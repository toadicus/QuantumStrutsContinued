// QuantumStrutsContinued © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Continued from QuantumStrut, © 2013 BoJaN.  Used with permission.

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace QuantumStrut
{
	public static partial class Tools
	{
		public enum MessageLevel
		{
			Log,
			Warning,
			Error,
			Exception
		}

		/// <summary>
		/// Formats objects into a message for logging.
		/// </summary>
		/// <param name="Sender">Sending object</param>
		/// <param name="args">A set of arguments to be printed.</param>
		private static string FormatMessage(object Sender, params object[] args)
		{
			string Msg;

			Msg = string.Format(
				"{0}:\t{1}",
				Sender.GetType().Name,
				string.Join("\n\t", args.Select(a => (string)a).ToArray())
			);

			return Msg;
		}

		private static ScreenMessage debugmsg = new ScreenMessage("", 4f, ScreenMessageStyle.UPPER_RIGHT);

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(string Msg)
		{
			PostLogMessage(MessageLevel.Log, Msg);

			if (HighLogic.LoadedScene > GameScenes.SPACECENTER)
			{
				debugmsg.message = Msg;
				ScreenMessages.PostScreenMessage(debugmsg, true);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(object Sender, params object[] args)
		{
			string Msg;
			Msg = FormatMessage(Sender, args);

			PostDebugMessage(Msg);
		}

		/// <summary>
		/// Posts a message to the KSP log.
		/// </summary>
		/// <param name="Level">Log level</param>
		/// <param name="Msg">The message to be printed</param>
		public static void PostLogMessage(MessageLevel Level, string Msg)
		{
			System.Action<string> logMethod;

			switch (Level)
			{
				case MessageLevel.Log:
					logMethod = Debug.Log;
					break;
				case MessageLevel.Warning:
					logMethod = Debug.LogWarning;
					break;
				case MessageLevel.Error:
					logMethod = Debug.LogError;
					break;
				default:
					throw new System.NotImplementedException("Exception message levels not yet implemented.");
			}

			logMethod(Msg);
		}

		/// <summary>
		/// Posts a message to the KSP log.
		/// </summary>
		/// <param name="Level">Log level</param>
		/// <param name="Sender">Sending object</param>
		/// <param name="args">A set of arguments to be printed.</param>
		public static void PostLogMessage(MessageLevel Level, object Sender, params object[] args)
		{
			string Msg;
			Msg = FormatMessage(Sender, args);

			PostLogMessage(Level, Msg);
		}

		/// <summary>
		/// Posts a warning message to the KSP log.
		/// </summary>
		/// <param name="Msg">The message to be logged.</param>
		public static void PostWarningMessage(string Msg)
		{
			PostLogMessage(MessageLevel.Warning, Msg);
		}

		/// <summary>
		/// Posts a warning message to the KSP log.
		/// </summary>
		/// <param name="Sender">The sending object</param>
		/// <param name="args">A set of arguments to be printed.</param>
		public static void PostWarningMessage(object Sender, params object[] args)
		{
			PostLogMessage(MessageLevel.Warning, Sender, args);
		}
	}
}
