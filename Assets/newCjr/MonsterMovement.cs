using UnityEngine;

/// <summary>
/// 怪物移动方式的统一接口。关卡逻辑只依赖此类型，无需关心怪物使用 A* 还是固定路径。
/// </summary>
public abstract class MonsterMovement : MonoBehaviour
{
    public abstract bool IsPaused { get; }

    public abstract void StartMoving();
    public abstract void Pause();
    public abstract void Resume();
    public abstract void ResetToSpawn();
}
