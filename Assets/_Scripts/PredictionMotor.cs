using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

public class PredictionMotor : NetworkBehaviour
{
    public struct ControlData : IReplicateData
    {
        public readonly float Horizontal;
        public readonly float Vertical;
        public readonly bool Fire;
        private uint _tick;

        public ControlData(float horizontal, float vertical, bool fire) : this()
        {
            Horizontal = horizontal;
            Vertical = vertical;
            Fire = fire;
            _tick = 0;
        }
        public void Dispose(){}
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Velocity;
        public readonly Vector3 AngularVelocity;
        private uint _tick;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) : this()
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            _tick = 0;
        }
        
        public void Dispose(){}
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [SerializeField] private float movementForce;
    [SerializeField] private float torqueForce;
    [SerializeField] private float recoilForce;

    [SerializeField] private float hitRadius;
    [SerializeField] private float hitPushForce;
    
    public static Dictionary<int, PredictionMotor> AllTanks = new ();

    private Rigidbody _rigidbody;
    private bool _subscribed;
    
    private float _verticalInput;
    private float _horizontalInput;
    private bool _fire;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        AllTanks.Add(Owner.ClientId, this);
    }

    public override void OnStartNetwork()
    {
        SubscribeToTimeManager(true);
    }
    

    private void SubscribeToTimeManager(bool subscribe)
    {
        if (TimeManager == null)
            return;

        if (subscribe == _subscribed)
            return;

        _subscribed = subscribe;

        if (subscribe)
        {
            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
        }
        else
        {
            TimeManager.OnTick -= OnTick;
            TimeManager.OnPostTick -= OnPostTick;
        }
        
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        if(!_fire)
            _fire = Input.GetMouseButtonDown(0);
    }


    private void OnTick()
    {
        if (IsOwner)
        {
            // GatherInputs(out var data);
            ControlData data = new ControlData(_horizontalInput, _verticalInput, _fire);
            
            Move(data);
        }

        if (IsServerInitialized)
        {
            Move(default);
        }
    }

    private void OnPostTick()
    {
        CreateReconcile();
    }


    [Reconcile]
    private void Reconciliation(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        _rigidbody.linearVelocity = data.Velocity;
        _rigidbody.angularVelocity = data.AngularVelocity;
    }

    [Replicate]
    private void Move(ControlData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        _rigidbody.AddForce(transform.forward * (data.Vertical * movementForce), ForceMode.Acceleration);

        _rigidbody.AddTorque(transform.up * (data.Horizontal * torqueForce), ForceMode.Acceleration);

        if (data.Fire)
        {
            _rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
            _fire = false;
        }

        var isAuthoritative = IsServerInitialized;
        var isPredicted = IsOwner;

        if (isAuthoritative || isPredicted)
        {
            foreach (var other in AllTanks.Values)
            {
                if(other == this) continue;
                
                // if(InstanceId() > other.InstanceId())
                //     continue;
                
                var myPos = transform.position;
                var otherPos = other.transform.position;
                var distance = Vector3.Distance(myPos, otherPos);

                if (distance < hitRadius)
                {
                    var dir = (myPos - otherPos).normalized;
                    dir.y = 0;
                    _rigidbody.AddForce(dir * hitPushForce, ForceMode.Impulse);
                    // if(isAuthoritative)
                    //     other._rigidbody.AddForce(-dir * hitPushForce, ForceMode.Impulse);
                }
            }
        }
    }

    private int InstanceId()
    {
        return GetInstanceID();
    }


    public override void CreateReconcile()
    {
        var data = new ReconcileData(
            transform.position,
            transform.rotation,
            _rigidbody.linearVelocity,
            _rigidbody.angularVelocity);
        Reconciliation(data);
    }

    public override void OnStopNetwork()
    {
        SubscribeToTimeManager(false);
        AllTanks.Remove(Owner.ClientId);
    }

    private void OnDestroy()
    {
        SubscribeToTimeManager(false);
        AllTanks.Remove(Owner.ClientId);
    }

}
