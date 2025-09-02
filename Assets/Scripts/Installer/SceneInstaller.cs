using Zenject;
using UnityEngine;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TilePool>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GridManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Tile>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CheckMatch>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Obstacle>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ObstaclePool>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AnimationHandler>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ObstacleManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PowerUp>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PowerUpHandler>().FromComponentInHierarchy().AsSingle();
    }
}
