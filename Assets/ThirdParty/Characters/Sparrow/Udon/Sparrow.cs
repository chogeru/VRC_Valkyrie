
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Sparrow : UdonSharpBehaviour
{
	private CharacterController controller;
	private Animator animator;
	private Vector3 velocity;

	private Vector3 initPosition;
	private float fallingTime;

	private int state;
	private float nextActionInterval;
	private int remainJumpCount;

	private float angle;
	private float destAngle;
	private float currentRotate;
	private float turnAngle;

	private uint x;

	[Tooltip("位置を同期します")]
	public bool globalSync = false;
	[Tooltip("シーンにナビメッシュが設定されている場合、ナビメッシュを参照して壁探索や行動範囲制限を行います\nこの方法は通常の処理より高速なため、大量に配置する場合は適切に設定することでワールドの動作を軽量化できます\nシーンにナビメッシュが設定されていない場合はこの項目をONにしないでください")]
	public bool useNavMesh = false;
	[Tooltip("空中にいる時間がこの秒数より長くなると、初期配置地点にリスポーンします")]
	public float fallingTimeout = 5.0f;
	[Tooltip("初期配置地点から離れすぎないように移動します\n同じ場所に大量に配置することで、群れがあるように見せることができます")]
	public bool clustering = true;

	[Header("Advanced Settings")]
	[Range(0, 0.1f)]
	public float angleSmooth = 0.02f;
	public float wallSensorDistance = 0.2f;
	public float minImpulse = 0.8f;
	public float maxImpulse = 1.4f;
	public float jumpScale = 1.0f;
	public float gravity = 18.0f;
	public float crouchDelay = 0.01f;
	public int minContinuousJumpCount = 1;
	public int maxContinuousJumpCount = 6;

	private int globalSyncInterval;
	private bool initialSync;
	[UdonSynced] private Vector3 syncPosition;
	[UdonSynced] private float syncAngle;
	[UdonSynced] private float syncDestAngle;
	[UdonSynced] private float syncCurrentRotate;
	[UdonSynced] private int syncState;
	[UdonSynced] private uint syncSeed;

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		initPosition = transform.position;

		controller.skinWidth *= transform.localScale.y;

		state = 0;
		nextActionInterval = 1.0f;
		remainJumpCount = 0;

		angle = transform.eulerAngles.y;
		destAngle = angle;
		currentRotate = 0.0f;

		x = (uint)Random.Range(1, 0x7FFFFFFF);
		globalSyncInterval = 5;
		initialSync = false;

		if (globalSync && Networking.IsOwner(gameObject) == false)
		{
			GetComponentInChildren<Renderer>().enabled = false;
		}
	}

	private void Update()
	{
		if (globalSync) { return; }
		Main();
	}

	private void FixedUpdate()
	{
		if (!globalSync) { return; }
		Main();
	}

	private void Main()
	{
		if (controller == null || animator == null) { return; }

		Action();

		angle = Mathf.SmoothDampAngle(angle, destAngle, ref currentRotate, angleSmooth);
		transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

		if (controller.isGrounded)
		{
			fallingTime = 0.0f;
		}
		else
		{
			fallingTime += Time.deltaTime;
			if (fallingTime >= fallingTimeout)
			{
				controller.enabled = false;
				transform.position = initPosition;
				controller.enabled = true;

				fallingTime = 0.0f;
			}
		}
	}

	private void Action()
	{
		nextActionInterval -= Time.deltaTime;
		switch (state)
		{
		case 0:
			if (nextActionInterval > 0)
			{
				return;
			}
			state = 1;
			animator.SetInteger(Animator.StringToHash("State"), 1);
			nextActionInterval += 0.09f + crouchDelay;
			remainJumpCount = (int)RandomRange(minContinuousJumpCount, maxContinuousJumpCount);
			break;
		case 1:
			if (nextActionInterval > 0)
			{
				return;
			}

			RaycastHit hitInfo;

			Vector2 initDirection = new Vector2(initPosition.x - transform.position.x, initPosition.z - transform.position.z);
			Vector2 forward2D = new Vector2(transform.forward.x, transform.forward.z).normalized;

			if (clustering && Vector2.Dot(forward2D, initDirection) < 0.0f && initDirection.sqrMagnitude / 50.0f > RandomValue())
			{
				turnAngle = Mathf.Atan2(initDirection.x, initDirection.y) * Mathf.Rad2Deg + RandomRange(-45.0f, 45.0f);
				nextActionInterval += RandomRange(0.4f, 1.0f);
				state = 4;
				return;
			}
			else if (HitWall(transform.position, transform.forward, wallSensorDistance * transform.localScale.z, out hitInfo))
			{
				if (hitInfo.normal.y == -1)
				{
					turnAngle += 180.0f;
					nextActionInterval += RandomRange(0.4f, 1.0f);
					state = 4;
					return;
				}
				Vector2 normal2D = new Vector2(hitInfo.normal.x, hitInfo.normal.z).normalized;

				if (Vector2.Dot(forward2D, normal2D) < -0.9f)
				{
					turnAngle = Mathf.Atan2(normal2D.x, normal2D.y) * Mathf.Rad2Deg + RandomRange(-45.0f, 45.0f);
					nextActionInterval += RandomRange(0.4f, 1.0f);
					state = 4;
					return;
				}

				if ((normal2D.x * forward2D.y - forward2D.x * normal2D.y) < 0)
				{
					destAngle -= 60.0f;
				}
				else
				{
					destAngle += 60.0f;
				}
			}
			else
			{
				destAngle += RandomRange(-60.0f, 60.0f);
			}

			velocity = (transform.forward * RandomRange(minImpulse, maxImpulse) + new Vector3(0.0f, RandomRange(minImpulse, maxImpulse) * jumpScale, 0.0f)) * transform.localScale.z;

			state = 2;
			animator.SetInteger(Animator.StringToHash("State"), 2);
			animator.SetFloat(Animator.StringToHash("Look"), RandomRange(-0.3f, 0.3f));
			break;
		case 2:
			if (useNavMesh)
			{
				if(NavMesh.Raycast(transform.position, transform.position + velocity * Time.deltaTime, out NavMeshHit navMeshHit, NavMesh.AllAreas))
				{
					velocity = Vector3.ProjectOnPlane(velocity, navMeshHit.normal);
					if(NavMesh.Raycast(transform.position, transform.position + velocity * Time.deltaTime, out navMeshHit, NavMesh.AllAreas))
					{
						velocity.x = 0;
						velocity.z = 0;
					}
				}
			}

			controller.Move(velocity * Time.deltaTime);
			velocity += new Vector3(0.0f, -gravity * transform.localScale.z, 0.0f) * Time.deltaTime;

			if (controller.isGrounded)
			{
				if (remainJumpCount > 0)
				{
					state = 1;
					animator.SetInteger(Animator.StringToHash("State"), 1);
					nextActionInterval = crouchDelay;
					remainJumpCount--;
				}
				else
				{
					state = 0;

					if (RandomValue() < 0.2)
					{
						int EatPattern = (int)RandomRange(10, 12);
						animator.SetInteger(Animator.StringToHash("State"), EatPattern);
						animator.SetFloat(Animator.StringToHash("Look"), 0.0f);

						switch (EatPattern)
						{
						default:
							nextActionInterval = RandomRange(0.4f, 1.0f);
							break;
						case 11:
							nextActionInterval = RandomRange(1.0f, 2.0f);
							break;
						}
					}
					else
					{
						animator.SetInteger(Animator.StringToHash("State"), 0);
						nextActionInterval = RandomRange(0.3f, 1.0f);
					}

					if (globalSync)
					{
						if (Networking.IsOwner(gameObject) == false) { break; }

						if (globalSyncInterval > 0)
						{
							globalSyncInterval--;
							break;
						}

						syncPosition = transform.position;
						syncAngle = angle;
						syncDestAngle = destAngle;
						syncCurrentRotate = currentRotate;
						syncSeed = (uint)Random.Range(1, 0x7FFFFFFF);
						syncState = state;
						RequestSerialization();

						x = syncSeed;
						globalSyncInterval = 5;
					}
				}
			}
			break;
		case 4:
			if (nextActionInterval > 0)
			{
				return;
			}

			destAngle = turnAngle;
			velocity = new Vector3(0.0f, 1.0f, 0.0f) * transform.localScale.z;
			remainJumpCount = 0;

			state = 2;
			animator.SetInteger(Animator.StringToHash("State"), 2);
			break;
		}
	}

	public override void OnDeserialization()
	{
		if (initialSync == false)
		{
			GetComponentInChildren<Renderer>().enabled = true;
			initialSync = true;
		}

		controller.enabled = false;
		transform.position = syncPosition;
		controller.enabled = true;
		angle = syncAngle;
		destAngle = syncDestAngle;
		currentRotate = syncCurrentRotate;
		state = syncState;
		x = syncSeed;
	}

	private bool HitWall(Vector3 position, Vector3 direction, float distance, out RaycastHit hitInfo)
	{
		if (useNavMesh)
		{
			NavMesh.Raycast(position, position + direction * distance, out NavMeshHit navMeshHit, 1);  // Walkable

			hitInfo = new RaycastHit();
			hitInfo.point = navMeshHit.position;
			hitInfo.normal = navMeshHit.normal;
			hitInfo.distance = navMeshHit.distance;
			return navMeshHit.hit;
		}

		float capsuleOffset = Mathf.Max(0.0f, controller.height - controller.radius * 2) / 2;
		Vector3 point1 = transform.TransformPoint(controller.center - Vector3.up * capsuleOffset);
		Vector3 point2 = transform.TransformPoint(controller.center + Vector3.up * capsuleOffset);
		int layerMask = 0b1101110000100111010111;	// StereoLeft - Pickup & StereoLeft
		Physics.CapsuleCast(point1, point2, controller.radius * transform.localScale.z - controller.skinWidth, direction, out hitInfo, distance, layerMask, QueryTriggerInteraction.Ignore);

		if (hitInfo.collider == null)
		{
			return false;
		}

		if (Mathf.Acos(hitInfo.normal.y) > controller.slopeLimit * Mathf.Deg2Rad)
		{
			return true;
		}

		Vector3 newPosition = position + direction * (hitInfo.distance - controller.skinWidth);
		Vector3 newDirection = new Vector3(direction.x, -(hitInfo.normal.x * direction.x + hitInfo.normal.z * direction.z) / hitInfo.normal.y, direction.z).normalized;

		return HitWall(newPosition, newDirection, distance - hitInfo.distance, out hitInfo);
	}

	private uint XorShift()
	{
		x ^= x << 13;
		x ^= x >> 17;
		x ^= x << 5;

		return x;
	}

	private float RandomValue()
	{
		return (XorShift() & 0x7fffffu) / 8388607.0f;
	}

	private float RandomRange(float min, float max)
	{
		return min + RandomValue() * (max - min);
	}
}
