using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    public bool DEBUG = false;
    public float waterVelocityDrag = 0.99f;
    public float waterAngularDrag = 0.5f;
    public FFTOcean ocean;

    [Range(1, 5)] public float strength;
    [Range(1, 5)] public float objectDepth;

    #region Private Fields
    int floaters;
    [HideInInspector]
    public Rigidbody rB;
    #endregion

    private void Start()
    {
        rB = GetComponentInParent<Rigidbody>();
        floaters = transform.parent.childCount;

        if (rB.useGravity)
            rB.useGravity = false;
    }

    private void FixedUpdate()
    {
        float wH = ocean.GetWaterHeight(transform.position) * 
            ((ocean.transform.GetComponent<PhillipsSpectrum>().windSpeed * ocean.transform.GetComponent<PhillipsSpectrum>().windSpeed) / ocean.transform.GetComponent<PhillipsSpectrum>().gravity);
        if (DEBUG)
            Debug.Log(wH);

        // Manual gravity subdivided based on the amount of floaters.
        rB.AddForce(Physics.gravity * rB.mass / floaters);

        // If the floater is below water
        if (transform.position.y <= wH)
        {
            float submersion = Mathf.Clamp01(wH - transform.position.y) / objectDepth;
            float buoyancy = Mathf.Abs(Physics.gravity.y) * submersion * strength;

            // Buoyant Force
            rB.AddForceAtPosition(Vector3.up * buoyancy, transform.position, ForceMode.Acceleration);

            // Drag Force
            rB.AddForce(-rB.velocity * waterVelocityDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);

            // Torque Force
            rB.AddTorque(-rB.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
