using UnityEngine;
using Unity.Entities;

/// <summary>
/// 球を削除するトリガー用のオーサリングコンポーネント
/// </summary>
public class DestroyBallTriggerAuthoring : MonoBehaviour
{
    class Baker : Baker<DestroyBallTriggerAuthoring>
    {
        public override void Bake(DestroyBallTriggerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<DestroyBallTrigger>(entity);
        }
    }
}
