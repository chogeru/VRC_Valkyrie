
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Sparrow_Wait : UdonSharpBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();

        animator.Play("Base Layer.Armature|Wait", 0, Random.value);
        animator.speed = Random.Range(0.8f, 1.2f);
    }
}
