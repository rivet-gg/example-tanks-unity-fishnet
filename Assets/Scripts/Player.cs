using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Player : NetworkBehaviour
{
    public float speed = 5.0f;
    private CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    [Client(RequireOwnership = true)]
    void Move() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 offset = new Vector3(horizontalInput, Physics.gravity.y, verticalInput) * speed * Time.deltaTime;
        cc.Move(offset);
    }
}
