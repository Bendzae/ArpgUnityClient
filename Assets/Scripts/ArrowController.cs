using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private ParticleSystem impactEffect;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.magnitude > 100)
        {
            this.enabled = false;
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        transform.rotation = Quaternion.identity;
        impactEffect.Play();
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.GetComponent<MeshRenderer>().enabled = false;
        Destroy(this.gameObject, 1f);
    }
}