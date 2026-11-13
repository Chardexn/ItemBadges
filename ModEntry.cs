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
            ModHelper.AddDebugFunction(
                MOD_ID,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Spawn Berry Samples (4 grades)",
                BerryBadge.SpawnSamples);
            ModHelper.AddDebugFunction(
                MOD_ID,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Spawn Yeast Samples (4 kinds)",
                YeastBadge.SpawnSamples);
            ModHelper.AddDebugFunction(
                MOD_ID,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Spawn Water Samples (6 grades)",
                WaterBadge.SpawnSamples);
        }

        public void OnDisable()
        {
            BadgeRegistry.Clear();
            ModHelper.RemoveDebugFunctions(MOD_ID);
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
