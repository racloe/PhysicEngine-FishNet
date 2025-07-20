using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

public class PredictionMotor : NetworkBehaviour
{
    private struct MoveData
    {
        public float Horizontal;
        public float Vertical;

        public MoveData(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }
    }

    private struct ReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }

    [SerializeField] private float movementForce;
    [SerializeField] private float torqueForce;

    private Rigidbody _rigidbody;
    private bool _subscribed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        SubscribeToTimeManager(true);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

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

    private void OnTick()
    {
        if (IsOwner)
        {
            Reconciliation(default, false);

            MoveData data;
            GatherInputs(out data);

            Move(data, false);
        }

        if (IsServerInitialized)
        {
            Move(default, true);
        }
    }

    private void OnPostTick()
    {
        ReconcileData data = new ReconcileData(transform.position, transform.rotation, _rigidbody.linearVelocity, _rigidbody.angularVelocity);

        Reconciliation(data, true);
    }


    [Reconcile]
    private void Reconciliation(ReconcileData data, bool asServer)
    {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        _rigidbody.linearVelocity = data.Velocity;
        _rigidbody.angularVelocity = data.AngularVelocity;
    }

    [Replicate]
    private void Move(MoveData data, bool asServer, bool replaying = false)
    {
        _rigidbody.AddForce(transform.forward * (data.Vertical * movementForce), ForceMode.Acceleration);

        _rigidbody.AddTorque(transform.up * (data.Horizontal * torqueForce), ForceMode.Acceleration);
    }

    private void GatherInputs(out MoveData data)
    {
        data = default;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        if (horizontal == 0f && vertical == 0f)
            return;

        data = new MoveData(horizontal, vertical);
    }

    private void OnDestroy()
    {
        SubscribeToTimeManager(false);
    }

}
