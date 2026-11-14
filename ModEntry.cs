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
            BerryBadge.Register();
            YeastBadge.Register();
            WaterBadge.Register();
            WineBadge.Register();
            IngotBadge.Register();
            ChemBadge.Register();
            ModHook.OnGameLoadedNormal += OnGameLoaded;
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Berries", BerryBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Yeast", YeastBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Water", WaterBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Wine", WineBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Ingots", IngotBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_SPAWN, "IB Chems", ChemBadge.SpawnSamples);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Dump Tools", DumpTools);
            ModHelper.AddDebugFunction(MOD_ID, ModHelper.DEBUG_CATEGORY_COMMAND, "IB Refresh Badges", BadgeRenderer.RefreshAll);
        }

        public void OnDisable()
        {
            ModHook.OnGameLoadedNormal -= OnGameLoaded;
            BadgeRegistry.Clear();
            ModHelper.RemoveDebugFunctions(MOD_ID);
        }

        private static void OnGameLoaded()
        {
            ToolAccess.Invalidate();
            BadgeRenderer.RefreshAll();
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
