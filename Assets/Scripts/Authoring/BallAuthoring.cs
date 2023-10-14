using UnityEngine;
using Unity.Entities;

/// <summary>
/// Ball用のオーサリングコンポーネント
/// </summary>
public class BallAuthoring : MonoBehaviour
{
    [SerializeField] private float _speed;

    class Baker : Baker<BallAuthoring>
    {
        public override void Bake(BallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Ball() { Speed = authoring._speed });
        }
    }
}
