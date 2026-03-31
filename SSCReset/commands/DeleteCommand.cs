using TShockAPI;

namespace SSCReset
{
    public static class DeleteCommand
    {
        private static readonly Dictionary<string, DateTime> _confirmations = new();

        public static void Execute(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Use: /sscdelete <all|player name>");
                args.Player.SendInfoMessage("/sscdelete list - Show all characters");
                return;
            }

            string target = string.Join(" ", args.Parameters);
            bool isAll = target.Equals("all", StringComparison.OrdinalIgnoreCase);

            if (target.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                var names = SSCReset.Database.GetAllCharacterNames();
                if (names.Count == 0)
                {
                    args.Player.SendInfoMessage("No SSC characters found.");
                }
                else
                {
                    args.Player.SendInfoMessage($"SSC Characters ({names.Count}): {string.Join(", ", names)}");
                }
                return;
            }

            if (SSCReset.Config.RequireConfirmation)
            {
                string confirmKey = $"{args.Player.Name}:{target}";
                
                if (!_confirmations.ContainsKey(confirmKey) || 
                    (DateTime.Now - _confirmations[confirmKey]).TotalSeconds > 30)
                {
                    _confirmations[confirmKey] = DateTime.Now;
                    args.Player.SendWarningMessage($"Are you sure you want to delete {(isAll ? "ALL characters" : $"{target}'s character")}?");
                    args.Player.SendWarningMessage("Run the command again within 30 seconds to confirm.");
                    return;
                }
                _confirmations.Remove(confirmKey);
            }

            if (isAll)
            {
                DeleteAll(args.Player);
            }
            else
            {
                DeletePlayer(args.Player, target);
            }
        }

        private static void DeleteAll(TSPlayer executor)
        {
            if (!SSCReset.Config.AllowDeleteOnline && TShock.Players.Any(p => p?.Active == true && p != executor))
            {
                executor.SendErrorMessage("Cannot delete all characters while players are online!");
                return;
            }

            int count = SSCReset.Database.GetCharacterCount();
            if (count == 0)
            {
                executor.SendErrorMessage("No characters to delete!");
                return;
            }

            if (SSCReset.Database.DeleteAllCharacters())
            {
                string message = $"{executor.Name} deleted all SSC characters ({count} total).";
                
                if (SSCReset.Config.LogDeletions)
                    TShock.Log.ConsoleInfo($"[SSCReset] {message}");

                if (SSCReset.Config.BroadcastDeletions)
                    TSPlayer.All.SendInfoMessage(message);

                executor.SendSuccessMessage($"Successfully deleted all SSC characters ({count} total)!");
            }
            else
            {
                executor.SendErrorMessage("Failed to delete all characters. Check logs for details.");
            }
        }

        private static void DeletePlayer(TSPlayer executor, string playerName)
        {
            int accountId = SSCReset.Database.GetAccountId(playerName);

            if (accountId == -1)
            {
                executor.SendErrorMessage($"Player '{playerName}' not found!");
                return;
            }

            if (!SSCReset.Database.CharacterExists(accountId))
            {
                executor.SendErrorMessage($"Player '{playerName}' does not have an SSC character!");
                return;
            }

            if (!SSCReset.Config.AllowDeleteOnline && SSCReset.Database.IsPlayerOnline(playerName))
            {
                executor.SendErrorMessage($"Cannot delete character while {playerName} is online!");
                return;
            }

            if (SSCReset.Database.DeleteCharacter(accountId))
            {
                string message = $"{executor.Name} deleted {playerName}'s SSC character.";
                
                if (SSCReset.Config.LogDeletions)
                    TShock.Log.ConsoleInfo($"[SSCReset] {message}");

                if (SSCReset.Config.BroadcastDeletions)
                    TSPlayer.All.SendInfoMessage(message);

                executor.SendSuccessMessage($"Successfully deleted {playerName}'s character!");
            }
            else
            {
                executor.SendErrorMessage($"Failed to delete {playerName}'s character.");
            }
        }
    }
}