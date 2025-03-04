using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public CapsuleCollider capsuleCollider;
    void Start()
    {
        if (NetworkManager.instance != null)
        {
            capsuleCollider.enabled = false;
        }
    }
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        Move();
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    private void Move()
    {
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_position">The new position.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(Vector3 _position, Quaternion _rotation)
    {
        transform.position = _position;
        transform.rotation = _rotation;
    }
}
