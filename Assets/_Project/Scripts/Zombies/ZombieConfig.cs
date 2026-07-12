using UdonSharp;
using UnityEngine;

// Data container describing one zombie archetype's base stats. Duplicate
// this asset to create new zombie types (fast/tanky/etc.) and assign
// different ones to individual pool entries in the future if needed.
public class ZombieConfig : UdonSharpBehaviour
{
    [Header("Identity")]
    public string zombieName = "Walker";

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2.2f;
    public float attackDamage = 10f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.2f;

    [Header("Score")]
    public int scoreValue = 10;
}
