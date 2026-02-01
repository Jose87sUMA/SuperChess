using System;
using System.Collections.Generic;
using System.Text;

namespace Localization
{
	internal static class LocalizationKeyMap
	{
		private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
		{
			{"Language", "ui.settings.language.label"},
			{"Idioma", "ui.settings.language.label"},

			{"Music", "ui.settings.music"},
			{"Música", "ui.settings.music"},

			{"Sound Effects", "ui.settings.sfx"},
			{"Efectos de Sonido", "ui.settings.sfx"},

			{"Local Game", "ui.menu.localGame"},
			{"Juego local", "ui.menu.localGame"},

			{"Online Game", "ui.menu.onlineGame"},
			{"Juego en línea", "ui.menu.onlineGame"},

			{"Host Game", "ui.menu.host"},
			{"Anfitrión", "ui.menu.host"},

			{"Connect", "ui.menu.connect"},
			{"Conectar", "ui.menu.connect"},

			{"Create Game", "ui.online.host"},
			{"Crear partida", "ui.online.host"},

			{"Join Game", "ui.online.join"},
			{"Conectarse", "ui.online.join"},

			{"Waiting for opponent...", "ui.online.waiting"},
			{"Esperando por oponente...", "ui.online.waiting"},

			{"Go back", "ui.common.back"},
			{"Volver", "ui.common.back"},

			{"Confirm", "ui.common.confirm"},
			{"Confirmar", "ui.common.confirm"},

			{"Cancel", "ui.common.cancel"},
			{"Cancelar", "ui.common.cancel"},

			{"Vs AI", "ui.menu.vsAI"},
			{"vs IA", "ui.menu.vsAI"},

			{"Build Deck", "ui.menu.deckBuilder"},
			{"Constructor de mazo", "ui.menu.deckBuilder"},

			{"Settings", "ui.menu.settings"},
			{"Ajustes", "ui.menu.settings"},

			{"Exit", "ui.menu.exit"},
			{"Salir", "ui.menu.exit"},

			{"Main Menu", "ui.menu.main"},
			{"Menú principal", "ui.menu.main"},

			{"Play again", "ui.game.rematch"},
			{"Jugar otra vez", "ui.game.rematch"},

			{"Exit to Menu", "ui.game.exitToMenu"},
			{"Salir al menú", "ui.game.exitToMenu"},

			{"White pieces win", "ui.game.whiteWins"},
			{"Ganan las piezas blancas", "ui.game.whiteWins"},

			{"Black pieces win", "ui.game.blackWins"},
			{"Ganan las piezas negras", "ui.game.blackWins"},

			{"Draw", "ui.game.draw"},
			{"Empate", "ui.game.draw"},

			{"Easy", "ui.menu.ai.easy"},
			{"Fácil", "ui.menu.ai.easy"},

			{"Medium", "ui.menu.ai.medium"},
			{"Medio", "ui.menu.ai.medium"},

			{"Hard", "ui.menu.ai.hard"},
			{"Difícil", "ui.menu.ai.hard"},

			{"Beginner", "ui.menu.ai.beginner"},
			{"Principiante", "ui.menu.ai.beginner"},

			{"Normal", "ui.menu.ai.normal"},

			{"Expert", "ui.menu.ai.expert"},
			{"Experto", "ui.menu.ai.expert"},

			{"Choose the difficulty", "ui.menu.ai.prompt"},
			{"Escoge dificultad", "ui.menu.ai.prompt"},

			{"Deck Builder", "ui.deck.title"},
			{"Creación de mazos", "ui.deck.title"}
		};

		public static bool TryGetKey(string defaultText, out string key)
		{
			var normalized = Normalize(defaultText);
			if (string.IsNullOrEmpty(normalized))
			{
				key = null;
				return false;
			}

			return Map.TryGetValue(normalized, out key);
		}

		public static string Normalize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;

			var sb = new StringBuilder(text.Length);
			var wasWhitespace = false;

			foreach (var c in text)
			{
				if (char.IsWhiteSpace(c))
				{
					if (!wasWhitespace)
					{
						sb.Append(' ');
						wasWhitespace = true;
					}
				}
				else
				{
					sb.Append(c);
					wasWhitespace = false;
				}
			}

			var trimmed = sb.ToString().Trim();
			if (trimmed.EndsWith(':'))
				trimmed = trimmed[..^1].TrimEnd();

			return trimmed;
		}
	}
}
