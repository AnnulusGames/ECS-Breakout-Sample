using Unity.Entities;

/// <summary>
/// ブロックを表すComponent
/// </summary>
public struct Block : IComponentData
{
    // 空のComponentはタグとして認識され、クエリに利用できる
}
