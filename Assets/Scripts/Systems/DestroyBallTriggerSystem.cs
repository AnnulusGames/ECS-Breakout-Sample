using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// 画面外に出た球を削除するSystem
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(AfterPhysicsSystemGroup))] // 物理演算の後にSystemを実行
public partial struct DestroyBallTriggerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // シミュレーション結果を保持するシングルトンを取得
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        // EntityCommandBufferを作成
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        // Jobを作成
        var job = new TriggerJob()
        {
            BallLookup = SystemAPI.GetComponentLookup<Ball>(),
            DestroyTriggerLookup = SystemAPI.GetComponentLookup<DestroyBallTrigger>(),
            CommandBuffer = commandBuffer.AsParallelWriter()
        };

        // Jobをスケジュールし完了を待機
        job.Schedule(simulation, state.Dependency).Complete();

        // コマンドの実行
        commandBuffer.Playback(state.EntityManager);

        // 使い終わったEntityCommandBufferはDisposeで削除
        commandBuffer.Dispose();
    }

    /// <summary>
    /// 衝突イベントを処理するためのJob
    /// </summary>
    [BurstCompile]
    struct TriggerJob : ITriggerEventsJob
    {
        public ComponentLookup<Ball> BallLookup;
        public ComponentLookup<DestroyBallTrigger> DestroyTriggerLookup;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            // 衝突したEntityの2つのうち、どちらがBallでどちらがTriggerかを判定する
            var aIsBall = BallLookup.HasComponent(triggerEvent.EntityA);
            var bIsBall = BallLookup.HasComponent(triggerEvent.EntityB);

            var aIsTrigger = DestroyTriggerLookup.HasComponent(triggerEvent.EntityA);
            var bIsTrigger = DestroyTriggerLookup.HasComponent(triggerEvent.EntityB);

            // Ball同士、Trigger同士の衝突やそれ以外の衝突を除外
            if (!(aIsBall ^ bIsBall)) return;
            if (!(aIsTrigger ^ bIsTrigger)) return;

            // ballのEntityを取得
            var ballEntity = aIsBall ? triggerEvent.EntityA : triggerEvent.EntityB;

            // Entityを削除(するコマンドをバッファに積む)
            CommandBuffer.DestroyEntity(0, ballEntity);
        }
    }
}
