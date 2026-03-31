namespace SSCReset
{
    public class PlayerData
    {
        public int ID { get; set; }
        public int Account { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public string Inventory { get; set; } = string.Empty;
        public int ExtraSlot { get; set; }
        public int QuestsCompleted { get; set; }
    }
}