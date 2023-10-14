using UnityEngine;
using Unity.Entities;

/// <summary>
/// Paddle用のオーサリングコンポーネント
/// </summary>
public class PaddleAuthoring : MonoBehaviour
{
    // Inspectorで編集するためのフィールド
    // Bake時にこの値からコンポーネントを作成する
    [SerializeField] private float _speed;

    // Bakerクラス内にBake時の処理を記述する
    class Baker : Baker<PaddleAuthoring>
    {
        public override void Bake(PaddleAuthoring authoring)
        {
            // GameObjectに対応するEntityを取得
            // 基本的に動かすEntityに対してはTransformUsageFlags.Dynamicを指定
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // EntityにPaddleコンポーネントを追加する
            AddComponent(entity, new Paddle() { Speed = authoring._speed });
        }
    }
}