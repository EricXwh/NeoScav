using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Block : MonoBehaviour
{
    private Vector3 mergeAxis = Vector3.zero;
    private bool isMerging = false; 

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
        }

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = Vector3.one; 

        gameObject.layer = LayerMask.NameToLayer("Block");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Block otherBlock = collision.gameObject.GetComponent<Block>();
        if (otherBlock != null)
        {
            if (!isMerging && !otherBlock.isMerging)
            {
                if (CanMergeWith(otherBlock))
                {
                    isMerging = true;
                    otherBlock.isMerging = true;

                    MergeWith(otherBlock);
                }
            }
        }
    }

    private bool CanMergeWith(Block other)
    {
        Vector3 thisSize = transform.localScale;
        Vector3 otherSize = other.transform.localScale;
        if (!thisSize.Equals(otherSize))
            return false;

        Vector3 difference = other.transform.position - transform.position;

        Vector3 normalizedDifference = new Vector3(
            difference.x / thisSize.x,
            difference.y / thisSize.y,
            difference.z / thisSize.z
        );

        float tolerance = 0.75f; 

        bool canMerge = false;
        Vector3 detectedMergeAxis = Vector3.zero;

        if (Mathf.Abs(normalizedDifference.x) >= 0.9f && Mathf.Abs(normalizedDifference.x) <= 1.1f &&
            Mathf.Abs(normalizedDifference.y) <= tolerance &&
            Mathf.Abs(normalizedDifference.z) <= tolerance)
        {
            canMerge = true;
            detectedMergeAxis = Vector3.right;
        }
        else if (Mathf.Abs(normalizedDifference.y) >= 0.9f && Mathf.Abs(normalizedDifference.y) <= 1.1f &&
                 Mathf.Abs(normalizedDifference.x) <= tolerance &&
                 Mathf.Abs(normalizedDifference.z) <= tolerance)
        {
            canMerge = true;
            detectedMergeAxis = Vector3.up;
        }
        else if (Mathf.Abs(normalizedDifference.z) >= 0.9f && Mathf.Abs(normalizedDifference.z) <= 1.1f &&
                 Mathf.Abs(normalizedDifference.x) <= tolerance &&
                 Mathf.Abs(normalizedDifference.y) <= tolerance)
        {
            canMerge = true;
            detectedMergeAxis = Vector3.forward;
        }

        if (canMerge)
        {
            this.mergeAxis = detectedMergeAxis;
            other.mergeAxis = detectedMergeAxis;
        }

        return canMerge;
    }

    private void MergeWith(Block other)
    {
        Vector3 axis = this.mergeAxis;

        Vector3 thisSize = transform.localScale;
        Vector3 otherSize = other.transform.localScale;

        Vector3 newSize = thisSize;
        Vector3 newPosition = transform.position;

        if (axis == Vector3.right)
        {
            newSize.x += otherSize.x;
            newPosition = (transform.position + other.transform.position) / 2;
        }
        else if (axis == Vector3.up)
        {
            newSize.y += otherSize.y;
            newPosition = (transform.position + other.transform.position) / 2;
        }
        else if (axis == Vector3.forward)
        {
            newSize.z += otherSize.z;
            newPosition = (transform.position + other.transform.position) / 2;
        }
        else
        {
            Debug.LogWarning("Unsupported merge axis.");
            return;
        }

        GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newBlock.transform.position = newPosition;
        newBlock.transform.localScale = newSize;

        newBlock.layer = LayerMask.NameToLayer("Block");

        Block newBlockScript = newBlock.AddComponent<Block>();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            newBlock.GetComponent<Renderer>().material = renderer.material;
        }

        Rigidbody newRb = newBlock.GetComponent<Rigidbody>();
        if (newRb != null)
        {
            newRb.isKinematic = false;
            newRb.velocity = Vector3.zero;
            newRb.angularVelocity = Vector3.zero;
        }

        BoxCollider newCollider = newBlock.GetComponent<BoxCollider>();
        if (newCollider != null)
        {
            newCollider.size = Vector3.one; 
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }


}
