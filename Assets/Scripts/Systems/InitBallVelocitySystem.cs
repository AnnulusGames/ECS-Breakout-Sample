using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

/// <summary>
/// 球の初速を設定するSystem
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct InitBallVelocitySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // SubSceneは非同期でロードされるため、Ballが読み込まれるのを待機する必要がある
        // RequireForUpdateで特定のComponentを持つEntityが存在しない場合にUpdateをスキップできる
        state.RequireForUpdate<Ball>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Ball、PhysicsVelocityを持つEntityを取得
        foreach (var (ball, velocity) in SystemAPI.Query<RefRO<Ball>, RefRW<PhysicsVelocity>>())
        {
            // 初速を設定
            velocity.ValueRW.Linear = math.normalize(new float3(1f, 0f, 1f)) * ball.ValueRO.Speed;
        }

        // 実行後はSystemを無効化する
        state.Enabled = false;
    }
}
