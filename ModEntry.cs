namespace ItemBadges
{
    /// <summary>
    /// Mod 入口。Init / OnEnable 在主菜单运行；补丁由加载器自动应用。
    /// </summary>
    public class ModEntry : IMod
    {
        public const string MOD_ID = "item_badges";

        public static ModEntry Instance { get; private set; }
        public ModManifest Manifest { get; private set; }
        public ModLog Log { get; private set; }

        #region MARK: 生命周期
        public void Init(ModManifest manifest)
        {
            Instance = this;
            Manifest = manifest;
            Log = new ModLog(manifest);
            BadgeConfig.Define();
            Log.Log($"{manifest.Name} v{manifest.ModVersion} loaded.");
        }

        public void OnEnable()
        {
            PulpBadge.Register();
            BerryBadge.Register();
            YeastBadge.Register();
            WaterBadge.Register();
            WineBadge.Register();
            GunBadge.Register();
            IngotBadge.Register();
            ChemBadge.Register();
            ModHook.OnGameLoadedLate += OnGameLoaded;
            ModHook.OnWakingUpLate += OnNightlyRefresh;
            ModHook.OnHandlingNightlyServicesLate += OnNightlyRefresh;
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Berries", BerryBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Yeast", YeastBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Water", WaterBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Wine", WineBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Ingots", IngotBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Chems", ChemBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Guns", GunBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Showcase Vanilla", BadgeShowcase.RunVanilla);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Showcase Guns", BadgeShowcase.RunGuns);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Showcase Mod", BadgeShowcase.RunMod);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Clear Showcase", BadgeShowcase.Clear);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Dump Tools", DumpTools);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Refresh Badges", BadgeRenderer.RefreshWorld);
        }

        public void OnDisable()
        {
            ModHook.OnGameLoadedLate -= OnGameLoaded;
            ModHook.OnWakingUpLate -= OnNightlyRefresh;
            ModHook.OnHandlingNightlyServicesLate -= OnNightlyRefresh;
            BadgeRegistry.Clear();
            ModHelper.RemoveDebugFunctions(MOD_ID);
        }

        private static void OnGameLoaded()
        {
            BadgeRenderer.RefreshWorld();
            ModHelper.Delay(MOD_ID, 0.25f, BadgeRenderer.RefreshWorld);
        }

        private static void OnNightlyRefresh()
        {
            BadgeRenderer.RefreshWorld();
        }

        private static void DumpTools()
        {
            Instance?.Log.Log("ToolAccess " + ToolAccess.Dump());
        }
        #endregion

        #region MARK: 本地化
        public static string L(string key, params object[] args)
        {
            return ModHelper.GetLocalized(MOD_ID, key, args);
        }
        #endregion
    }
}
