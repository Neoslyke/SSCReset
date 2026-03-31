using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SSCReset
{
    [ApiVersion(2, 1)]
    public class SSCReset : TerrariaPlugin
    {
        public override string Name => "SSCReset";
        public override string Author => "Neoslyke";
        public override Version Version => new Version(2, 1, 0);
        public override string Description => "Reset or wipe SSC characters";

        public static Configuration Config { get; private set; } = null!;
        public static SSCDatabase Database { get; private set; } = null!;
        
        private static readonly string ConfigPath = Path.Combine(TShock.SavePath, "SSCReset.json");

        public SSCReset(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            LoadConfig();
            Database = new SSCDatabase(TShock.DB);
            
            Commands.ChatCommands.Add(new Command("sscreset.delete", DeleteCommand.Execute, "sscdelete")
            {
                HelpText = "Deletes SSC character(s). Usage: /sscdelete <all|player name>"
            });

            Commands.ChatCommands.Add(new Command("sscreset.wipe", WipeCommand.Execute, "sscwipe")
            {
                HelpText = "Wipes SSC inventory(s). Usage: /sscwipe <all|player name>"
            });

            TShock.Log.ConsoleInfo("[SSCReset] Plugin initialized successfully!");
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Config = Configuration.GetDefault();
                Config.Save(ConfigPath);
                TShock.Log.ConsoleInfo("[SSCReset] Created default configuration file.");
            }
            else
            {
                Config = Configuration.Load(ConfigPath);
                TShock.Log.ConsoleInfo("[SSCReset] Configuration loaded successfully.");
            }
        }
    }
}