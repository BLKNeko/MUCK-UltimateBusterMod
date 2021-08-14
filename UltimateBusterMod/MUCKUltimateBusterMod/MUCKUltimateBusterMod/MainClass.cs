using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using R2API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Bindings;

namespace MUCKXBusterMod
{
    [BepInPlugin("com.BLKNeko.UltimateBusterMod", "UltimateBusterMod", "1.0.0")]

    public class MainClass : BaseUnityPlugin
    {

        public static MainClass instance;
        public ManualLogSource log;
        public Harmony harmony;

        public static ConfigEntry<int> DamageConfig { get; set; }
        public static ConfigEntry<float> ProjectileSpeedConfig { get; set; }
        public static ConfigEntry<float> ShootCooldownConfig { get; set; }

        public static ConfigEntry<bool> DisableShootSpamConfig { get; set; }
        public static ConfigEntry<bool> CustomSFXConfig { get; set; }
        public static ConfigEntry<bool> izyCraftConfig { get; set; }
        public static ConfigEntry<bool> DisableArrowConsumeConfig { get; set; }





        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            log = Logger;
            harmony = new Harmony("com.BLKNeko.UltimateBusterMod");

            Assets.LoadAssetBundle();
            //Assets.LoadSoundbank();
            Assets.PopulateAssets();


            harmony.PatchAll(typeof(AddBuster));
           // harmony.PatchAll(typeof(UseBuster));
            harmony.PatchAll(typeof(UseButtonBuster));
            harmony.PatchAll(typeof(ShootBuster));
            log.LogInfo("Test UltimateBuster");

            //-----------------CONFIGURATION ------------------

            DamageConfig = Config.Bind<int>(
            "ShootDamage",
            "Damage",
                8,
            "This is the weapon base damage, my default is 8, [INTEGER 1,2,3...]"
            );



            ProjectileSpeedConfig = Config.Bind<float>(
            "ProjectileSpeed",
            "Speed",
                184.0f,
            "This is the arrow speed, my default is 184.0f, [FLOAT 1.0f, 1.1f, 1.2f...]"
            );

            ShootCooldownConfig = Config.Bind<float>(
           "ShootCooldown",
           "Time",
               0.2f,
           "This is the time between every shoot, ONLY WORK IF IF SHOOTSPAM IS DISABLED, my default is 0.2f, [FLOAT 1.0f, 1.1f, 1.2f...]"
           );



            DisableShootSpamConfig = Config.Bind<bool>(
            "DisableShootSpam",
            "DisableShootSpam",
                true,
            "This control the shoot rate, without this you can spam shoots like crazy, my default is true, [BOOL true/false]"
            );

            CustomSFXConfig = Config.Bind<bool>(
            "CustomSFX",
            "Enable",
                true,
            "This control if the custom SFX of megaman X series will play, my default is true, [BOOL true/false]"
            );

            izyCraftConfig = Config.Bind<bool>(
            "IzyCraft",
            "Enable",
                false,
            "This will simplify the craft requeriment to 1 rock, my default is false, [BOOL true/false]"
            );

            DisableArrowConsumeConfig = Config.Bind<bool>(
            "DisableArrowConsume",
            "Enable",
                true,
            "This will make shoots do NOT consume the arrow, my default is true, [BOOL true/false]"
            );





        }

        class Assets
        {

            //-------------------ASSETS

            internal static AssetBundle mainAssetBundle;

            // CHANGE THIS
            private const string assetbundleName = "muckultimatebustermod";

            private static string[] assetNames = new string[0];

            public static Mesh UBMesh;

            public static Texture2D UBSprite;

            public static Material UBMat;

            public static AudioClip XBShootSFX;
            public static AudioClip XBChargeShootSFX;
            public static AudioClip XBChargingSFX;
            public static AudioClip XBFullChargeSFX;

            internal static void LoadAssetBundle()
            {
                if (mainAssetBundle == null)
                {
                    using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MUCKUltimateBusterMod." + assetbundleName))
                    {
                        mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    }
                }

                assetNames = mainAssetBundle.GetAllAssetNames();
            }


            //internal static void LoadSoundbank()
            // {
            //    using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("ZeroModV2.ZeroSB.bnk"))
            //   {
            //         byte[] array = new byte[manifestResourceStream2.Length];
            //         manifestResourceStream2.Read(array, 0, array.Length);
            //         SoundAPI.SoundBanks.Add(array);
            //    }
            // }


            internal static void PopulateAssets()
            {
                if (!mainAssetBundle)
                {
                    Debug.LogError("There is no AssetBundle to load assets from.");
                    return;
                }


                UBSprite = mainAssetBundle.LoadAsset<Texture2D>("UBIcon");

                UBMesh = mainAssetBundle.LoadAsset<Mesh>("UBMesh");

                UBMat = mainAssetBundle.LoadAsset<Material>("UBMat");

                XBShootSFX = mainAssetBundle.LoadAsset<AudioClip>("XBullet");
                XBChargeShootSFX = mainAssetBundle.LoadAsset<AudioClip>("XChargeShot");
                XBChargingSFX = mainAssetBundle.LoadAsset<AudioClip>("Charging");
                XBFullChargeSFX = mainAssetBundle.LoadAsset<AudioClip>("FullCharged");


            }




            //---------------------- END ASSETS

        }

        class AddBuster
        {
            //------------------------------- ADD ITEM

            public static int BusterID;

            

			// Token: 0x06000010 RID: 16 RVA: 0x00002458 File Offset: 0x00000658
			[HarmonyPatch(typeof(ItemManager), "InitAllItems")]
			[HarmonyPostfix]
			private static void Postfix()
			{
				bool flag = ItemManager.Instance.allItems.Count < 1;
				if (!flag)
				{
					Debug.Log("Adding UltimateBuster");
					InventoryItem inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();
					foreach (InventoryItem inventoryItem2 in ItemManager.Instance.allItems.Values)
					{
						bool flag2 = inventoryItem2.name == "Wood Bow";
						if (flag2)
						{
							inventoryItem.Copy(inventoryItem2, 1);
							break;
						}
					}
					inventoryItem.name = "UltimateBuster";
					inventoryItem.description = "Equip at least 1 ARROW to shoot (IT DOES NOT CONSUMES ARROWS)";
					inventoryItem.id = ItemManager.Instance.allItems.Count;
					//inventoryItem.mesh = MuckUpgradeBuildings.hammerMesh;
					inventoryItem.mesh = Assets.UBMesh;
					//inventoryItem.material = MuckUpgradeBuildings.hammerMaterial;
					inventoryItem.material = Assets.UBMat;
					//inventoryItem.sprite = Sprite.Create(MuckUpgradeBuildings.hammerSprite, new Rect(0f, 0f, (float)MuckUpgradeBuildings.hammerSprite.width, (float)MuckUpgradeBuildings.hammerSprite.height), new Vector2(0.5f, 0.5f));
					inventoryItem.sprite = Sprite.Create(Assets.UBSprite, new Rect(0f, 0f, (float)Assets.UBSprite.width, (float)Assets.UBSprite.height), new Vector2(0.5f, 0.5f));

					inventoryItem.positionOffset = new Vector3(-0.24f, 0.80f, 0);

					inventoryItem.scale = 1;
					
					inventoryItem.rotationOffset = new Vector3(140, 270, 300);

					inventoryItem.type = InventoryItem.ItemType.Bow;

                    //inventoryItem.attackDamage = 8;
                    inventoryItem.attackDamage = DamageConfig.Value;


                    inventoryItem.attackSpeed = 0.84f;

                    inventoryItem.attackRange = 184f;

                    

                   // inventoryItem.bowComponent.projectileSpeed = 300f;
                   // inventoryItem.bowComponent.nArrows = 1;
                   // inventoryItem.bowComponent.angleDelta = 1;

				

					

					


					InventoryItem.CraftRequirement craftRequirement = new InventoryItem.CraftRequirement();
                    InventoryItem.CraftRequirement craftRequirement2 = new InventoryItem.CraftRequirement();
                    InventoryItem.CraftRequirement craftRequirement3 = new InventoryItem.CraftRequirement();
                    InventoryItem.CraftRequirement craftRequirement4 = new InventoryItem.CraftRequirement();
                    InventoryItem.CraftRequirement izycraftRequirement = new InventoryItem.CraftRequirement();
                    foreach (InventoryItem inventoryItem3 in ItemManager.Instance.allItems.Values)
					{
						bool flag3 = inventoryItem3.name == "Adamantite bar";
						if (flag3)
						{
							craftRequirement.item = inventoryItem3;
							break;
						}

                    }
					craftRequirement.amount = 2;
                    // ------------------------------------

                    foreach (InventoryItem inventoryItem3 in ItemManager.Instance.allItems.Values)
                    {
                        bool flag4 = inventoryItem3.name == "Obamium bar";
                        if (flag4)
                        {
                            craftRequirement2.item = inventoryItem3;
                            break;
                        }

                    }
                    craftRequirement2.amount = 2;
                    // ------------------------------------

                    foreach (InventoryItem inventoryItem3 in ItemManager.Instance.allItems.Values)
                    {
                        bool flag5 = inventoryItem3.name == "Mithril bar";
                        if (flag5)
                        {
                            craftRequirement3.item = inventoryItem3;
                            break;
                        }

                    }
                    craftRequirement3.amount = 3;
                    // ------------------------------------

                    foreach (InventoryItem inventoryItem4 in ItemManager.Instance.allItems.Values)
                    {
                        bool flag6 = inventoryItem4.name == "AncientCore";
                        if (flag6)
                        {
                            craftRequirement4.item = inventoryItem4;
                            break;
                        }

                    }
                    craftRequirement4.amount = 1;
                    // ------------------------------------


                    //--------------IZY CRAFT -----------------------------

                    foreach (InventoryItem inventoryItemizy in ItemManager.Instance.allItems.Values)
                    {
                        bool flag7 = inventoryItemizy.name == "Rock";
                        if (flag7)
                        {
                            izycraftRequirement.item = inventoryItemizy;
                            break;
                        }

                    }
                    izycraftRequirement.amount = 1;
                    // ------------------------------------


                    if(izyCraftConfig.Value)
                    {


                        inventoryItem.requirements = new InventoryItem.CraftRequirement[]
                            {
                            izycraftRequirement  
                            };
                        ItemManager.Instance.allItems.Add(inventoryItem.id, inventoryItem);
                        Debug.Log("Added UltimateBuster");
                        BusterID = inventoryItem.id;


                    }
                    else
                    {

                        inventoryItem.requirements = new InventoryItem.CraftRequirement[]
                            {
                            craftRequirement,
                            craftRequirement2,
                            craftRequirement3,
                            craftRequirement4
                            };
                        ItemManager.Instance.allItems.Add(inventoryItem.id, inventoryItem);
                        Debug.Log("Added UltimateBuster");
                        BusterID = inventoryItem.id;

                    }

                    
				}
			}

			// Token: 0x06000011 RID: 17 RVA: 0x00002640 File Offset: 0x00000840
			[HarmonyPatch(typeof(CraftingUI), "Awake")]
			[HarmonyPostfix]
			private static void CraftingPostfix(CraftingUI __instance)
			{
				InventoryItem[] items = __instance.tabs[1].items;
				InventoryItem[] array = new InventoryItem[items.Length + 1];
				items.CopyTo(array, 0);
				array[items.Length] = ItemManager.Instance.allItems[BusterID];
				__instance.tabs[1].items = array;
			}



            //------------------------------ ADD ITEM END


        }

        
        public class UseBuster
        {
            public static Animator animator;
            public static UseInventory Instance;

            [HarmonyPatch(typeof(UseInventory), "Use")]
            [HarmonyPostfix]
            public static bool Postfix(UseInventory __instance)
            {
                Debug.Log("Use UltimateBuster");

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("UltimateBuster");

                if (flag)
                {

                    //__instance.stateName = "Charge";
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Charge, true);
                    __instance.chargeSfx.PlayOneShot(Assets.XBChargingSFX);
                   // __instance.chargeSfx.pitch = this.currentItem.attackSpeed;
                    //__instance.stayOnScreen = true;

                }

                return true;

            }

        }
        


        

        public class UseButtonBuster
        {

            [HarmonyPatch(typeof(UseInventory), "UseButtonUp")]
            [HarmonyPrefix]
            public static bool Postfix(UseInventory __instance)
            {

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("UltimateBuster");

                if(flag)
                {

                    __instance.animator.Play("Idle");
                    // __instance.eatingEmission.enabled = false;
                    CooldownBar.Instance.HideBar();
                    // __instance.eatSfx.Stop();
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Eat, false);
                    __instance.CancelInvoke();

                    // __instance.chargeSfx.Stop();
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Charge, false);
                    Debug.Log("UseButton UltimateBuster");
                    ShootBuster.Postfix(__instance);

                }

                
                return true;

            }

        }

        

        public class ShootBuster
        {


            public static Animator animator;
            public static HitBox hitBox;
            // Token: 0x0400048C RID: 1164
            public static UseInventory Instance;
            // Token: 0x0400048F RID: 1167
            public static TrailRenderer swingTrail;

            // Token: 0x04000490 RID: 1168
            public static RandomSfx swingSfx;


            // Token: 0x04000496 RID: 1174
            public static float eatTime;

            // Token: 0x04000497 RID: 1175
            public static float attackTime;

            // Token: 0x04000498 RID: 1176
            public static float chargeTime;

            // Token: 0x0400049C RID: 1180
            public static InventoryItem currentItem;

            public static bool InCooldown = false;

            public static bool IsCharged = false;
            public static bool IsReady = false;

            

            [HarmonyPatch(typeof(UseInventory), "ReleaseWeapon")]
            [HarmonyPrefix]
            public static bool Postfix(UseInventory __instance)
            {

                InventoryItem inventoryBuster = (InventoryItem)Traverse.Create(__instance).Field("currentItem").GetValue();
                bool flag = inventoryBuster != null && inventoryBuster.name.Contains("UltimateBuster");

                


                if (flag)
                {

                    Debug.Log("Shoot UltimateBuster");

                    


                    //ShootBuster.animator.Play("Shoot", -1, 0f);

                    float num = 1f;
                    // if (this.IsAnimationPlaying("ChargeHold"))
                    // {
                    //     num = 1f;
                    // }
                    // else
                    // {
                    //    num = __instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    //     MonoBehaviour.print("charge: " + num);
                    // }
                    ClientSend.AnimationUpdate(OnlinePlayer.SharedAnimation.Charge, false);
                    //__instance.animator.Play("Shoot", -1, 0f);
                    // CooldownBar.Instance.HideBar();
                    //bool col = CooldownBar.Instance.isActiveAndEnabled;
                    
                    //Debug.Log(col);
                    if (InventoryUI.Instance.arrows.currentItem == null || InCooldown )
                    {
                        Debug.Log("Stoooooop");
                        return false;
                        
                    }
                    else
                    {
                        __instance.animator.Play("Shoot", -1, 0f);


                        //__instance.chargeSfx.clip = Assets.XBChargingSFX;
                       // __instance.chargeSfx.Play();

                       // __instance.eatSfx.clip = Assets.MBShootSFX;

                        if(CustomSFXConfig.Value)
                        __instance.eatSfx.PlayOneShot(Assets.XBShootSFX);
                        
                        
                        

                        InventoryItem inventoryItem = Hotbar.Instance.currentItem;
                        InventoryItem inventoryItem2 = InventoryUI.Instance.arrows.currentItem;
                        List<Collider> list = new List<Collider>();
                        int num2 = 0;
                        while (num2 < inventoryItem.bowComponent.nArrows && !(InventoryUI.Instance.arrows.currentItem == null))
                        {
                            if(!DisableArrowConsumeConfig.Value)
                            inventoryItem2.amount--;


                            if (inventoryItem2.amount <= 0)
                            {
                                InventoryUI.Instance.arrows.currentItem = null;
                            }
                            Vector3 vector = PlayerMovement.Instance.playerCam.position + Vector3.down * 0.5f;
                            Vector3 vector2 = PlayerMovement.Instance.playerCam.forward;
                            float num3 = (float)inventoryItem.bowComponent.angleDelta;
                            vector2 = Quaternion.AngleAxis(-(num3 * (float)(inventoryItem.bowComponent.nArrows - 1)) / 2f + num3 * (float)num2, PlayerMovement.Instance.playerCam.up) * vector2;
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryItem2.prefab);
                            gameObject.GetComponent<Renderer>().material = inventoryItem2.material;
                            gameObject.transform.position = vector;
                            gameObject.transform.rotation = __instance.transform.rotation;
                            float num4 = (float)Hotbar.Instance.currentItem.attackDamage;
                            float num5 = (float)inventoryItem2.attackDamage;
                            // float projectileSpeed = inventoryItem.bowComponent.projectileSpeed;
                            //float projectileSpeed = 184f;
                            float projectileSpeed = ProjectileSpeedConfig.Value;
                            Rigidbody component = gameObject.GetComponent<Rigidbody>();
                            float num6 = 80f * num * projectileSpeed * PowerupInventory.Instance.GetRobinMultiplier(null);
                            component.AddForce(vector2 * num6);
                            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), PlayerMovement.Instance.GetPlayerCollider(), true);
                            float num7 = num5 * num4;
                            num7 *= num;
                            Arrow component2 = gameObject.GetComponent<Arrow>();
                            component2.damage = (int)(num7* PowerupInventory.Instance.GetRobinMultiplier(null));
                            component2.fallingWhileShooting = (!PlayerMovement.Instance.grounded && PlayerMovement.Instance.GetVelocity().y < 0f);
                            component2.speedWhileShooting = PlayerMovement.Instance.GetVelocity().magnitude;
                            component2.item = inventoryItem2;


                            ClientSend.ShootArrow(vector, vector2, (num6), inventoryItem2.id);



                            list.Add(component2.GetComponent<Collider>());
                            num2++;
                        }
                        foreach (Collider collider in list)
                        {
                            foreach (Collider collider2 in list)
                            {
                                Physics.IgnoreCollision(collider, collider2, true);
                            }
                        }
                        InventoryUI.Instance.arrows.UpdateCell();
                        CameraShaker.Instance.ChargeShake(num);

                        CooldownBar.Instance.ResetCooldownTime(ShootCooldownConfig.Value, true);

                        if(DisableShootSpamConfig.Value)
                        __instance.StartCoroutine(ShootCooldownCoroutine());

                        

                    }
                    
                   


                    return false;

                }

                return true;


            }

           


            public static IEnumerator ShootCooldownCoroutine()
            {
                //Print the time of when the function is first called.
                Debug.Log("Started Coroutine at timestamp : " + Time.time);
                ShootBuster.InCooldown = true;

                //yield on a new YieldInstruction that waits for 5 seconds.
                yield return new WaitForSeconds(ShootCooldownConfig.Value);

                //After we have waited 5 seconds print the time again.
                Debug.Log("Finished Coroutine at timestamp : " + Time.time);
                ShootBuster.InCooldown = false;
            }

           

           




        }


        

    }
}
