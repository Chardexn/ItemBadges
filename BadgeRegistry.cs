using System;
using System.Collections.Generic;

namespace ItemBadges
{
    /// <summary>
    /// 按注册顺序取第一个非空角标。
    /// </summary>
    internal static class BadgeRegistry
    {
        private static readonly List<Entry> Entries = new List<Entry>();

        #region MARK: 注册
        public static void Register(string key, Func<GameItem, Badge?> provider)
        {
            if (string.IsNullOrEmpty(key) || provider == null)
            {
                return;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Key == key)
                {
                    Entries[i] = new Entry(key, provider);
                    return;
                }
            }

            Entries.Add(new Entry(key, provider));
        }

        public static void Clear()
        {
            Entries.Clear();
        }
        #endregion

        #region MARK: 查询
        public static Badge? Resolve(GameItem item)
        {
            if (item == null)
            {
                return null;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                Badge? badge = Entries[i].Provider(item);
                if (badge.HasValue)
                {
                    return badge;
                }
            }

            return null;
        }
        #endregion

        private readonly struct Entry
        {
            public readonly string Key;
            public readonly Func<GameItem, Badge?> Provider;

            public Entry(string key, Func<GameItem, Badge?> provider)
            {
                Key = key;
                Provider = provider;
            }
        }
    }
}
