using FishNet.Object;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletFire : NetworkBehaviour
{
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    

    private void Update()
    {

        if (!IsOwner)
            return;
        
        if(Input.GetMouseButtonDown(0))
            Fire();
    }

    private void Fire()
    {
        var startPos = transform.position + new Vector3(0, 0.5f, 0);
        var direction = transform.forward;
        // SpawnBulletLocal(startPos, direction);
        // SpawnBulletServer(startPos, direction, TimeManager.Tick);
        FireServer();
    }

    [ServerRpc]
    private void FireServer()
    {
        var bullet = Instantiate(bulletPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        Spawn(bullet);
        var bulletCom = bullet.GetComponent<Bullet>();
        bulletCom.Initialize(transform.forward, bulletSpeed, Owner.ClientId);
    }

    /*private void SpawnBulletLocal(Vector3 startPos, Vector3 dir)
    {
        var bullet = Instantiate(bulletPrefab, startPos, Quaternion.identity);
        
        var bulletId = Random.Range(0, int.MaxValue);
        
        bullet.Initialize(dir, bulletSpeed, bulletId, OwnerId);
    }
    
    [ServerRpc]
    private void SpawnBulletServer(Vector3 startPos, Vector3 dir, uint startTick)
    {
        var timeDifference = (TimeManager.Tick - startTick) / TimeManager.TickRate;
        var spawnPos = startPos + dir * timeDifference * bulletSpeed;
        
        var bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        
        var bulletId = Random.Range(0, int.MaxValue);

        bullet.Initialize(dir, bulletSpeed, bulletId, OwnerId);

        SpawnBulletObserver(startPos, dir, startTick);
    }

    [ObserversRpc (ExcludeOwner = true)]
    private void SpawnBulletObserver(Vector3 startPos, Vector3 dir, uint startTick)
    {
        var timeDifference = (TimeManager.Tick - startTick) / TimeManager.TickRate;
        var spawnPos = startPos + dir * timeDifference * bulletSpeed;
        
        var bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        var bulletId = Random.Range(0, int.MaxValue);

        bullet.Initialize(dir, bulletSpeed, bulletId, OwnerId);
    }*/
}
