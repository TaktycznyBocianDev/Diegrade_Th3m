using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBehav : MonoBehaviour
{
    [SerializeField] GameObject sword;
    [SerializeField] float attackCooldown;
    [SerializeField] AudioClip attackSound;
    [SerializeField] ParticleSystem trial, glow;
    private bool canAttack = true;
    Animator animator;
    AudioSource audioSource;



    private void Awake()
    {
        animator = sword.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canAttack)
            {
                Attack();
            }
        }
    }

    private void Attack()
    {
        canAttack = false;
        animator.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSound);
        var emTrial = trial.emission;
        emTrial.enabled = true;
        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        var emTrial = trial.emission;
        emTrial.enabled = false;
        animator.Rebind();
        canAttack = true;
    }

}




