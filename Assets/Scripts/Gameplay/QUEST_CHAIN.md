# Demo 三段任务链

当前场景使用 `DialogueRuntimeState` 保存轻量任务状态，存档时由 `SaveManager` 一并写入。

## 状态流程

| 步骤 | Quest 状态 | 完成方式 | 同步 Flag |
|---|---|---|---|
| 与守门人交谈 | `not_started` | 对话结束 | `greybox.gate_open` |
| 收集三朵纸花 | `collect_flowers` | `example.flower.count >= 3` | Number 状态 |
| 完成方向校准 | `calibrate` | 收齐纸花后小游戏成功 | `greybox.minigame_complete` |
| 回去复命 | `report_back` | 再次与守门人交谈 | `greybox.ending_unlocked` |
| 前往出口 | `completed` | 出口交互 | `greybox.demo_complete` |

Quest ID 为 `demo.gate_repair`。核心状态转换集中在 `DemoQuestChain.cs`，NPC、小游戏和出口只负责在对应交互结束后推进状态。

## 当前配置位置

- NPC 的提示、角色名和原始完成 Flag：场景中 `NPC - Gate Keeper` 的 `DialogueFlagInteractable`。
- 小游戏 ID、完成 Flag、提示：`Minigame Station - Direction Sequence` 的 `MinigameStationInteractable`。
- 出口所需 Flag：`Exit Portal` 的 `ScenePortal.requiredFlag`，当前为 `greybox.ending_unlocked`。
- 任务 ID、阶段状态名和 HUD 目标文字：`DemoQuestChain.cs`。

更换文案时修改 `DialogueFlagInteractable.GetDialogueText` 和 `DemoQuestChain.GetObjectiveText`。更换关卡条件时修改组件 Inspector 中的 Flag，并同步调整 `DemoQuestChain` 的常量。

## 通用目标类型

`QuestObjectiveDefinition` 的主要配置字段：

- `objectiveId`：同一任务内唯一的目标 ID。
- `description`：显示给玩家的目标文案。
- `type`：目标类型。
- `targetId`：要检查的 Flag、Item 或 Number key。
- `requiredAmount`：数值目标要求的数量，其余类型保持 1。
- `completionEffects`：完成后附加效果，例如 `item:key;add:favor=2`。

| Type | 判定来源 | Target ID 示例 |
|---|---|---|
| `Talk` | Flag | `npc.gatekeeper.reported` |
| `Flag` | Flag | `bridge.repaired` |
| `CollectItem` | Item | `item.lost_bell` |
| `Number` | Number ≥ Required Amount | `flower.count` |
| `Minigame` | Flag | `minigame.direction.complete` |

`QuestDefinition.objectives` 的排列顺序就是任务顺序；`TryComplete` 只接受当前目标，不能跳过前置目标。

## 场景案例

`PaperDiorama` 的 `GREYBOX - EXPLORATION LOOP/QUEST OBJECTIVE EXAMPLES` 下直接保存了 5 个场景对象：

1. 黄色球体：`CollectItem` 案例，拾取 `example.item.lost_bell`。
2. 三个粉色胶囊：`Number` 案例，每个为 `example.flower.count` 增加 1；这三个对象已经接入主线任务 2。
3. 蓝色圆柱：`Flag` 案例，设置 `example.beacon.lit`。

这些物体不是运行时生成的，可以直接在 Hierarchy 中选择、移动、复制或修改组件。铃铛和信标仍是独立类型示例；三朵纸花属于正式流程，未收齐时校准台不能互动。案例结果会显示在灰盒调试面板，点击 Reset 会同时清空主线和案例状态。

如需恢复标准案例布局，执行菜单：

```text
Tools/Template/Quest/Build Static Objective Examples
```

该命令会重新创建 `QUEST OBJECTIVE EXAMPLES` 及其 5 个子物体，并保存 `PaperDiorama` 场景。

## 在 Inspector 中配置交互目标

给带 Collider 的场景物件添加 `QuestObjectiveInteractable`，配置：

1. `Prompt`：玩家靠近时显示的提示。
2. `Objective Type`：选择 Talk、Flag、Collect Item、Number 或 Minigame。
3. `Target Id`：必须与 `QuestObjectiveDefinition.targetId` 完全一致。
4. `Amount`：Number 类型每次交互增加的数量。
5. `Completion Effects`：可选的附加状态效果。
6. `Consume On Interact`：拾取后是否隐藏该物件。

例如“三朵花”目标定义为：

```csharp
new QuestObjectiveDefinition(
    "collect_flowers",
    "收集三朵纸花",
    QuestObjectiveType.Number,
    "flower.count",
    3f);
```

场景中的三朵花分别配置：

```text
Objective Type: Number
Target Id: flower.count
Amount: 1
Consume On Interact: true
```

目标定义负责判断 `flower.count >= 3`；场景交互组件只负责每次贡献 1，二者通过相同的 Target ID 连接。

Talk 和 Minigame 本质上也通过 Flag 连接：对话结束或小游戏成功时调用 `QuestObjectiveInteractable.ApplyContribution`，或者直接使用 `QuestDefinition.TryComplete` 推进当前目标。
