# 钥匙与门配置指南

钥匙本质是字符串，存在 Player 的 `PlayerKeyInventory` 里，不是场景里的物理物体。

## 玩家配置

Player GameObject 挂载 `PlayerKeyInventory` 脚本，通过以下方式获得钥匙：

- **代码**：`GetComponent<PlayerKeyInventory>().AddKey("red")`
- **调试按钮**：Inspector 里用 `Add Key (Debug)` 直接添加

## 门配置

1. 创建门 GameObject，挂 Collider2D（勾选 **Is Trigger**）
2. 挂载 `Door` 脚本
3. 填写 **Key Id**，如 `red`

## 流程

玩家带着 Collider 碰到门 → Door 遍历 `PlayerKeyInventory` → 匹配 `Key Id` → 钥匙被消耗，门 Collider 关闭（门消失）

## 示例

| 钥匙来源 | Key Id | 对应门 |
|----------|--------|--------|
| 对话获得 | `red` | 红色门填 `red` |
| 任务奖励 | `boss` | Boss 门填 `boss` |

一对钥匙和门只要 **Key Id 完全一致** 即可配对。
