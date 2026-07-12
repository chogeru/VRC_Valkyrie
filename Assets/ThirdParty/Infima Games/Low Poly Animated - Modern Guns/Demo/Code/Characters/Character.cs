//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;
using Vector2 = UnityEngine.Vector2;

namespace InfimaGames.Animated.ModernGuns
{
	/// <summary>
	/// Main Character Component. This component handles the most important functions of the character, and interfaces
	/// with basically every part of the asset, it is the hub where it all converges.
	/// </summary>
	public class Character : CharacterBehaviour
	{
		#region FIELDS SERIALIZED
		
		[Title(label: "Inventory")]
		
		[Tooltip("Inventory.")]
		[SerializeField]
		private InventoryBehaviour inventory;

		[Title(label: "Animation")]

		[Tooltip("Determines how smooth the locomotion blendspace is.")]
		[SerializeField]
		private float dampTimeLocomotion = 0.15f;
		
		[Title(label: "Animation Procedural")]
		
		[Tooltip("Character Animator.")]
		[SerializeField]
		private Animator characterAnimator;
		
		#endregion

		#region FIELDS

		/// <summary>
		/// True if the character is aiming.
		/// </summary>
		private bool aiming;
		/// <summary>
		/// True if the character is running.
		/// </summary>
		private bool running;
		/// <summary>
		/// True if the character has its weapon holstered.
		/// </summary>
		private bool holstered;

		/// <summary>
		/// Last Time.time at which we shot.
		/// </summary>
		private float lastShotTime;
		
		/// <summary>
		/// The currently equipped weapon.
		/// </summary>
		private WeaponBehaviour equippedWeapon;

		/// <summary>
		/// True if the character is crouching.
		/// </summary>
		private bool crouching;
		/// <summary>
		/// True if the character is lowering its weapon.
		/// </summary>
		private bool lowered;
		
		/// <summary>
		/// Movement Axis Values.
		/// </summary>
		private Vector2 axisMovement;

		/// <summary>
		/// True if the player is holding the aiming button.
		/// </summary>
		private bool holdingButtonAim;
		/// <summary>
		/// True if the player is holding the running button.
		/// </summary>
		private bool holdingButtonRun;
		/// <summary>
		/// True if the player is holding the firing button.
		/// </summary>
		private bool holdingButtonFire;

		/// <summary>
		/// True if the game cursor is locked! Used when pressing "Escape" to allow developers to more easily access the editor.
		/// </summary>
		private bool cursorLocked = true;
		/// <summary>
		/// Inputs Data. Contains all the data related to the player's possible inputs.
		/// </summary>
		private Inputs inputs;
		/// <summary>
		/// If false, it means that the player can't control anything in the asset. We use this to disable controls for a bit when the demos start.
		/// </summary>
		private bool canActionInputs;

		/// <summary>
		/// The IGameStartService reference we cache at the start of the game so we can check whether it has started or not. There might be some other uses for this later on, but for now that is it!
		/// </summary>
		private IGameStartService gameStartService;

		//TODO
		private float lastTimeRunning;
		//TODO
		private bool tacticalSprint;

		#endregion

		#region UNITY

		/// <summary>
		/// Awake.
		/// </summary>
		protected override void Awake()
		{
			//Update the cursor's state.
			UpdateCursorState();
			
			//Initialize Inventory.
			inventory.Init();
			//Refresh!
			RefreshWeaponSetup();

			//Get DataLinker.
			var dataLinker = GetComponent<DataLinker>();
			//Check Reference.
			if (dataLinker == null)
				return;
			
			//Cache Inputs.
			inputs = dataLinker.Get<Inputs>("Inputs");
			//Cache IGameStartService.
			gameStartService = ServiceLocator.Current.Get<IGameStartService>();
		}

		/// <summary>
		/// Update.
		/// </summary>
		protected override void Update()
		{
			//Handle Input.
			if(gameStartService.HasStarted())
				HandleInput();
			
			//Update Animator.
			UpdateAnimator();
		}

		#endregion

		#region FUNCTIONS

		/// <summary>
		/// GetInventory.
		/// </summary>
		public override InventoryBehaviour GetInventory() => inventory;
		/// <summary>
		/// IsCursorLocked.
		/// </summary>
		/// <returns></returns>
		public override bool IsCursorLocked() => cursorLocked;

		#endregion

		#region METHODS

		//TODO
		private void StopRunning() => running = tacticalSprint = false;

		/// <summary>
		/// HandleInput. This function handles all the inputs in the asset.
		/// </summary>
		private void HandleInput()
		{
			#region Aiming
			
			//Update Aiming Value.
			aiming = cursorLocked && Input.GetKey(inputs.Get(CInputs.Aiming)) && !holstered;
			
			//If we're aiming, make sure that the character can never have its weapon lowered or run. We do this because otherwise it looks super odd.
			if (aiming)
			{
				//Stop Lowered.
				lowered = false;
				//Stop Running.
				StopRunning();

				//Stop the reloading.
				if(!equippedWeapon.CanReloadAimed())
					characterAnimator.SetBool("Stop Reload", true);
			}
			else
                characterAnimator.SetBool("Stop Reload", false);

            #endregion

            #region Crouching

            //Check Input.
            if (cursorLocked && Input.GetKeyDown(inputs.Get(CInputs.Crouching)))
			{
				//Toggle.
				crouching = !crouching;
				//Set Bool.
				characterAnimator.SetBool(AHashes.Crouching, crouching);
			}

			#endregion
			
			#region Firing

			//Get Fire KeyCode.
			KeyCode fireKeyCode = inputs.Get(CInputs.Fire);
			//Update the value that lets us know if the player is holding down the fire button.
			holdingButtonFire = Input.GetKey(fireKeyCode) && cursorLocked;

			//We can't fire while holstered, so we check this right here.
			if (!holstered && cursorLocked)
			{
				//Single-Fire.
				if (Input.GetKeyDown(fireKeyCode) && !equippedWeapon.IsAutomatic())
					TryFire();
				
				//Holding the firing button.
				if (holdingButtonFire && equippedWeapon.IsAutomatic())
					TryFire();
			}

			#endregion
			
			#region Lowering

			//Pressing the lower button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Lower)) && cursorLocked)
			{
				//Toggle.
				lowered = !lowered;
				
				//Stop running no matter whether the weapon is lowered or not. This way our transitions get to play properly.
				StopRunning();
			}

			#endregion

			#region Reloading

			//We can't reload while being holstered.
			bool canReloadAimed = !aiming || equippedWeapon.CanReloadAimed();
			if (!holstered && cursorLocked && canReloadAimed)
			{
				//Pressing the reload button.
				if (Input.GetKeyDown(inputs.Get(CInputs.Reload)))
					PlayReloadAnimation();

				//Pressing the reload empty button.
				if (Input.GetKeyDown(inputs.Get(CInputs.ReloadEmpty)))
					PlayReloadAnimation("Reload Empty");
			}

			#endregion

			#region Inspect

			//Pressing Inspect Button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Inspect)) && !holstered && cursorLocked)
				Inspect();

			#endregion

			#region Holster

			//Pressing Holster Button.
			if (cursorLocked && Input.GetKeyDown(inputs.Get(CInputs.Holster)))
			{
				//Stop Running/Lowered. These make no sense while holstering, obviously.
				lowered = false; StopRunning();

				//Set.
				SetHolstered(!holstered);

				//Play Animation.
				characterAnimator.CrossFade(holstered ? "Holster" : "Unholster", 0.0f, 3, 0.0f);
			}

			#endregion

			#region Grenade Throw

			//Pressing Grenade Button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Grenade)) && !holstered && cursorLocked)
				PlayGrenadeThrow();

			#endregion

			#region Knife

			//Pressing Knife Button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Knife)))
				PlayMelee();

			#endregion
			
			#region Running
			
			//Pressing Running Button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Running)) && !holstered && cursorLocked)
			{
				//TODO
				if (Time.time - lastTimeRunning < 0.2f && running)
					tacticalSprint = true;
				else
				{
					//Toggle.
					running = !running;	
				}

				//Save the last time we pressed this button so we can use it for tactical sprints.
				lastTimeRunning = Time.time;
			}
			
			//Stop Lowered When Running.
			if (running)
				lowered = false;
			else
				tacticalSprint = false;
			
			#endregion
			
			#region Movement
			
			//Update Movement.
			axisMovement = cursorLocked ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : default;

			#endregion

			#region Inventory Switching

			//Make sure the cursor is locked, and we're not paused.
			if (cursorLocked)
			{
				//ScrollWheel Value.
				// float scrollWheel = Input.GetAxisRaw("Mouse ScrollWheel");
				// if(scrollWheel != 0)
				// 	ScrollInventory(scrollWheel);
				
				//Scroll Forward.
				if (Input.GetKeyDown(inputs.Get(CInputs.SwitchPositive)))
					ScrollInventory(1);

				//Scroll Backward.
				if (Input.GetKeyDown(inputs.Get(CInputs.SwitchNegative)))
					ScrollInventory(-1);	
			}

			#endregion

			#region Escape
			
			//Pressed Escape Button.
			if (Input.GetKeyDown(inputs.Get(CInputs.Escape)))
			{
				//Toggle the cursor locked value.
				cursorLocked = !cursorLocked;
				//Update the cursor's state.
				UpdateCursorState();
			}

			#endregion
		}
		
		/// <summary>
		/// Scrolls the inventory in a specific direction. The direction is determined by the sign of the value passed.
		/// </summary>
		private void ScrollInventory(float value)
		{
			//Get the next index to switch to.
			int indexNext = value > 0 ? inventory.GetNextIndex() : inventory.GetLastIndex();
			//Get the current weapon's index.
			int indexCurrent = inventory.GetEquippedIndex();
					
			//Make sure we're allowed to change, and also that we're not using the same index, otherwise weird things happen!
			if (indexCurrent != indexNext)
				StartCoroutine(nameof(Equip), indexNext);
		}

		/// <summary>
		/// Updates all the animator properties for this frame.
		/// </summary>
		private void UpdateAnimator()
		{
			#region Update Movement Values
			
			//Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
			float movementValue = Mathf.Clamp01(Mathf.Abs(axisMovement.x) + Mathf.Abs(axisMovement.y));
			characterAnimator.SetFloat(AHashes.Movement, holstered ? 0.0f : movementValue, dampTimeLocomotion, Time.deltaTime);

			//Horizontal Movement Float.
			characterAnimator.SetFloat(AHashes.Horizontal, holstered ? 0.0f : axisMovement.x, dampTimeLocomotion, Time.deltaTime);
			//Vertical Movement Float.
			characterAnimator.SetFloat(AHashes.Vertical, holstered ? 0.0f : axisMovement.y, dampTimeLocomotion, Time.deltaTime);
			
			//Moving.
			if(equippedWeapon)
			{
				equippedWeapon.GetComponent<Animator>().SetBool(AHashes.Movement, movementValue > 0f);
				equippedWeapon.GetComponent<Animator>().SetBool(AHashes.Running, running);
			}
			
			#endregion

			#region Calculate Aiming Alpha
			
			//Aiming Alpha.
			var aimingAlpha = 0.0f;
			
			//This entire weird chunk of code just makes sure that the aimingAlpha value is properly set to the correct percentage.
			if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				aimingAlpha = characterAnimator.GetNextAnimatorStateInfo(0).IsName("Aim") ? Mathf.Lerp(0, 1f, characterAnimator.GetAnimatorTransitionInfo(0).normalizedTime) : 0.0f;
			}
			else if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Aim"))
			{
				aimingAlpha = characterAnimator.GetNextAnimatorStateInfo(0).IsName("Idle") ? Mathf.Lerp(1f, 0, characterAnimator.GetAnimatorTransitionInfo(0).normalizedTime) : 1.0f;
			}
			
			//Update the aiming value, but use interpolation. This makes sure that things like firing can transition properly.
			characterAnimator.SetFloat(AHashes.AimingAlpha, aimingAlpha);
			//Update weapon aiming value too!
			if(equippedWeapon)
				equippedWeapon.GetComponent<Animator>().SetFloat(AHashes.AimingAlpha, aimingAlpha);

			#endregion
			
			#region Update Grip Index
			
			//Get AttachmentBehaviour. This will allow us to get attachments.
			AttachmentBehaviour attachmentBehaviour = equippedWeapon.GetAttachments();
			//Reference Check.
			if (attachmentBehaviour != null)
			{
				//Get Grip Attachment.
				var gripBehaviour = attachmentBehaviour.GetVariant<GripBehaviour>("Grip");
				//Update the Grip Index value. This value is what changes the animation used for the idle pose.
				characterAnimator.SetFloat(AHashes.GripIndex, gripBehaviour != null ? gripBehaviour.GetIndex() : 0, 0.05f, Time.deltaTime);	
			}
			
			#endregion

			//Update Jumping Value.
			characterAnimator.SetBool(AHashes.Jumping, Input.GetKey(inputs.Get(CInputs.Jumping)));
			//Update Lowered Value.
			characterAnimator.SetBool(AHashes.Lowered, lowered);
			//Update Animator Aiming.
			characterAnimator.SetBool(AHashes.Aim, aiming);
			//Update Animator Running.
			characterAnimator.SetBool(AHashes.Running, running);
			//Update Animator Tactical Sprint.
			characterAnimator.SetBool(AHashes.TacticalSprint, tacticalSprint);
		}
		/// <summary>
		/// Plays the inspect animation.
		/// </summary>
		private void Inspect()
		{
			//Play.
			characterAnimator.CrossFade("Inspect", 0.0f, 1, 0);
			
			//Stop Running/Lowered To Inspect. Doing this actually helps a lot with feel.
			lowered = false; StopRunning();
		}
		/// <summary>
		/// Fires the character's weapon.
		/// </summary>
		private void TryFire()
		{
			//Check Fire Rate.
			if (!(Time.time - lastShotTime > 60.0f / equippedWeapon.GetRateOfFire())) 
				return;
			
			//Save the shot time, so we can calculate the fire rate correctly.
			lastShotTime = Time.time;

			//Play firing animation.
			characterAnimator.CrossFade("Fire", 0.0f, 1, 0);

			//Stop Running/Lowered To Fire. Doing this actually helps a lot with feel.
			lowered = false; StopRunning();
		}
		
		/// <summary>
		/// Plays the reload animation.
		/// </summary>
		private void PlayReloadAnimation(string animName = "Reload")
		{
			#region Animation

			//Get the name of the animation state to play, which depends on weapon settings, and ammunition!
			string stateName = equippedWeapon.HasCycledReload() ? "Reload Open" : animName;
			
			//Play the animation state!
			characterAnimator.Play(stateName, 1, 0.0f);

			#endregion
			
			//Reload.
			equippedWeapon.Reload();
			//Stop Running/Lowered.
			lowered = false; StopRunning();
		}

		/// <summary>
		/// Equip Weapon Coroutine.
		/// </summary>
		private IEnumerator Equip(int index = 0)
		{
			//Only if we're not holstered, holster. If we are already, we don't need to wait.
			if(!holstered)
			{
				//Play.
				characterAnimator.CrossFade("Holster Quick", 0.0f, 3, 0.0f);
				//Holster.
				SetHolstered();
				//Wait.
				yield return new WaitUntil(() => characterAnimator.GetCurrentAnimatorStateInfo(3).IsName("Holster Quick Completed"));
			}
			
			//Equip The New Weapon.
			inventory.Equip(index);
			//Refresh.
			RefreshWeaponSetup();
			
			//Rebind. If we don't do this we get some super weird errors with some animation curves not working properly.
			characterAnimator.Rebind();

			characterAnimator.CrossFade("Unholster Quick", 0.0f, 3, 0.0f);
			//Unholster. We do this just in case we were holstered.
			SetHolstered(false);
		}
		/// <summary>
		/// Refresh all weapon things to make sure we're all set up!
		/// </summary>
		private void RefreshWeaponSetup()
		{
			//Make sure we have a weapon. We don't want errors!
			if ((equippedWeapon = inventory.GetEquipped()) == null)
				return;
			
			//Update Animator Controller. We do this to update all animations to a specific weapon's set.
			characterAnimator.runtimeAnimatorController = equippedWeapon.GetAnimatorController();
		}

		/// <summary>
		/// Updates the cursor state based on the value of the cursorLocked variable.
		/// </summary>
		private void UpdateCursorState()
		{
			//Update cursor visibility.
			Cursor.visible = !cursorLocked;
			//Update cursor lock state.
			Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}

		/// <summary>
		/// Plays The Grenade Throwing Animation.
		/// </summary>
		private void PlayGrenadeThrow()
		{
			//Play.
			characterAnimator.CrossFade("Grenade Throw", 0.0f,
				1, 0.0f);
			
			//Stop Running/Lowered.
			lowered = false; StopRunning();
		}
		/// <summary>
		/// Play The Melee Animation.
		/// </summary>
		private void PlayMelee()
		{
			//Play Normal.
			characterAnimator.CrossFade("Knife Attack", 0, 1, 0.0f);
			
			//Stop Running/Lowered.
			lowered = false; StopRunning();
		}
		
		/// <summary>
		/// Updates the "Holstered" variable, along with the Character's Animator value.
		/// </summary>
		private void SetHolstered(bool value = true)
		{
			//Update value.
			holstered = value;
			
			//Update Animator.
			const string boolName = "Holstered";
			characterAnimator.SetBool(boolName, holstered);
		}

		#endregion
	}
}