using UnityEngine;
using Unity.Entities;

/// <summary>
/// Block用のオーサリングコンポーネント
/// </summary>
public class BlockAuthoring : MonoBehaviour
{
    class Baker : Baker<BlockAuthoring>
    {
        public override void Bake(BlockAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Block>(entity);
        }
    }
}
