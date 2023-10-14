using Unity.Entities;

/// <summary>
/// パドルのデータを保持するComponent
/// </summary>
public struct Paddle : IComponentData
{
    /// <summary>
    /// パドルの移動速度
    /// </summary>
    public float Speed;
}
