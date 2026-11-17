原版游戏里，葡萄、酵母、水、成品酒、枪械、金属锭和化学品的外观接近，品质与纯度藏在标签、悬停说明或鉴定结果里，仓库和货架一多就很难一眼分清。

本模组在物品格子一角直接叠加品质角标（色块 / 像素数字 / 箭头），方便快速分辨与整理；物品旋转后仍会贴到离设定方向最近的实际占用格，单格物品的角标会自动缩小以减少遮挡。只改显示层，不修改游戏数值、配方或价格公式。

适配游戏：1.0.0（覆盖该补丁全部热修复）

## 怎么用

1. 订阅后启动游戏，在「模组」里启用，然后重启一次。
2. 无需额外道具，进入存档后符合条件的物品会自动显示角标（默认在右下角，默认读标签纯度）。
3. 需工具检查的物品，工具放在手上、店里、工具箱格子或走私者暗格内都会被识别：

- **葡萄 / 酿酒果实**（需**放大镜**）：显示对酒分的贡献。发霉 -1、欠熟/过熟 +0、正常 +1、小颗 +2。兼容《酿酒拓展》果实、巨大果与果浆（果浆随此开关；按档位取四颗均分，按评分显示总分）
- **酵母**（不需工具）：默认酿酒值一律 +1，颜色区分普通 / 超级 / 可循环（紫）/ 非法（红）；可切换为发酵天数。兼容《酿酒拓展》反应器酵母
- **水**（仅含水容器）：默认按水品质（基础水 +0，纯水 +2，沟渠 -3）；可切换按酿酒值 W（纯水与优质 +1，沟渠 -3）。标签纯度不需工具；切到真实纯度需 AquaScan 或未用试纸
- **酒**（不需工具）：覆盖酿造中与成品酒（含《酿酒拓展》配方酒）。默认按档位，自酿（2 分）为 +0、琼浆（6 分）为 +3，更高分同色并继续加数字；也可改按评分原值。假酒显示紫色 x
- **枪械**（不需工具）：只标枪械与电击枪，冷兵器不标。差 -2 / 一般 -1 / 良好 +0 / 极佳 +1 / 新品 +2。成色跟「纯度显示」
- **金属锭**（不需工具）：以平均（Fair）为 +0。极低 -2 / 低 -1 / 平均 +0 / 高 +1 / 极高 +2 / 完美 +3。跟「纯度显示」读标签或真实纯度
- **化学品**：以街头标准为 +0。淀粉 -3 / 垃圾 -2 / 掺假 -1 / 街头 +0 / 近制药 +1 / 制药 +2 / 实验室 +3。标签纯度不需工具；切到真实纯度需成分光谱仪

默认配色为「黑红灰白黄蓝绿」，7 色正中（白）为 +0。

## 设置

主菜单 → 模组 → 齿轮（选项全文见 README）：

- 外观：默认数字、黑红灰白黄蓝绿、右下
- 品质对照：默认关
- 各类显示：默认开
- 酵母数值：默认酿酒值；水质：按水品质；酒：按档位；纯度：标签纯度

## 卸载

在模组菜单里停用并重启即可。纯显示层模组，不写存档数据，不影响存档和已有物品。

## 反馈

遇到报错、显示错位或有改进建议，请附带游戏版本、模组版本（当前 v1.9）、你的操作、截图以及一段 `Player.log`：

- 创意工坊评论（本页）
- GitHub Issues：[Chardexn/ItemBadges](https://github.com/Chardexn/ItemBadges/issues)

源码：[Chardexn/ItemBadges](https://github.com/Chardexn/ItemBadges)

## 更新历史

**v1.9**
- 兼容《酿酒拓展》果实、巨大果、原浆、配方酒与反应器酵母
- 酒分重标定：自酿 0 / 琼浆 +3，支持切换评分原值；酵母支持切换发酵天数
- 新增枪械五档成色；设置面板按种类分块且支持折叠品质对照
- 角标自动贴近实际占用格角落（避免落入枪械等物品包围盒缺口），单格物品缩小显示

**v1.8**
- 新增成品酒、金属锭与化学品角标，支持标签纯度 / 真实纯度切换
- 新增箭头样式、三种 7 色配色、设置页实时色带预览与工具箱 / 走私者暗格工具扫描

**v1.0**
- 首次发布：在葡萄、酵母与水瓶角落显示色块或数字品质角标

---

In the vanilla game, berries, yeast, water, wine, guns, ingots, and chemicals look nearly identical while their quality or purity sits inside labels or inspection tooltips. This mod draws a compact corner badge (swatch, pixel number, or arrow) directly on each item cell. Badges snap to the nearest occupied cell on irregular shapes like guns and shrink on 1x1 items. Display-only: it never alters game stats, recipes, or prices.

Game versions: 1.0.0

## How to use

1. Subscribe, enable the mod, then restart the game.
2. Badges appear automatically once in a save (default bottom right, label purity). Tools held in hand, in storage, in toolbox slots, or in the smuggler bay all count:

- **Berries & Brewing Expansion fruit / pulp** (requires **magnifier**): wine-score contribution (-1 mold, +0 under/overripe, +1 normal, +2 small; pulp averages 4 berries in Grade mode or shows sum in Score mode)
- **Yeast** (no tool): +1 wine score by default with colors for normal / super / reusable (purple) / illegal (red), or switch to ferment days
- **Water** (filled containers): water grade (+0 base, +2 pure, -3 gutterflow) or brewing W. Label needs no tool; real needs AquaScan or an unused test strip
- **Wine** (no tool): fermenting and finished (incl. Brewing Expansion recipe wines). Grade mode sets homemade (2) = +0 and vintage (6) = +3; counterfeit is purple x
- **Guns** (no tool): firearms and stun guns only (melee excluded). Poor -2 / fair -1 / good +0 / excellent +1 / new +2, following Purity display
- **Ingots** (no tool): fair = +0, very low -2 to perfect +3, following Purity display
- **Chemicals**: street standard = +0, starch -3 to lab pure +3. Label needs no tool; real needs the composition spectrometer

## Settings

Mod menu gear (full options in README):

- Appearance: default Numbers, 7-color black-red-gray-white-yellow-blue-green, bottom right
- Quality legend: default off
- Category badges: default on
- Yeast: wine score; Water: by grade; Wine: by grade; Purity: label

## Uninstall

Disable and restart. Display-only; saves and items stay untouched.

## Feedback

Please include game version, mod version (v1.9), what happened, a screenshot, and `Player.log` in workshop comments or [GitHub Issues](https://github.com/Chardexn/ItemBadges/issues).

Source: [Chardexn/ItemBadges](https://github.com/Chardexn/ItemBadges)

## Changelog

**v1.9** Brewing Expansion support (fruit, giant fruit, pulp, recipe wines, reactor yeast); wine rebased (homemade 0 / vintage +3); yeast ferment days mode; 5-grade gun condition; sectioned settings with collapsible legend; occupied-cell snapping and smaller 1x1 badges.

**v1.8** Added wine, ingots, and chemicals; label vs real purity; arrow style, 3 color schemes, live legend strips, toolbox/smuggler tool scanning.

**v1.0** Initial release with swatch/number badges for berries, yeast, and water.
