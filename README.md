# 物品角标（ItemBadges）

《深空当铺》(Probably Stolen) 模组。在物品右下角显示品质 / 种类角标（色块或数字），无需鉴定即可分辨葡萄、酵母和水。

A quality-of-life mod for *Probably Stolen*. Draws a corner badge (swatch and/or number) on berries, yeast, and water so you can tell grade or type without identifying them.

## 功能

- **葡萄**：隐藏品质。数字是对酒分的实际贡献：发霉 -1，欠熟/过熟 0，正常 +1，小颗 +2
- **酵母**：普通酵母显示发酵速度；超级酵母更快；非法酵母酿假酒；可循环酵母显示 ∞
- **水**：以基础水（98%）为 0。纯水 +2，优质 +1，基础 0，幽灵 -1，锈水 -2，沟水 -3（真实纯净度）

设置在游戏「模组」菜单的齿轮里：角标样式（色块 / 数字 / 两者）、颜色预设、色块与字号、三类物品各自开关。

## 安装

把编译产物放到游戏的 `Mods/ItemBadges/`（需包含 `manifest.xml`、`ItemBadges.dll` 以及 `Localization/`）。启用后重启游戏。

本仓库是源码，作为作者私有工作区的 Git 子模块编译：项目引用工作区共享的 `build/PsmMod.props`。单独克隆后无法直接 `dotnet build`，请把本目录放到该工作区根目录后再编译，或把已编译的 DLL 与资源复制进游戏 `Mods/`。

## 清单

| 字段 | 值 |
|---|---|
| ID | `item_badges` |
| 游戏版本 | `1.0.0` |
| 能力 | 无 |
