using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

/// <summary>
/// パドルの操作を担当するSystem
/// </summary>
[BurstCompile]
public partial struct PaddleControlSystem : ISystem
{
    const string AXIS_HORIZONTAL = "Horizontal";

    public void OnUpdate(ref SystemState state)
    {
        // 横方向の入力を取得
        var horizontal = Input.GetAxisRaw(AXIS_HORIZONTAL);

        // Paddle, PhysicsVelocityを持つEntityを全て取得する
        foreach (var (paddle, velocity) in SystemAPI.Query<RefRO<Paddle>, RefRW<PhysicsVelocity>>())
        {
            // パドルの移動速度を変更する
            velocity.ValueRW.Linear = new float3(paddle.ValueRO.Speed * horizontal, 0f, 0f);
        }
    }
}
