using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerCollisionRollback : NetworkBehaviour
{
    public static Dictionary<int, PlayerCollisionRollback> Players = new Dictionary<int, PlayerCollisionRollback>();


    private List<PlayerState> _pastStates = new List<PlayerState>();
    private CapsuleCollider _capsuleCollider;
    private float _capsuleRadius, _capsuleHeight;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServerInitialized)
            enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        Players.Add(Owner.ClientId, this);
        
        if(TryGetComponent(out CapsuleCollider capsuleCollider))
            _capsuleCollider = capsuleCollider;
        
        _capsuleHeight = _capsuleCollider.height;
        _capsuleRadius = _capsuleCollider.radius;

        TimeManager.OnTick += OnTick;
    }

    private void OnTick()
    {
        if(_pastStates.Count > TimeManager.TickRate)
            _pastStates.RemoveAt(0);
        
        _pastStates.Add(new PlayerState{Position = transform.position});
    }

    public bool CheckPastCollisions(Bullet bullet)
    {
        for (var i = 0; i < Mathf.Min(_pastStates.Count, bullet.PastStates.Count); i++)
        {
            var playerPos = _pastStates[i].Position;
            var bulletPos = bullet.PastStates[i].Position;

            var capsuleCenter = playerPos + Vector3.up * _capsuleRadius;
            var point1 = capsuleCenter + Vector3.up * _capsuleRadius;
            var point2 = capsuleCenter - Vector3.up * _capsuleHeight;
            
            
        }
    }


    private Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 bulletPos)
    {
        var ab = b - a;
        var t = Vector3.Dot(bulletPos - a, ab) / Vector3.Dot(ab, ab);
        return a + Mathf.Clamp01(t) * ab;
    }
    public class PlayerState
    {
        public Vector3 Position;
    }
}
