using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// 球の速度を調整するSystem
/// </summary>
[BurstCompile] // classを使用しないため[BurstCompile]による高速化が適用可能
[UpdateInGroup(typeof(BeforePhysicsSystemGroup))] // 物理演算の前に実行
public partial struct UpdateBallVelocitySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Ball、PhysicsVelocityを持つEntityを取得
        foreach (var (ball, velocity) in SystemAPI.Query<RefRO<Ball>, RefRW<PhysicsVelocity>>())
        {
            // 速度を一定に保つ
            velocity.ValueRW.Linear = math.normalizesafe(velocity.ValueRW.Linear) * ball.ValueRO.Speed;
        }
    }
}
