using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    
    [HideInInspector] public int ownerId;
    private Vector3 _direction;
    private float _force;

    [SerializeField] private float damageForce;
    [SerializeField] private float trailForce;
    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 dir, float force, int ownerPlayerId)
    {
        _direction = dir;
        _force = force;
        ownerId = ownerPlayerId;
    }


    private void Update()
    {
        // transform.position += _direction * _force * Time.deltaTime;
        _rigidbody.AddForce(_direction * _force, ForceMode.Acceleration);
        CheckTankAndApplyForce();
    }
    
    private void CheckTankAndApplyForce()
    {
        foreach (var tank in PredictionMotor.AllTanks.Values)
        {
            if(tank.Owner.ClientId == ownerId)
                continue;
            var tankPos = tank.transform.position;
            var distance = Vector3.Distance(transform.position, tankPos);
            if (distance < 5)
            {
                var dir = (tankPos - transform.position).normalized;
                var force = 5 / distance * trailForce;
                _rigidbody.AddForce(dir * force, ForceMode.Acceleration);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServerInitialized) return;

        if (other.CompareTag("Tank"))
        {
            if (ownerId == other.GetComponent<BulletFire>().OwnerId) return;
            var dir = (other.transform.position - transform.position).normalized;
            dir.y = 0;
            other.GetComponent<Rigidbody>().AddForce(dir * damageForce, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
