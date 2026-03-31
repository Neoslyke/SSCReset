using System.Data;
using TShockAPI;
using TShockAPI.DB;

namespace SSCReset
{
    public class SSCDatabase
    {
        private readonly IDbConnection _database;

        public SSCDatabase(IDbConnection db)
        {
            _database = db;
        }

        public bool DeleteCharacter(int accountId)
        {
            try
            {
                int affected = _database.Query("DELETE FROM tsCharacter WHERE Account = @0", accountId);
                
                if (affected > 0)
                {
                    TShock.CharacterDB.RemovePlayer(accountId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error deleting character: {ex.Message}");
                return false;
            }
        }

        public bool WipeInventory(int accountId)
        {
            try
            {
                string emptyInventory = string.Join("~", Enumerable.Repeat("0,0,0", NetItem.MaxInventory));
                
                int affected = _database.Query(@"UPDATE tsCharacter SET 
                    Health = @0,
                    MaxHealth = @1,
                    Mana = @2,
                    MaxMana = @3,
                    Inventory = @4,
                    extraSlot = @5,
                    questsCompleted = @6
                    WHERE Account = @7",
                    100,
                    100,
                    20,
                    20,
                    emptyInventory,
                    -1,
                    0,
                    accountId);

                return affected > 0;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error wiping inventory: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAllCharacters()
        {
            try
            {
                var accountIds = GetAllAccountIds();
                int affected = _database.Query("DELETE FROM tsCharacter");
                
                foreach (var id in accountIds)
                {
                    TShock.CharacterDB.RemovePlayer(id);
                }
                
                return affected > 0;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error deleting all characters: {ex.Message}");
                return false;
            }
        }

        public bool WipeAllInventories()
        {
            try
            {
                string emptyInventory = string.Join("~", Enumerable.Repeat("0,0,0", NetItem.MaxInventory));
                
                int affected = _database.Query(@"UPDATE tsCharacter SET 
                    Health = @0,
                    MaxHealth = @1,
                    Mana = @2,
                    MaxMana = @3,
                    Inventory = @4,
                    extraSlot = @5,
                    questsCompleted = @6",
                    100, 100, 20, 20, emptyInventory, -1, 0);

                return affected > 0;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error wiping all inventories: {ex.Message}");
                return false;
            }
        }

        public int GetAccountId(string playerName)
        {
            try
            {
                var user = TShock.UserAccounts.GetUserAccountByName(playerName);
                if (user != null)
                {
                    return user.ID;
                }
                return -1;
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error getting account ID: {ex.Message}");
                return -1;
            }
        }

        public bool CharacterExists(int accountId)
        {
            try
            {
                using (var reader = _database.QueryReader("SELECT Account FROM tsCharacter WHERE Account = @0", accountId))
                {
                    return reader.Read();
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error checking character: {ex.Message}");
                return false;
            }
        }

        public bool IsPlayerOnline(string playerName)
        {
            return TShock.Players.Any(p => p?.Active == true && 
                (p.Account?.Name?.Equals(playerName, StringComparison.OrdinalIgnoreCase) == true ||
                 p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)));
        }

        public List<int> GetAllAccountIds()
        {
            var ids = new List<int>();
            try
            {
                using (var reader = _database.QueryReader("SELECT Account FROM tsCharacter"))
                {
                    while (reader.Read())
                    {
                        var accountValue = reader.Reader.GetValue(0);
                        if (accountValue != null && accountValue != DBNull.Value)
                        {
                            ids.Add(Convert.ToInt32(accountValue));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error getting account IDs: {ex.Message}");
            }
            return ids;
        }

        public List<string> GetAllCharacterNames()
        {
            var names = new List<string>();
            try
            {
                using (var reader = _database.QueryReader("SELECT Account FROM tsCharacter"))
                {
                    while (reader.Read())
                    {
                        var accountValue = reader.Reader.GetValue(0);
                        if (accountValue != null && accountValue != DBNull.Value)
                        {
                            int accountId = Convert.ToInt32(accountValue);
                            var user = TShock.UserAccounts.GetUserAccountByID(accountId);
                            if (user != null)
                            {
                                names.Add(user.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error getting character names: {ex.Message}");
            }
            return names;
        }

        public int GetCharacterCount()
        {
            try
            {
                using (var reader = _database.QueryReader("SELECT COUNT(*) FROM tsCharacter"))
                {
                    if (reader.Read())
                    {
                        var result = reader.Reader.GetValue(0);
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"[SSCReset] Error getting character count: {ex.Message}");
            }
            return 0;
        }
    }
}