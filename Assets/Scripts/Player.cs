using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Player : NetworkBehaviour
{
    public float speed = 5.0f;
    private CharacterController _cc;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    [Client(RequireOwnership = true)]
    void Move() {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        var offset = new Vector3(horizontalInput, Physics.gravity.y, verticalInput) * speed * Time.deltaTime;
        _cc.Move(offset);
    }
}
