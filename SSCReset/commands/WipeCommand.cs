using TShockAPI;

namespace SSCReset
{
    public static class WipeCommand
    {
        private static readonly Dictionary<string, DateTime> _confirmations = new();

        public static void Execute(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Use: /sscwipe <all|player name>");
                args.Player.SendInfoMessage("/sscwipe list - Show all characters");
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
                string confirmKey = $"{args.Player.Name}:{target}:wipe";
                
                if (!_confirmations.ContainsKey(confirmKey) || 
                    (DateTime.Now - _confirmations[confirmKey]).TotalSeconds > 30)
                {
                    _confirmations[confirmKey] = DateTime.Now;
                    args.Player.SendWarningMessage($"Are you sure you want to wipe {(isAll ? "ALL inventories" : $"{target}'s inventory")}?");
                    args.Player.SendWarningMessage("Run the command again within 30 seconds to confirm.");
                    return;
                }
                _confirmations.Remove(confirmKey);
            }

            if (isAll)
            {
                WipeAll(args.Player);
            }
            else
            {
                WipePlayer(args.Player, target);
            }
        }

        private static void WipeAll(TSPlayer executor)
        {
            if (!SSCReset.Config.AllowWipeOnline && TShock.Players.Any(p => p?.Active == true && p != executor))
            {
                executor.SendErrorMessage("Cannot wipe all inventories while players are online!");
                return;
            }

            int count = SSCReset.Database.GetCharacterCount();
            if (count == 0)
            {
                executor.SendErrorMessage("No characters to wipe!");
                return;
            }

            if (SSCReset.Database.WipeAllInventories())
            {
                string message = $"{executor.Name} wiped all SSC inventories ({count} total).";
                
                if (SSCReset.Config.LogDeletions)
                    TShock.Log.ConsoleInfo($"[SSCReset] {message}");

                if (SSCReset.Config.BroadcastDeletions)
                    TSPlayer.All.SendInfoMessage(message);

                executor.SendSuccessMessage($"Successfully wiped all SSC inventories ({count} total)!");
            }
            else
            {
                executor.SendErrorMessage("Failed to wipe all inventories. Check logs for details.");
            }
        }

        private static void WipePlayer(TSPlayer executor, string playerName)
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

            if (!SSCReset.Config.AllowWipeOnline && SSCReset.Database.IsPlayerOnline(playerName))
            {
                executor.SendErrorMessage($"Cannot wipe inventory while {playerName} is online!");
                return;
            }

            if (SSCReset.Database.WipeInventory(accountId))
            {
                string message = $"{executor.Name} wiped {playerName}'s SSC inventory.";
                
                if (SSCReset.Config.LogDeletions)
                    TShock.Log.ConsoleInfo($"[SSCReset] {message}");

                if (SSCReset.Config.BroadcastDeletions)
                    TSPlayer.All.SendInfoMessage(message);

                executor.SendSuccessMessage($"Successfully wiped {playerName}'s inventory!");
            }
            else
            {
                executor.SendErrorMessage($"Failed to wipe {playerName}'s inventory.");
            }
        }
    }
}