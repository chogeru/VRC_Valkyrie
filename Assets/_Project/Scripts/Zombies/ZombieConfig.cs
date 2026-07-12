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
    [Tooltip("How often (seconds) this zombie re-picks its nearest-player target.")]
    public float retargetInterval = 1f;
    [Tooltip("How long the corpse stays visible/active after death before the pool slot is freed for reuse.")]
    public float corpseLingerDuration = 3f;

    [Header("Score")]
    public int scoreValue = 10;

    [Header("Voice Audio (optional)")]
    [Tooltip("Played when this zombie spots/attacks a player.")]
    public AudioClip[] attackClips;
    [Tooltip("Played when this zombie takes damage but survives.")]
    public AudioClip[] damageClips;
    [Tooltip("Played once when this zombie dies.")]
    public AudioClip[] deathClips;
    [Tooltip("Random ambient grunt/breathing while chasing - optional flavor.")]
    public AudioClip[] idleClips;
    [Range(0f, 1f)]
    public float idleClipChancePerRetarget = 0.15f;
}
