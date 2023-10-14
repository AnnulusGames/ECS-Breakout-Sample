using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// パドルと球の衝突を処理するSystem
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(AfterPhysicsSystemGroup))] // 物理演算の後にSystemを実行
public partial struct BlockCollisionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // シミュレーション結果を保持するシングルトンを取得
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        // EntityCommandBufferを作成
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        // Jobを作成
        var job = new CollisionJob()
        {
            BallLookup = SystemAPI.GetComponentLookup<Ball>(),
            BlockLookup = SystemAPI.GetComponentLookup<Block>(),
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
    struct CollisionJob : ICollisionEventsJob
    {
        // ComponentLookup<TComponent>はEntityのコンポーネントを取得するための構造体
        // ここでは衝突したEntity同士から必要なComponentを取得するために使用する
        public ComponentLookup<Ball> BallLookup;
        public ComponentLookup<Block> BlockLookup;

        // Entityの削除等の処理はメインスレッドでしか行えない (Jobでは実行できない)
        // そのためコマンドをEntityCommandBufferに積み、Jobの完了後にまとめて実行する
        // ParallelWriterはJobで並列書き込みを行うためのもの
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [BurstCompile]
        public void Execute(CollisionEvent collisionEvent)
        {
            // 衝突したEntityの2つのうち、どちらがBallでどちらがBlockかを判定する
            // この辺りはECSの仕組み上どうしても長くなってしまいがちなのが難しいところ
            var aIsBall = BallLookup.HasComponent(collisionEvent.EntityA);
            var bIsBall = BallLookup.HasComponent(collisionEvent.EntityB);

            var aIsBlock = BlockLookup.HasComponent(collisionEvent.EntityA);
            var bIsBlock = BlockLookup.HasComponent(collisionEvent.EntityB);

            // Ball同士、Block同士の衝突やそれ以外の衝突を除外 (今回のサンプルでは両方1つしかないためあり得ないが)
            if (!(aIsBall ^ bIsBall)) return;
            if (!(aIsBlock ^ bIsBlock)) return;

            // blockのEntityを取得
            var blockEntity = aIsBlock ? collisionEvent.EntityA : collisionEvent.EntityB;

            // Entityを削除(するコマンドをバッファに積む)
            CommandBuffer.DestroyEntity(0, blockEntity);
        }
    }
}
