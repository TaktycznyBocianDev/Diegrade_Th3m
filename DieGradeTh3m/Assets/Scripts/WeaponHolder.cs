using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] GameObject sword;
    [SerializeField] float attackCooldown;
    [SerializeField] float animationLenght;
    [SerializeField] AudioClip attackSound;
    [SerializeField] ParticleSystem trial;

    public bool isAttacking = false;
    
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
        isAttacking = true;
        animator.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSound);
        var emTrial = trial.emission;
        emTrial.enabled = true;
        StartCoroutine(ResetAttackCooldown());
        StartCoroutine(ResetIsAttacing());
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        var emTrial = trial.emission;
        emTrial.enabled = false;
        animator.Rebind();
        canAttack = true;
    }

    IEnumerator ResetIsAttacing()
    {
        yield return new WaitForSeconds(animationLenght);
        isAttacking = false;
    }

}




