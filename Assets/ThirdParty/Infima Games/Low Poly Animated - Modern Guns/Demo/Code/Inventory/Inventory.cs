//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Inventory. This script handles all to do with changing weapons/items.
    /// </summary>
    public class Inventory : InventoryBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "References")]
    
        [Tooltip("The root of all items we're going to spawn.")]
        [SerializeField, NotNull]
        private Transform root;
        
        [Title(label: "Prefabs")]
        
        [Tooltip("All the possible items that we can spawn in the game. This is very important!")]
        [SerializeField, ReorderableList(Foldable = true)]
        private GameObject[] items;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Currently equipped WeaponBehaviour.
        /// </summary>
        private WeaponBehaviour equipped;
        /// <summary>
        /// Currently equipped index.
        /// </summary>
        private int equippedIndex = -1;

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Init.
        /// </summary>
        public override void Init(int equippedAtStart = 0)
        {
            //Equip.
            Equip(equippedAtStart);
        }
        /// <summary>
        /// Equip.
        /// </summary>
        public override WeaponBehaviour Equip(int index)
        {
            //The index needs to be within the array's bounds.
            if (index > items.Length - 1)
                return equipped;

            //No point in allowing equipping the already-equipped weapon.
            if (equippedIndex == index)
                return equipped;
            
            //Disable the currently equipped weapon, if we have one.
            if (equipped != null)
                Destroy(equipped.gameObject);

            //Update index.
            equippedIndex = index;
            //Update equipped.
            equipped = Instantiate(items[equippedIndex], root).GetComponent<WeaponBehaviour>();

            //Reset Location.
            equipped.transform.localPosition = default;
            //Reset Rotation.
            equipped.transform.localEulerAngles = default;

            //Return.
            return equipped;
        }
        
        #endregion

        #region GETTERS

        /// <summary>
        /// GetLastIndex.
        /// </summary>
        public override int GetLastIndex()
        {
            //Get last index with wrap around.
            int newIndex = equippedIndex - 1;
            if (newIndex < 0)
                newIndex = items.Length - 1;

            //Return.
            return newIndex;
        }
        /// <summary>
        /// GetNextIndex.
        /// </summary>
        public override int GetNextIndex()
        {
            //Get next index with wrap around.
            int newIndex = equippedIndex + 1;
            if (newIndex > items.Length - 1)
                newIndex = 0;

            //Return.
            return newIndex;
        }
        
        /// <summary>
        /// GetEquipped.
        /// </summary>
        public override WeaponBehaviour GetEquipped() => equipped;
        /// <summary>
        /// GetEquippedIndex.
        /// </summary>
        /// <returns></returns>
        public override int GetEquippedIndex() => equippedIndex;

        #endregion
    }
}