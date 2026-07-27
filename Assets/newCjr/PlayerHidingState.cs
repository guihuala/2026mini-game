using System;
using UnityEngine;

/// <summary>Shared player state used by hideouts and monster perception.</summary>
public class PlayerHidingState : MonoBehaviour
{
    public bool IsHidden { get; private set; }
    public HidingCabinet CurrentCabinet { get; private set; }

    public event Action<bool> HiddenChanged;

    public void SetHidden(HidingCabinet cabinet, bool hidden)
    {
        CurrentCabinet = hidden ? cabinet : null;
        if (IsHidden == hidden) return;

        IsHidden = hidden;
        HiddenChanged?.Invoke(hidden);
    }
}
