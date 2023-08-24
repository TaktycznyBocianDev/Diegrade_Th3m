using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] WeaponHolder holder;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && holder.isAttacking)
        {
            other.GetComponent<EnemyBehav>().Damaged();
        }
    }
}
