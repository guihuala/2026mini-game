# 48h Game Jam 框架

这里只保留当前游戏已经接入的基础能力：

- `GameManager`：播放、暂停和结束状态。
- `Input`：按键映射与简单重绑定。
- `SceneFlow`：场景名称映射与异步切换。
- `Save`：单份进度存取和基础校验。
- `Dialogue`：直接由代码或 `DialogueSequenceAsset` 驱动的轻量对话。
- `Utils`：MonoBehaviour 单例基类。

为缩短导入、编译和维护时间，模板原有的本地化、Excel 配置表、事件总线、对象池、独立时间管理器及示例数据均已移除。需要新功能时优先按实际玩法补充，避免预先引入未使用的抽象层。
