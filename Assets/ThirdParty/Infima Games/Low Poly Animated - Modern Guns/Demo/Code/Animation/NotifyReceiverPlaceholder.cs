//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
	/// <summary>
	/// This class is helpful when adding weapons alone in the scene that are playing animations.
	/// As, without it, the animation events would not have a receiver, and thus create errors!
	/// </summary>
	public class NotifyReceiverPlaceholder : MonoBehaviour
	{
		#region ANIMATION

		private void OnAmmunitionFill(int amount = 0)
		{
		}

		private void OnGrenade()
		{
		}
		private void OnSetActiveMagazine(int active)
		{
		}
		
		private void OnAnimationEndedBolt()
		{
		}
		private void OnAnimationEndedReload()
		{
		}

		private void OnAnimationEndedGrenadeThrow()
		{
		}
		private void OnAnimationEndedMelee()
		{
		}

		private void OnAnimationEndedInspect()
		{
		}
		private void OnAnimationEndedHolster()
		{
		}
		
		/// <summary>
		/// Ejects a casing!
		/// </summary>
		private void OnEjectCasing()
		{
			//Grab Equipped Weapon.
			WeaponBehaviour equippedWeapon = ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();
			//Check Reference.
			if (equippedWeapon == null)
				return;
			
			//Eject Casing.
			equippedWeapon.EjectCasing();
		}

		private void OnSlideBack()
		{
		}

		private void OnSetActiveKnife()
		{
		}

		private void OnDropMagazine(int drop = 0)
		{
		}

		#endregion
	}   
}