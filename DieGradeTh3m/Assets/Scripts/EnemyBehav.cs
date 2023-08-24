using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]  
public class EnemyBehav : MonoBehaviour
{
    [SerializeField] AudioClip[] damagedVoiceClips;
    private AudioSource auSo;
    private Animator animator;

    private void Awake()
    {
        auSo = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public void Damaged()
    {
        animator.SetTrigger("Hit");
        auSo.PlayOneShot(damagedVoiceClips[Random.Range(0, damagedVoiceClips.Length)]);
    }
}
