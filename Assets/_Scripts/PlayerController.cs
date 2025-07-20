using System;
using FishNet.Object;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float movementForce;
    [SerializeField] private float torqueForce;
    
    private Rigidbody _rigidbody;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsClientInitialized) 
            return;
        
        if (!IsOwner) 
            return;
        
        var movement = Input.GetAxis("Vertical");
        _rigidbody.AddForce(transform.forward * (movement * movementForce), ForceMode.Acceleration);
        
        var turning = Input.GetAxis("Horizontal");
        _rigidbody.AddTorque(transform.up * (turning * torqueForce), ForceMode.Acceleration);
    }
    
}
