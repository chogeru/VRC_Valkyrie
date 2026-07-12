//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Attachments. Handles spawning all the required attachments for a weapon.
    /// </summary>
    [RequireComponent(typeof(WeaponSkinner), typeof(Sockets))]
    public class Attachments : AttachmentBehaviour
    {
        #region FIELDS

        /// <summary>
        /// The component that applies a skin to this GameObject. We use this to apply the skin after all attachments
        /// are spawned.
        /// </summary>
        private WeaponSkinner weaponSkinner;
        /// <summary>
        /// Sockets Component. Contains references to all the item's socket points.
        /// </summary>
        private Sockets sockets;
        
        /// <summary>
        /// ScriptableObject containing all the required information on what Attachments to spawn.
        /// </summary>
        private AttachmentData data;
        /// <summary>
        /// This boolean value tells us if this component is currently in the middle of spawning attachments or not.
        /// </summary>
        private bool spawning;
        /// <summary>
        /// List of all spawned attachments.
        /// </summary>
        private readonly List<AttachmentSpawned> spawnedAttachments = new List<AttachmentSpawned>();

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Base.
            base.Awake();
            
            //Get DataLinker.
            var dataLinker = GetComponent<DataLinker>();
            //Check Reference.
            if (dataLinker == null)
                return;
            
            //Get WeaponSkinner.
            weaponSkinner = GetComponent<WeaponSkinner>();
            //Get AttachmentData Value.
            data = dataLinker.Get<AttachmentData>("Attachments");

            //Spawn Attachments.
            Spawn();
        }
        
        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// GetVariant.
        /// </summary>
        public override GameObject GetVariant(string type)
        {
            //Get Attachment Of Type.
            AttachmentSpawned attachment = Get(type);
            //Make sure said Attachment actually has some variants spawned, otherwise what's the point of returning them?
            if (attachment.GameObject == null)
                return null;

            //Return.
            return attachment.GameObject;
        }
        /// <summary>
        /// GetVariant.
        /// </summary>
        public override T GetVariant<T>(string type)
        {
            //Get Variant.
            GameObject variant = GetVariant(type);
            //Return a component of type T on the variant.
            return variant == null ? null : variant.GetComponent<T>();
        }

        /// <summary>
        /// Get.
        /// </summary>
        public override AttachmentSpawned Get(string type)
        {
            //Return.
            return spawnedAttachments.Find(x =>
            {
                //Split The Type By '/'.
                string[] types = x.Type.Split('/');
                //Return this attachment if the last of "types" matches the type passed as input.
                return types[types.Length-1] == type;
            });
        }

        /// <summary>
        /// Waits until the end of the current frame before spawning all attachments again. We do this just in case
        /// there's some attachments left to clean up due to the Destroy method doing some weird things.
        /// </summary>
        private IEnumerator WaitAndSpawn()
        {
            //Yield.
            yield return new WaitForEndOfFrame();
            //Spawn.
            Spawn();
        }
        
        /// <summary>
        /// Waits until all attachments are spawned before finally applying a skin to everything.
        /// </summary>
        private IEnumerator WaitAndApplySkin()
        {
            //Yield.
            yield return new WaitUntil(() => spawning == false);
            //Apply Skin.
            if(weaponSkinner != null)
                weaponSkinner.Apply();
        }
        
        #endregion

        #region METHODS
        
        /// <summary>
        /// Sets up for spawning all the attachments.
        /// </summary>
        private void Spawn()
        {
            //We start spawning the attachments, so we let the value know.
            spawning = true;
            //Spawn all the attachments.
            SpawnAll(data.Attachments.Select(attachment => new Attachment() {Type = attachment.Type, Socket = attachment.Socket, Variants = attachment.Variants}).ToList());
            //Wait to apply a skin to the GameObject.
            StartCoroutine(nameof(WaitAndApplySkin));
        }
        /// <summary>
        /// Spawns all the Attachments in the given toSpawn List.
        /// </summary>
        private void SpawnAll(List<Attachment> toSpawn)
        {
            //If we get here it means that we're done spawning all attachments!
            if(toSpawn.Count == 0)
            {
                //Stop spawning more!
                spawning = false;
                
                //Return.
                return;
            }
            
            //Get Attachment.
            Attachment attachment = toSpawn[0];
            
            //Index of the variant to spawn. We randomize this for every Attachment type.
            int index = Random.Range(0, attachment.Variants.Count);
            
            //Split Types.
            string[] typeSplit = attachment.Type.Split('/');
            //Check if we have sub-types.
            if (typeSplit.Length > 1)
            {
                //Find all attachments that have the first sub-type the same as the current one.
                List<Attachment> sameGroupAttachments = toSpawn.FindAll(x =>
                {
                    //Split Type Match.
                    string[] subTypeSplit = x.Type.Split('/');
                    //If there's only one type, we don't really care too much about this one, as it is impossible to match it here.
                    if (subTypeSplit.Length == 1)
                        return false;

                    //Return true if the types match.
                    return subTypeSplit[0] == typeSplit[0];
                });
                
                //Duplicate list of the attachments to spawn. Used so we can safely remove items while still inside the for loop iterating on them.
                List<Attachment> toSpawnDuplicate = toSpawn.Select(att => new Attachment() {Type = att.Type, Socket = att.Socket, Variants = att.Variants}).ToList();

                //Loop through the Attachment items that have the same first Type (../..) as the current Attachment value in the for loop.
                foreach (Attachment item in sameGroupAttachments)
                {
                    //Spawn Attachment. We spawn the attachment with the same type so that we can make sure they all start existing at about the same time.
                    SpawnAttachment(item, index);
                    //Remove it from the list of Attachment items to spawn.
                    toSpawnDuplicate.Remove(item);
                }
                
                //Keep spawning attachments that are in the duplicate list. This list doesn't have any of the attachments that were spawned as a consequence of matching the first Type with the current one (../..).
                SpawnAll(toSpawnDuplicate);

                //Break. Don't keep iterating on this Attachment list, as it is faulty. We now have the toSpawnDuplicate one, which excludes all the already-spawned ones.
                return;
            }
            
            //Spawn The Attachment.
            SpawnAttachment(attachment, index);

            //Spawn.
            toSpawn.Remove(attachment);
            SpawnAll(toSpawn);
        }

        /// <summary>
        /// SpawnAttachment. Spawns a specific Attachment variant, and adds it to the Attachments list.
        /// </summary>
        /// <param name="attachment">Attachment to spawn.</param>
        /// <param name="index">Index of the variant to spawn for this Attachment.</param>
        private void SpawnAttachment(Attachment attachment, int index)
        {
            //Get Sockets.
            sockets = GetComponent<Sockets>();
            
            //Split the Socket value to check for sub-Sockets.
            string[] socketSplit = attachment.Socket.Split('/');
                
            //Check if the Socket value has some sort of Type before it. If it does, that means we're specifying we want a socket on an already spawned Attachment variant instead of on the item itself.
            if(socketSplit.Length > 1)
            {
                //This tries to give us an Attachment that matches the type we're looking for to use as a Socket point.
                Attachment matchingAttachment = data.Attachments.Find(x =>
                {
                    //Splits the type value.
                    string[] typeSplit = x.Type.Split('/');

                    //Returns true only when the type matches the socket's type.
                    return typeSplit[typeSplit.Length - 1] == socketSplit[0];
                });

                //Socket Transform.
                Transform socketTransform = sockets.GetSocketTransform(matchingAttachment.Socket);
                //Check Reference.
                if (socketTransform == null)
                    return;
                
                //Actual Variant.
                Transform variantSpawnedObject = socketTransform.GetChild(0);
                //Check Reference.
                if (variantSpawnedObject == null)
                    return;
                
                //Get Sockets. This time we get it from the actual variant, this way we can socket to it.
                sockets = variantSpawnedObject.GetComponent<Sockets>();
            }
                
            //Socket Parent. Gets the Transform of the Socket we actually will use to parent the new Attachment spawned.
            Transform socketParent = sockets.GetSocketTransform(socketSplit[socketSplit.Length - 1]);
            //Instantiate the new Attachment variant.
            GameObject attachmentGameObject = Instantiate(attachment.Variants[index], default, default, socketParent);
            //Reset Location.
            attachmentGameObject.transform.localPosition = default;
            //Reset Rotation.
            attachmentGameObject.transform.localEulerAngles = default;
                
            //Add New Attachment.
            spawnedAttachments.Add(new AttachmentSpawned()
            {
                //Type.
                Type = attachment.Type,
                //Socket.
                Socket = attachment.Socket,
                //Variant.
                GameObject = attachmentGameObject,
                //Index.
                Index = index
            });
        }
        /// <summary>
        /// Randomize.
        /// </summary>
        public override void Randomize()
        {
            //Destroy Attachment Variants.
            foreach (AttachmentSpawned attachment in spawnedAttachments)
                Destroy(attachment.GameObject);
            
            //Clear.
            spawnedAttachments.Clear();
            //Wait And Spawn.
            StartCoroutine(nameof(WaitAndSpawn));
        }
        
        #endregion
    }
}