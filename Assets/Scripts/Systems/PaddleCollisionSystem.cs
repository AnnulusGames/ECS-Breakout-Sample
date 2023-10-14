using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

/// <summary>
/// パドルと球の衝突を処理するSystem
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(AfterPhysicsSystemGroup))] // 物理演算の後にSystemを実行
public partial struct PaddleCollisionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // シミュレーション結果を保持するシングルトンを取得
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        // Jobを作成
        var job = new CollisionJob()
        {
            BallLookup = SystemAPI.GetComponentLookup<Ball>(),
            PaddleLookup = SystemAPI.GetComponentLookup<Paddle>(),
            LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(),
            VelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            StorageInfoLookup = SystemAPI.GetEntityStorageInfoLookup()
        };

        // Jobをスケジュールし、state.DependencyにJobHandleを代入
        state.Dependency = job.Schedule(simulation, state.Dependency);
    }

    /// <summary>
    /// 衝突イベントを処理するためのJob
    /// </summary>
    [BurstCompile]
    struct CollisionJob : ICollisionEventsJob
    {
        public ComponentLookup<Ball> BallLookup;
        public ComponentLookup<Paddle> PaddleLookup;
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        public ComponentLookup<PhysicsVelocity> VelocityLookup;

        public EntityStorageInfoLookup StorageInfoLookup;

        [BurstCompile]
        public void Execute(CollisionEvent collisionEvent)
        {
            // Entityの存在チェック
            if (!StorageInfoLookup.Exists(collisionEvent.EntityA) || !StorageInfoLookup.Exists(collisionEvent.EntityB)) return;

            // 衝突したEntityの2つのうち、どちらがBallでどちらがPaddleかを判定する
            var aIsBall = BallLookup.HasComponent(collisionEvent.EntityA);
            var bIsBall = BallLookup.HasComponent(collisionEvent.EntityB);

            var aIsPaddle = PaddleLookup.HasComponent(collisionEvent.EntityA);
            var bIsPaddle = PaddleLookup.HasComponent(collisionEvent.EntityB);

            // Ball同士、Paddle同士の衝突やそれ以外の衝突を除外
            if (!(aIsBall ^ bIsBall)) return;
            if (!(aIsPaddle ^ bIsPaddle)) return;

            // ballとpaddleのEntityをそれぞれ取得
            var (ballEntity, paddleEntity) = aIsBall ?
                (collisionEvent.EntityA, collisionEvent.EntityB) :
                (collisionEvent.EntityB, collisionEvent.EntityA);

            // 球とパドルの位置を取得
            var ballWorldPosition = LocalToWorldLookup[ballEntity].Position;
            var paddleWorldPosition = LocalToWorldLookup[paddleEntity].Position;

            // 球がパドルより上にいる場合は除外
            if (ballWorldPosition.y < paddleWorldPosition.y) return;

            // 球がパドルの左右どちらにいるかで向きを変える
            var direction = ballWorldPosition.x > paddleWorldPosition.x ? 1 : -1;

            // 球の速度を変更
            var ballVelocity = VelocityLookup.GetRefRW(ballEntity);
            ballVelocity.ValueRW.Linear = new float3(direction, 0f, 1f) * math.length(ballVelocity.ValueRW.Linear);
        }
    }
}
