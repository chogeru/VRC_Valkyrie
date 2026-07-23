using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// Owner-authoritative pet dog brain. Only the current owner runs RunAi() -
// every other client just watches the transform (via VRC Object Sync on
// this GameObject) and the synced ActionState animator param arrive
// through OnDeserialization, same shape as ZombieAI/GameManager elsewhere
// in this project. The per-client "Speed" animator param is NOT synced -
// every client (owner included) derives it locally from how far the
// transform actually moved last frame, which tracks the interpolated
// Object Sync position on remote clients for free.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DogAI : UdonSharpBehaviour
{
    // Mirrors ShibaInu_Gameplay.controller's int "ActionState" parameter.
    public const int ACTION_NONE = 0;
    public const int ACTION_SIT = 1;
    public const int ACTION_LIE = 2;
    public const int ACTION_EAT = 3;
    public const int ACTION_DRINK = 4;
    public const int ACTION_DIG = 5;
    public const int ACTION_CARRY_BALL = 6;
    public const int ACTION_SLEEP = 7;
    public const int ACTION_SCRATCH = 8;
    public const int ACTION_TREAT = 9;

    // Local-only high level task the owner is currently running.
    private const int TASK_IDLE = 0;
    private const int TASK_WANDER = 1;
    private const int TASK_GO_TO_BALL = 2;
    private const int TASK_RETURN_BALL = 3;
    private const int TASK_GO_EAT = 4;
    private const int TASK_EATING = 5;
    private const int TASK_GO_DRINK = 6;
    private const int TASK_DRINKING = 7;
    private const int TASK_GO_SLEEP = 8;
    private const int TASK_SLEEPING = 9;
    private const int TASK_GO_TO_BONE = 10;
    private const int TASK_CHEWING = 11;
    private const int TASK_AGILITY = 12;
    private const int TASK_SIT_AMBIENT = 13;
    private const int TASK_LIE_AMBIENT = 14;
    private const int TASK_PET_REACTION = 15;
    private const int TASK_SCRATCH_AMBIENT = 16;
    private const int TASK_GO_TO_TREAT = 17;
    private const int TASK_EATING_TREAT = 18;

    // How long the one-shot Scratching clip actually plays (ShibaInu_anim_IP.fbx).
    private const float SCRATCH_CLIP_DURATION = 6.04f;

    [Header("Debug")]
    [Tooltip("Logs task/state transitions and a periodic status line to the console. Turn off before publishing.")]
    public bool debugLogging = true;
    private float nextDebugLogTime;

    [Header("Config")]
    public DogConfig config;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform mouthSocket;
    public AudioSource voiceAudioSource;

    [Header("Toys")]
    [Tooltip("The fetch ball - DogAI polls ball.wasThrown/heldByPlayer directly each tick.")]
    public DogBall ball;
    [Tooltip("The chew bone - DogAI polls toy.wasGiven directly each tick.")]
    public DogToy toy;
    [Tooltip("A treat prop - DogAI polls treat.wasOffered/heldByPlayer directly each tick, same shape as ball.")]
    public DogTreat treat;

    [Header("Home / Rest")]
    public Transform homeCenter;
    public Transform sleepPoint;

    [Header("Feeding")]
    public FoodBowl foodBowl;
    public WaterBowl waterBowl;

    [Header("Agility Course (visiting order)")]
    public Transform[] agilityWaypoints;
    [Tooltip("Parallel to agilityWaypoints - true fires the Jump animation trigger shortly before reaching that waypoint. Set directly rather than read via GetComponent<AgilityWaypoint>() at runtime, since cross-UdonSharpBehaviour GetComponent<T>() calls during Start() are unreliable due to Udon init ordering.")]
    public bool[] agilityIsJumpPoints;

    [UdonSynced] private int syncedActionState;
    [UdonSynced] private float syncedHunger = 1f;
    [UdonSynced] private float syncedThirst = 1f;
    [UdonSynced] private float syncedEnergy = 1f;
    [UdonSynced] private float syncedAffection = 0.2f;

    private int lastAppliedActionState = -1;
    private int task = TASK_IDLE;

    private Vector3 lastPos;
    private float currentAnimSpeed;
    private float nextNeedsSyncTime;

    private float nextWanderTime;
    private float nextAgilityTime;
    private float taskEndTime;

    private float nextGreetCheckTime;
    private float nextGreetTime;

    private DogBall targetBall;
    private DogBall carriedBall;
    private DogTreat targetTreat;

    private VRCPlayerApi returnBallTarget;
    private float nextReturnTargetRefresh;

    private Transform toyTarget;

    private Vector3 lastSetDestination;
    private bool hasSetDestination;

    private int agilityIndex;
    private bool agilityJumpFiredForCurrentLeg;

    // --- Animation self-diagnostics -----------------------------------------
    // Watches all four leg bones every frame and SCREAMS in the console
    // (LogWarning) the moment it detects Speed>0 without ANY of them
    // actually moving, along with a full dump of every setting that could
    // plausibly cause that. Tracking all four (and requiring ALL of them to
    // be still) instead of just one is deliberate: a single leg legitimately
    // holds still for a few hundred ms during its own gait stance phase -
    // monitoring only leg_f.L produced false "frozen" alarms during that
    // phase even while the dog was visibly walking normally on the other
    // three legs. Always active (not gated on debugLogging) so it can't be
    // accidentally left off during a repro.
    private Transform[] diagLegBones;
    private Quaternion[] lastLegBoneRot;
    private int lastAnimStateHash = -1;
    private float boneFrozenSince = -1f;
    private float nextAnimDiagLogTime;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        lastPos = transform.position;

        if (agent != null && config != null)
        {
            agent.angularSpeed = config.angularSpeed;
            agent.acceleration = config.acceleration;
        }

        string[] legBoneNames = new string[] { "leg_f.L", "leg_f.R", "leg_b.L", "leg_b.R" };
        diagLegBones = new Transform[legBoneNames.Length];
        lastLegBoneRot = new Quaternion[legBoneNames.Length];
        for (int i = 0; i < legBoneNames.Length; i++)
        {
            diagLegBones[i] = FindByName(transform, legBoneNames[i]);
            if (diagLegBones[i] != null) lastLegBoneRot[i] = diagLegBones[i].localRotation;
        }

        ScheduleNextWander();
        ScheduleNextAgility();
        ApplyActionStateLocal();

        if (debugLogging)
        {
            Debug.Log("[DogAI] Start: config=" + (config != null) + " agent=" + (agent != null) +
                " agent.isOnNavMesh=" + (agent != null && agent.isOnNavMesh) +
                " animator=" + (animator != null) + " mouthSocket=" + (mouthSocket != null) +
                " foodBowl=" + (foodBowl != null) + " waterBowl=" + (waterBowl != null) +
                " treat=" + (treat != null) +
                " agilityWaypoints=" + (agilityWaypoints != null ? agilityWaypoints.Length : -1) +
                " isOwner=" + Networking.IsOwner(gameObject) +
                " diagLegBones=" + (diagLegBones != null && diagLegBones[0] != null) +
                " animCullingMode=" + (animator != null ? animator.cullingMode.ToString() : "n/a") +
                " animApplyRootMotion=" + (animator != null && animator.applyRootMotion));
        }
    }

    // GetComponentsInChildren-based lookup instead of a recursive walk -
    // Udon has historically not supported recursive UdonSharp method calls
    // reliably, so avoid that shape entirely here.
    private Transform FindByName(Transform root, string name)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name) return all[i];
        }
        return null;
    }

    void Update()
    {
        DebugTick();
    }

    public void DebugTick()
    {
        UpdateAnimSpeedFromMovement();
        LogAnimDiagnostics();

        if (debugLogging && Time.time >= nextDebugLogTime)
        {
            nextDebugLogTime = Time.time + 2f;
            Debug.Log("[DogAI] status task=" + TaskName(task) + " pos=" + transform.position +
                " Speed=" + currentAnimSpeed + " ActionState=" + syncedActionState +
                " isOwner=" + Networking.IsOwner(gameObject) +
                " onNavMesh=" + (agent != null && agent.isOnNavMesh) +
                " agentEnabled=" + (agent != null && agent.enabled) +
                " hunger=" + syncedHunger + " thirst=" + syncedThirst + " energy=" + syncedEnergy);
        }

        if (!Networking.IsOwner(gameObject)) return;

        DecayNeeds();
        RunAi();

        if (Time.time >= nextNeedsSyncTime)
        {
            nextNeedsSyncTime = Time.time + 1.5f;
            RequestSerialization();
        }
    }

    private string TaskName(int t)
    {
        if (t == TASK_IDLE) return "IDLE";
        if (t == TASK_WANDER) return "WANDER";
        if (t == TASK_GO_TO_BALL) return "GO_TO_BALL";
        if (t == TASK_RETURN_BALL) return "RETURN_BALL";
        if (t == TASK_GO_EAT) return "GO_EAT";
        if (t == TASK_EATING) return "EATING";
        if (t == TASK_GO_DRINK) return "GO_DRINK";
        if (t == TASK_DRINKING) return "DRINKING";
        if (t == TASK_GO_SLEEP) return "GO_SLEEP";
        if (t == TASK_SLEEPING) return "SLEEPING";
        if (t == TASK_GO_TO_BONE) return "GO_TO_BONE";
        if (t == TASK_CHEWING) return "CHEWING";
        if (t == TASK_AGILITY) return "AGILITY";
        if (t == TASK_SIT_AMBIENT) return "SIT_AMBIENT";
        if (t == TASK_LIE_AMBIENT) return "LIE_AMBIENT";
        if (t == TASK_PET_REACTION) return "PET_REACTION";
        if (t == TASK_SCRATCH_AMBIENT) return "SCRATCH_AMBIENT";
        if (t == TASK_GO_TO_TREAT) return "GO_TO_TREAT";
        if (t == TASK_EATING_TREAT) return "EATING_TREAT";
        return "UNKNOWN(" + t + ")";
    }

    private void UpdateAnimSpeedFromMovement()
    {
        float dt = Time.deltaTime;
        float instSpeed;

        // Prefer the NavMeshAgent's own engine-tracked velocity - it's smooth
        // and correct every frame regardless of this script's own tick timing.
        // The previous approach (diffing transform.position against a
        // remembered lastPos) produced spurious huge spikes whenever this
        // script's Update() was skipped/delayed for a frame (small dt divided
        // into a position delta that had actually accumulated over several
        // real frames), which threw off the locomotion blend tree's timing.
        // Only the owner's agent is ever given a destination (RunAi() below
        // is owner-gated), so on every other client agent.velocity sits at 0
        // forever even while VRC Object Sync visibly moves the transform -
        // those clients must fall back to position-diffing instead, or the
        // dog reads as frozen in Idle for everyone except the owner.
        if (Networking.IsOwner(gameObject) && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            instSpeed = agent.velocity.magnitude;
        }
        else
        {
            float maxPlausibleSpeed = config != null ? config.fastRunSpeed * 1.5f : 10f;
            instSpeed = dt > 0f ? Mathf.Min((transform.position - lastPos).magnitude / dt, maxPlausibleSpeed) : 0f;
        }
        lastPos = transform.position;

        currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, instSpeed, 1f - Mathf.Exp(-10f * dt));
        if (animator != null) animator.SetFloat("Speed", currentAnimSpeed);
    }

    // See the "Animation self-diagnostics" field block for why this exists.
    // Runs every frame, unconditionally, regardless of debugLogging.
    private void LogAnimDiagnostics()
    {
        if (animator == null) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.fullPathHash != lastAnimStateHash)
        {
            Debug.Log("[DogAI][ANIM] STATE CHANGE at t=" + Time.time.ToString("F2") +
                " newStateHash=" + info.fullPathHash + " task=" + TaskName(task) +
                " ActionState=" + syncedActionState + " isOwner=" + Networking.IsOwner(gameObject));
            lastAnimStateHash = info.fullPathHash;
        }

        // Max delta across all four legs - a real freeze means every leg is
        // static, not just the one currently in its stance phase.
        float boneDelta = -1f;
        bool haveLegBones = diagLegBones != null && diagLegBones[0] != null;
        if (haveLegBones)
        {
            boneDelta = 0f;
            for (int i = 0; i < diagLegBones.Length; i++)
            {
                if (diagLegBones[i] == null) continue;
                float d = Quaternion.Angle(lastLegBoneRot[i], diagLegBones[i].localRotation);
                if (d > boneDelta) boneDelta = d;
                lastLegBoneRot[i] = diagLegBones[i].localRotation;
            }
        }

        bool meaningfullyMoving = currentAnimSpeed > 0.3f;
        if (haveLegBones && meaningfullyMoving && boneDelta < 0.03f)
        {
            if (boneFrozenSince < 0f) boneFrozenSince = Time.time;
            if (Time.time - boneFrozenSince > 0.5f)
            {
                Debug.LogWarning("[DogAI][ANIM-FREEZE-DETECTED] all 4 leg bones static for " +
                    (Time.time - boneFrozenSince).ToString("F2") + "s while Speed=" + currentAnimSpeed.ToString("F2") +
                    " task=" + TaskName(task) + " ActionState=" + syncedActionState +
                    " isOwner=" + Networking.IsOwner(gameObject) +
                    " animEnabled=" + animator.enabled + " animatorGOActive=" + animator.gameObject.activeInHierarchy +
                    " cullingMode=" + animator.cullingMode + " animatorSpeed=" + animator.speed +
                    " applyRootMotion=" + animator.applyRootMotion +
                    " controllerId=" + (animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.GetInstanceID() : 0) +
                    " carrying=" + (carriedBall != null) + " normalizedTime=" + info.normalizedTime.ToString("F2") +
                    " inTransition=" + animator.IsInTransition(0) +
                    " agentVel=" + (agent != null ? agent.velocity.magnitude.ToString("F2") : "n/a"));
                // Only warn once per continuous freeze, not every single frame it persists.
                boneFrozenSince = Time.time - 0.4f;
            }
        }
        else
        {
            boneFrozenSince = -1f;
        }

        if (debugLogging && Time.time >= nextAnimDiagLogTime)
        {
            nextAnimDiagLogTime = Time.time + 1f;
            Debug.Log("[DogAI][ANIM] t=" + Time.time.ToString("F2") + " task=" + TaskName(task) +
                " ActionState=" + syncedActionState + " Speed=" + currentAnimSpeed.ToString("F2") +
                " normTime=" + info.normalizedTime.ToString("F2") + " boneDelta=" + boneDelta.ToString("F3") +
                " isOwner=" + Networking.IsOwner(gameObject) + " carrying=" + (carriedBall != null) +
                " animEnabled=" + animator.enabled + " cullMode=" + animator.cullingMode);
        }
    }

    private void DecayNeeds()
    {
        if (config == null) return;
        float dt = Time.deltaTime;

        if (task == TASK_SLEEPING) syncedEnergy = Mathf.Clamp01(syncedEnergy + config.energyRegenPerSecond * dt);
        else syncedEnergy = Mathf.Clamp01(syncedEnergy - config.energyDecayPerSecond * dt);

        if (task != TASK_EATING) syncedHunger = Mathf.Clamp01(syncedHunger - config.hungerDecayPerSecond * dt);
        if (task != TASK_DRINKING) syncedThirst = Mathf.Clamp01(syncedThirst - config.thirstDecayPerSecond * dt);

        syncedAffection = Mathf.Clamp01(syncedAffection - config.affectionDecayPerSecond * dt);
    }

    private void RunAi()
    {
        if (carriedBall != null && mouthSocket != null)
        {
            carriedBall.transform.SetPositionAndRotation(mouthSocket.position, mouthSocket.rotation);
        }

        // An uncaught exception anywhere in this Udon program permanently
        // halts the whole behaviour with no visible error, so guard the one
        // condition that would make every SetDestination call below misbehave:
        // the agent having been knocked off (or never placed on) the NavMesh.
        if (agent == null || !agent.isOnNavMesh)
        {
            if (task != TASK_SIT_AMBIENT && task != TASK_LIE_AMBIENT && task != TASK_PET_REACTION)
            {
                if (debugLogging && Time.time >= nextDebugLogTime - 1.9f) Debug.LogWarning("[DogAI] RunAi blocked: agent=" + (agent != null) + " isOnNavMesh=" + (agent != null && agent.isOnNavMesh) + " pos=" + transform.position);
                return;
            }
        }

        if (task == TASK_GO_TO_BALL) { TickGoToBall(); return; }
        if (task == TASK_RETURN_BALL) { TickReturnBall(); return; }
        if (task == TASK_GO_TO_TREAT) { TickGoToTreat(); return; }
        if (task == TASK_EATING_TREAT) { TickEatingTreat(); return; }
        if (task == TASK_GO_EAT) { TickGoEat(); return; }
        if (task == TASK_EATING) { TickEating(); return; }
        if (task == TASK_GO_DRINK) { TickGoDrink(); return; }
        if (task == TASK_DRINKING) { TickDrinking(); return; }
        if (task == TASK_GO_SLEEP) { TickGoSleep(); return; }
        if (task == TASK_SLEEPING) { TickSleeping(); return; }
        if (task == TASK_GO_TO_BONE) { TickGoToBone(); return; }
        if (task == TASK_CHEWING) { TickChewing(); return; }
        if (task == TASK_AGILITY) { TickAgility(); return; }
        if (task == TASK_SIT_AMBIENT || task == TASK_LIE_AMBIENT || task == TASK_PET_REACTION || task == TASK_SCRATCH_AMBIENT) { TickTimedIdleReturn(); return; }

        // Free to pick a new priority task, highest first.
        bool foodReady = foodBowl != null && foodBowl.filled;
        bool waterReady = waterBowl != null && waterBowl.filled;
        if (debugLogging && (syncedHunger < 0.4f || syncedThirst < 0.4f || syncedEnergy < 0.4f) && Time.time >= nextDebugLogTime - 1.9f)
        {
            Debug.Log("[DogAI] priority-check hunger=" + syncedHunger + " thirst=" + syncedThirst + " energy=" + syncedEnergy +
                " foodReady=" + foodReady + " waterReady=" + waterReady);
        }

        if (ball != null && ball.wasThrown && !ball.heldByPlayer)
        {
            targetBall = ball;
            ball.wasThrown = false;
            if (debugLogging) Debug.Log("[DogAI] Ball throw detected at " + ball.transform.position);
            BeginGoToBall();
            return;
        }
        if (treat != null && treat.wasOffered && !treat.heldByPlayer)
        {
            targetTreat = treat;
            treat.wasOffered = false;
            if (debugLogging) Debug.Log("[DogAI] Treat offered at " + treat.transform.position);
            BeginGoToTreat();
            return;
        }
        if (config != null && syncedHunger < config.needThreshold && foodReady) { BeginGoEat(); return; }
        if (config != null && syncedThirst < config.needThreshold && waterReady) { BeginGoDrink(); return; }
        if (config != null && syncedEnergy < config.needThreshold) { BeginGoSleep(); return; }
        if (toy != null && toy.wasGiven)
        {
            toyTarget = toy.transform;
            toy.wasGiven = false;
            if (debugLogging) Debug.Log("[DogAI] Toy given at " + toy.transform.position);
            BeginGoToBone();
            return;
        }
        if (Time.time >= nextAgilityTime && agilityWaypoints != null && agilityWaypoints.Length > 0) { BeginAgility(); return; }

        TickIdleWander();
    }

    // --- Fetch ------------------------------------------------------------

    private void BeginGoToBall()
    {
        task = TASK_GO_TO_BALL;
        hasSetDestination = false;
        if (agent != null && config != null)
        {
            agent.isStopped = false;
            agent.speed = config.fetchMoveSpeed;
            agent.SetDestination(targetBall.transform.position);
        }
        if (debugLogging) Debug.Log("[DogAI] BeginGoToBall dest=" + (targetBall != null ? targetBall.transform.position.ToString() : "null") + " agentOnMesh=" + (agent != null && agent.isOnNavMesh));
    }

    private void TickGoToBall()
    {
        if (targetBall == null) { task = TASK_IDLE; ScheduleNextWander(); return; }
        if (agent == null) return;

        if (targetBall.heldByPlayer)
        {
            // A player grabbed it out of the air/off the ground - abandon the fetch.
            targetBall = null;
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
            return;
        }

        SetDestinationIfMoved(targetBall.transform.position);
        if (!agent.pathPending && agent.remainingDistance <= config.ballPickupDistance)
        {
            if (!Networking.IsOwner(targetBall.gameObject)) Networking.SetOwner(Networking.LocalPlayer, targetBall.gameObject);
            targetBall.SetCarried(true);
            carriedBall = targetBall;
            targetBall = null;
            // Deliberately NOT setting ActionState=CarryBall here - that state
            // is a static held-still pose (Pick_up_idle) with no movement
            // blend, so using it for the whole run back left the dog's legs
            // frozen while it visibly moved. Locomotion (ActionState=None)
            // keeps animating normally; the ball itself follows the mouth
            // bone (see RunAi), which reads as "carrying" on its own.
            SetActionState(ACTION_NONE);
            task = TASK_RETURN_BALL;
            hasSetDestination = false;
            Bark();
            if (debugLogging) Debug.Log("[DogAI] Picked up ball, now returning.");
        }
    }

    private void TickReturnBall()
    {
        if (agent == null) return;

        // FindNearestPlayer() allocates a fresh VRCPlayerApi[] every call - fine
        // as an occasional lookup, but calling it every single Update() frame
        // for the whole return-the-ball walk (which can take several seconds)
        // meant a GC allocation every frame, which shows up as visible
        // stutter. Only re-pick the nearest player a few times a second;
        // GetPosition() on the cached reference every frame is free.
        if (Time.time >= nextReturnTargetRefresh || returnBallTarget == null || !returnBallTarget.IsValid())
        {
            nextReturnTargetRefresh = Time.time + 0.5f;
            returnBallTarget = FindNearestPlayer();
        }

        Vector3 destPos = homeCenter != null ? homeCenter.position : transform.position;
        if (returnBallTarget != null && returnBallTarget.IsValid()) destPos = returnBallTarget.GetPosition();
        SetDestinationIfMoved(destPos);

        if (!agent.pathPending && agent.remainingDistance <= config.ballReturnDistance)
        {
            DropCarriedBall();
        }
    }

    private void DropCarriedBall()
    {
        if (carriedBall != null)
        {
            carriedBall.SetCarried(false);
            carriedBall = null;
        }
        // ballReturnDistance's arrival tolerance can be a meter or more, so
        // without this the dog snaps into the static Sit pose while the
        // agent is still gliding the rest of the way to its destination -
        // it visibly skates across the ground while "sitting".
        if (agent != null) agent.isStopped = true;
        SetActionState(ACTION_SIT);
        task = TASK_SIT_AMBIENT;
        taskEndTime = Time.time + 2f;
        Bark();
        if (debugLogging) Debug.Log("[DogAI] Dropped ball at " + transform.position);
    }

    // --- Treat / Beg ---------------------------------------------------------

    private void BeginGoToTreat()
    {
        task = TASK_GO_TO_TREAT;
        hasSetDestination = false;
        if (agent != null && config != null)
        {
            agent.isStopped = false;
            agent.speed = config.fetchMoveSpeed;
            agent.SetDestination(targetTreat.transform.position);
        }
        if (debugLogging) Debug.Log("[DogAI] BeginGoToTreat dest=" + (targetTreat != null ? targetTreat.transform.position.ToString() : "null"));
    }

    private void TickGoToTreat()
    {
        if (targetTreat == null || agent == null) { task = TASK_IDLE; ScheduleNextWander(); return; }

        if (targetTreat.heldByPlayer)
        {
            // Picked back up before the dog arrived - abandon, same as a ball
            // grabbed mid-fetch (see TickGoToBall).
            targetTreat = null;
            task = TASK_IDLE;
            ScheduleNextWander();
            return;
        }

        SetDestinationIfMoved(targetTreat.transform.position);
        if (!agent.pathPending && agent.remainingDistance <= config.treatEatDistance)
        {
            if (!Networking.IsOwner(targetTreat.gameObject)) Networking.SetOwner(Networking.LocalPlayer, targetTreat.gameObject);
            targetTreat.Consume();
            FaceTarget(targetTreat.transform.position);
            // Same skating fix as DropCarriedBall/ReactToPet - stop the agent
            // before snapping into the static Treat pose, or the dog visibly
            // glides the last bit of remainingDistance while "frozen" eating.
            agent.isStopped = true;

            syncedHunger = Mathf.Clamp01(syncedHunger + config.treatHungerBoost);
            syncedAffection = Mathf.Clamp01(syncedAffection + config.affectionPerTreat);
            RequestSerialization();

            SetActionState(ACTION_TREAT);
            task = TASK_EATING_TREAT;
            taskEndTime = Time.time + config.treatEatDuration;
            targetTreat = null;
            Bark();
            if (debugLogging) Debug.Log("[DogAI] Ate treat, hunger=" + syncedHunger + " affection=" + syncedAffection);
        }
    }

    private void TickEatingTreat()
    {
        if (Time.time >= taskEndTime)
        {
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    // --- Feeding ------------------------------------------------------------

    // Called (void, no return value) by FoodBowl/WaterBowl whenever their
    // filled state changes, so this AI never needs to call back into them
    // for a bool return value.
    private void BeginGoEat()
    {
        task = TASK_GO_EAT;
        if (agent != null && config != null)
        {
            agent.isStopped = false;
            agent.speed = config.wanderMoveSpeed;
            agent.SetDestination(foodBowl.transform.position);
        }
    }

    private void TickGoEat()
    {
        if (foodBowl == null || agent == null) { task = TASK_IDLE; return; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
        {
            agent.isStopped = true;
            FaceTarget(foodBowl.transform.position);
            task = TASK_EATING;
            SetActionState(ACTION_EAT);
            taskEndTime = Time.time + config.eatDuration;
            if (debugLogging) Debug.Log("[DogAI] Arrived at food bowl, dist=" + Vector3.Distance(transform.position, foodBowl.transform.position));
        }
    }

    private void TickEating()
    {
        if (Time.time >= taskEndTime)
        {
            syncedHunger = 1f;
            if (foodBowl != null)
            {
                if (!Networking.IsOwner(foodBowl.gameObject)) Networking.SetOwner(Networking.LocalPlayer, foodBowl.gameObject);
                foodBowl.filled = false;
                foodBowl.RequestSerialization();
            }
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    private void BeginGoDrink()
    {
        task = TASK_GO_DRINK;
        if (agent != null && config != null)
        {
            agent.isStopped = false;
            agent.speed = config.wanderMoveSpeed;
            agent.SetDestination(waterBowl.transform.position);
        }
    }

    private void TickGoDrink()
    {
        if (waterBowl == null || agent == null) { task = TASK_IDLE; return; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
        {
            agent.isStopped = true;
            FaceTarget(waterBowl.transform.position);
            task = TASK_DRINKING;
            SetActionState(ACTION_DRINK);
            taskEndTime = Time.time + config.drinkDuration;
            if (debugLogging) Debug.Log("[DogAI] Arrived at water bowl, dist=" + Vector3.Distance(transform.position, waterBowl.transform.position));
        }
    }

    private void TickDrinking()
    {
        if (Time.time >= taskEndTime)
        {
            syncedThirst = 1f;
            if (waterBowl != null)
            {
                if (!Networking.IsOwner(waterBowl.gameObject)) Networking.SetOwner(Networking.LocalPlayer, waterBowl.gameObject);
                waterBowl.filled = false;
                waterBowl.RequestSerialization();
            }
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    // --- Sleep ------------------------------------------------------------

    private void BeginGoSleep()
    {
        task = TASK_GO_SLEEP;
        if (agent == null || config == null) return;
        agent.isStopped = false;
        agent.speed = config.wanderMoveSpeed;
        Vector3 dest = sleepPoint != null ? sleepPoint.position : (homeCenter != null ? homeCenter.position : transform.position);
        agent.SetDestination(dest);
    }

    private void TickGoSleep()
    {
        if (agent == null) return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            agent.isStopped = true;
            task = TASK_SLEEPING;
            SetActionState(ACTION_SLEEP);
            taskEndTime = Time.time + config.sleepMinDuration;
        }
    }

    private void TickSleeping()
    {
        if (Time.time >= taskEndTime && syncedEnergy >= 0.9f)
        {
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    // --- Toy / chew ---------------------------------------------------------

    private void BeginGoToBone()
    {
        task = TASK_GO_TO_BONE;
        if (agent != null && config != null)
        {
            agent.isStopped = false;
            agent.speed = config.wanderMoveSpeed;
            agent.SetDestination(toyTarget.position);
        }
    }

    private void TickGoToBone()
    {
        if (toyTarget == null || agent == null) { task = TASK_IDLE; return; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
        {
            agent.isStopped = true;
            FaceTarget(toyTarget.position);
            task = TASK_CHEWING;
            SetActionState(ACTION_DIG);
            taskEndTime = Time.time + config.chewDuration;
        }
    }

    private void TickChewing()
    {
        if (Time.time >= taskEndTime)
        {
            toyTarget = null;
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    // --- Agility course -----------------------------------------------------

    private void BeginAgility()
    {
        if (agent == null || config == null || agilityWaypoints.Length == 0) { ScheduleNextAgility(); return; }
        task = TASK_AGILITY;
        agilityIndex = 0;
        agilityJumpFiredForCurrentLeg = false;
        agent.isStopped = false;
        agent.speed = config.agilityMoveSpeed;
        if (agilityWaypoints[0] != null) agent.SetDestination(agilityWaypoints[0].position);
    }

    private void TickAgility()
    {
        if (agent == null) return;
        Transform wp = agilityWaypoints[agilityIndex];
        if (wp == null) { AdvanceAgility(); return; }

        if (agilityIsJumpPoints != null && agilityIndex < agilityIsJumpPoints.Length && agilityIsJumpPoints[agilityIndex] && !agilityJumpFiredForCurrentLeg &&
            !agent.pathPending && agent.remainingDistance <= config.agilityJumpLeadDistance)
        {
            if (animator != null) animator.SetTrigger("JumpTrigger");
            agilityJumpFiredForCurrentLeg = true;
        }

        if (!agent.pathPending && agent.remainingDistance <= config.agilityWaypointArriveDistance)
        {
            AdvanceAgility();
        }
    }

    private void AdvanceAgility()
    {
        agilityIndex++;
        agilityJumpFiredForCurrentLeg = false;
        if (agilityIndex >= agilityWaypoints.Length)
        {
            task = TASK_IDLE;
            ScheduleNextAgility();
            ScheduleNextWander();
            return;
        }
        Transform next = agilityWaypoints[agilityIndex];
        if (next != null && agent != null) agent.SetDestination(next.position);
    }

    private void ScheduleNextAgility()
    {
        float baseInterval = config != null ? config.agilityIntervalSeconds : 45f;
        nextAgilityTime = Time.time + baseInterval * Random.Range(0.7f, 1.3f);
    }

    // --- Idle / wander / ambient ---------------------------------------------

    private void TickIdleWander()
    {
        if (task == TASK_WANDER)
        {
            if (agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                task = TASK_IDLE;
                ScheduleNextWander();
            }
            return;
        }

        TryGreetNearbyPlayer();

        if (Time.time >= nextWanderTime)
        {
            float r = Random.value;
            if (r < 0.12f) { BeginAmbientSit(); return; }
            if (r < 0.22f) { BeginAmbientLie(); return; }
            if (r < 0.30f) { Bark(); ScheduleNextWander(); return; }
            if (r < 0.36f) { BeginAmbientScratch(); return; }
            BeginWander();
        }
    }

    private void BeginWander()
    {
        if (agent == null || config == null) return;
        Vector3 center = homeCenter != null ? homeCenter.position : transform.position;
        Vector3 randomPoint = center + Random.insideUnitSphere * config.wanderRadius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, config.wanderRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.speed = config.wanderMoveSpeed;
            agent.SetDestination(hit.position);
            task = TASK_WANDER;
            if (debugLogging) Debug.Log("[DogAI] BeginWander -> " + hit.position + " (from " + transform.position + ")");
        }
        else
        {
            ScheduleNextWander();
            if (debugLogging) Debug.Log("[DogAI] BeginWander: NavMesh.SamplePosition failed near " + randomPoint);
        }
    }

    private void BeginAmbientSit()
    {
        task = TASK_SIT_AMBIENT;
        SetActionState(ACTION_SIT);
        taskEndTime = Time.time + Random.Range(2f, 5f);
    }

    private void BeginAmbientLie()
    {
        task = TASK_LIE_AMBIENT;
        SetActionState(ACTION_LIE);
        taskEndTime = Time.time + Random.Range(3f, 8f);
    }

    // One-shot "scratch an itch" flavor gimmick, same shape as
    // BeginAmbientSit/Lie - only entered from Idle/Wander (see
    // TickIdleWander), so the agent is already stationary and there's no
    // static-pose skating risk to guard against here (unlike DropCarriedBall
    // and ReactToPet, which can fire mid-move).
    private void BeginAmbientScratch()
    {
        task = TASK_SCRATCH_AMBIENT;
        SetActionState(ACTION_SCRATCH);
        taskEndTime = Time.time + SCRATCH_CLIP_DURATION;
    }

    private void TickTimedIdleReturn()
    {
        if (Time.time >= taskEndTime)
        {
            SetActionState(ACTION_NONE);
            task = TASK_IDLE;
            ScheduleNextWander();
        }
    }

    private void ScheduleNextWander()
    {
        float lo = config != null ? config.wanderIntervalMin : 3f;
        float hi = config != null ? config.wanderIntervalMax : 8f;
        nextWanderTime = Time.time + Random.Range(lo, hi);
        RandomizeIdleVariant();
    }

    // Idle state's motion is now a Simple 1D blend tree (IdleVariety) keyed on
    // the "IdleVariant" float, one child per unused idle clip in
    // ShibaInu_anim_IP.fbx (Idle_1/2/3/4/6/7) at integer thresholds 0-5.
    // Setting it to an exact integer selects that single clip with no blend
    // bleed - picking a fresh one each time the dog is about to sit at Idle
    // for a while (rather than every frame) keeps the loop from visibly
    // popping mid-loop while still giving idle variety over time.
    private void RandomizeIdleVariant()
    {
        if (animator != null) animator.SetFloat("IdleVariant", Random.Range(0, 6));
    }

    // --- Petting ------------------------------------------------------------

    // Place this on a Collider covering the dog's body (or the root itself).
    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReactToPet));
    }

    // Runs on every client so everyone sees the happy reaction, but only the
    // owner actually advances affection/task state.
    public void ReactToPet()
    {
        if (debugLogging) Debug.Log("[DogAI] ReactToPet (isOwner=" + Networking.IsOwner(gameObject) + ")");
        if (animator != null) animator.SetTrigger("BarkTrigger");
        PlayRandomClip(config != null ? config.happyClips : null);

        if (!Networking.IsOwner(gameObject) || config == null) return;

        syncedAffection = Mathf.Clamp01(syncedAffection + config.affectionPerPet);
        RequestSerialization();

        if (task == TASK_IDLE || task == TASK_WANDER || task == TASK_SIT_AMBIENT || task == TASK_LIE_AMBIENT || task == TASK_SCRATCH_AMBIENT)
        {
            // Being pet mid-wander must not leave the agent gliding toward its
            // old destination while the static Sit pose plays - same skating
            // issue as DropCarriedBall (see its comment).
            if (agent != null) agent.isStopped = true;
            task = TASK_PET_REACTION;
            SetActionState(ACTION_SIT);
            taskEndTime = Time.time + config.petReactionDuration;
        }
    }

    // --- Shared helpers -------------------------------------------------------

    // NavMeshAgent only turns to face its direction of travel, so it can arrive
    // at a bowl/toy pointed slightly off to the side rather than square at it -
    // snap to face the target directly so the head-down eat/drink/dig clip
    // actually lines up with the prop instead of aiming past it.
    private void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    // Calling NavMeshAgent.SetDestination() every single tick - even to a
    // point that hasn't meaningfully moved (a stationary ball, a player who
    // only jittered a few centimeters) - forces the agent to keep
    // re-querying/re-committing its path instead of just running it, which
    // shows up as stuttery agent.velocity and, by extension, a locomotion
    // blend that never settles into a steady Walk/Run pose. Only repath when
    // the target has actually moved.
    private void SetDestinationIfMoved(Vector3 worldPos)
    {
        if (hasSetDestination && (worldPos - lastSetDestination).sqrMagnitude < 0.04f) return; // 0.2m
        lastSetDestination = worldPos;
        hasSetDestination = true;
        agent.SetDestination(worldPos);
    }

    private VRCPlayerApi FindNearestPlayer()
    {
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        VRCPlayerApi nearest = null;
        float best = float.MaxValue;
        foreach (VRCPlayerApi p in players)
        {
            if (p == null || !p.IsValid()) continue;
            float d = Vector3.Distance(transform.position, p.GetPosition());
            if (d < best) { best = d; nearest = p; }
        }
        return nearest;
    }

    // Only called while genuinely idle (TASK_WANDER returns before reaching
    // this), so a fresh FindNearestPlayer() here is at most twice a second -
    // no need for the same result-caching TickReturnBall() uses for its much
    // longer, every-Update()-tick fetch walk.
    private void TryGreetNearbyPlayer()
    {
        if (config == null || Time.time < nextGreetCheckTime || Time.time < nextGreetTime) return;
        nextGreetCheckTime = Time.time + 0.5f;

        VRCPlayerApi nearest = FindNearestPlayer();
        if (nearest == null) return;

        if (Vector3.Distance(transform.position, nearest.GetPosition()) <= config.greetDistance)
        {
            Bark();
            nextGreetTime = Time.time + config.greetCooldownSeconds;
        }
    }

    // Local-only flavor bark (like ZombieAI's ambient sounds) - plays for
    // whoever currently owns the dog, not broadcast to everyone. The
    // network-broadcast happy bark on pet is handled separately via ReactToPet.
    private void Bark()
    {
        if (animator != null) animator.SetTrigger("BarkTrigger");
        PlayRandomClip(config != null ? config.barkClips : null);
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (voiceAudioSource == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) voiceAudioSource.PlayOneShot(clip);
    }

    private void SetActionState(int newState)
    {
        if (syncedActionState == newState) return;
        syncedActionState = newState;
        RequestSerialization();
        ApplyActionStateLocal();
    }

    public override void OnDeserialization()
    {
        if (syncedActionState != lastAppliedActionState) ApplyActionStateLocal();
    }

    private void ApplyActionStateLocal()
    {
        lastAppliedActionState = syncedActionState;
        if (animator == null) return;
        animator.SetInteger("ActionState", syncedActionState);

        // Entering Sit plays a one-shot Sitting_start -> Sitting_loop_1 hand-off
        // in the Animator rather than snapping straight into the loop (see
        // ShibaInu_Gameplay.controller's SitStart/SitEnd states). That entry is
        // gated on this Trigger rather than "ActionState==1" directly (which is
        // how every other ambient pose still works) because a persistent int
        // condition on an Any State transition can only be guarded against
        // re-firing by "Can Transition To Self", and that guard only works when
        // the transition's destination IS the state you're currently sitting in -
        // once past SitStart into the loop state, the destination (SitStart) no
        // longer matches the active state, so it would restart Sitting_start
        // every single frame for as long as ActionState stayed 1. A Trigger
        // self-consumes on fire, so it only ever fires once per entry. This runs
        // here (not at each call site that sets ACTION_SIT) so remote clients get
        // it too, exactly once, whenever OnDeserialization applies a real change.
        if (syncedActionState == ACTION_SIT) animator.SetTrigger("SitTrigger");
        // Same start->loop hand-off as Sit, see the comment above - Lie and
        // Dig also now have their own Start/End sequences in the controller.
        if (syncedActionState == ACTION_LIE) animator.SetTrigger("LieTrigger");
        if (syncedActionState == ACTION_DIG) animator.SetTrigger("DigTrigger");
        if (syncedActionState == ACTION_SLEEP) animator.SetTrigger("SleepTrigger");
    }

    public float GetHunger() { return syncedHunger; }
    public float GetThirst() { return syncedThirst; }
    public float GetEnergy() { return syncedEnergy; }
    public float GetAffection() { return syncedAffection; }
}
