# 钥匙与门系统

## 脚本关系

```
Key (Trigger) ──拾取──→ PlayerKeyInventory (背包) ──开门──→ Door (碰撞体)
                              ↑
                    MiniSceneManager (管理重置)
```

## Player 配置

挂载 `PlayerKeyInventory`：
- Inspector 可见当前钥匙列表
- `Add Key (Debug)` 按钮可调试添加钥匙
- 代码添加：`GetComponent<PlayerKeyInventory>().AddKey("red")`

## Key（钥匙）配置

| 步骤 | 说明 |
|------|------|
| 1 | 创建钥匙 GameObject |
| 2 | 挂 Collider2D（脚本自动设为 Is Trigger） |
| 3 | 挂 `Key` 脚本，填 **Key Id**（如 `red`） |

**效果**：玩家碰到钥匙 → 钥匙字符串存入背包 → 钥匙物体隐藏

## Door（门）配置

| 步骤 | 说明 |
|------|------|
| 1 | 创建门 GameObject |
| 2 | 挂 Collider2D（**不要**勾选 Is Trigger，门需要阻挡玩家） |
| 3 | 挂 `Door` 脚本，填与对应钥匙相同的 **Key Id** |

**效果**：玩家带着匹配钥匙碰到门 → 钥匙消耗 → 门隐藏

## 死亡重置

`MiniSceneManager` 在 Start 搜集场景所有 Key 和 Door。
玩家死亡重置时：
1. 清空玩家背包
2. 所有 Key 和 Door 恢复显示（`SetActive(true)`）

## 示例

| Key Id | 钥匙 | 门 |
|--------|------|-----|
| `red_key` | 红色钥匙挂在墙边 | 红色门挡住通道 |
| `boss_key` | Boss 钥匙在角落里 | Boss 房间入口门 |

> **配对规则**：Key.Id = Door.Id，字符串完全一致即可解锁。
