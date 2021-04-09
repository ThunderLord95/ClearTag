using BepInEx;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System.Collections;

namespace ClearTag
{
    [BepInPlugin("org.bepinex.plugins.ClearTag", "ClearTag", "1.0.0.0")]




    public class MyPatcher : BaseUnityPlugin
    {


        public void Awake()
        {
            var harmony = new Harmony("org.thunder.monkeytag.cleartag");
            harmony.PatchAll(Assembly.GetExecutingAssembly()); // Patches The Mod Into the Game I think?
        }
        [HarmonyPatch(typeof(GorillaTagManager))]
        [HarmonyPatch("Update", 0)]// How frequent it checks I think?
        class ClearTag : MonoBehaviour
        {
            static void Prefix(GorillaTagManager __instance) // Allows getting/changing variables and using functions from GorillaTagManager
            {
                bool secondaryDown = false;
                bool primaryDown = false;
                if (!PhotonNetwork.CurrentRoom.IsVisible || !PhotonNetwork.InRoom) // If you're in a room and the room is private allow the mod
                {
                    List<InputDevice> list = new List<InputDevice>(); // Make a list for current controllers
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);  // Put Left Controller in List
                    list[0].TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryDown);  // Checking if Secondary Has been Pressed
                    list[0].TryGetFeatureValue(CommonUsages.primaryButton, out primaryDown);  // Checking if Primary Has been Pressed

                    if (secondaryDown) //If button has been pressed
                    {
                        if (!__instance.isCurrentlyTag || __instance.currentIt != null) // If the game isn't Tag or if there is a Tagger allow the code
                        {
                            int num = PhotonNetwork.PlayerList.Length; // Get amount of players in lobby
                            if (num < 4)
                            {
                                ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable(); // Hashtable to store material index
                                hashtable2.Add("matIndex", 0); // Material Index: 0 == normal material
                                if (__instance.currentIt != null) // If there is an "it" player change their material to normal
                                {
                                    __instance.currentIt.SetCustomProperties(hashtable2, null, null);
                                }
                                ExitGames.Client.Photon.Hashtable hashtable3 = new ExitGames.Client.Photon.Hashtable(); // Hashtable to store currentRoom custom settings
                                hashtable3.Add("currentIt", null); // Set the current "it" player to null which basically means nothing
                                __instance.currentRoom.SetCustomProperties(hashtable3, null, null); // Change the room settings
                                __instance.lastTag = (double)Time.time;
                                __instance.currentIt = null; // Update client currentIt
                                __instance.UpdateTagState(); // Sets players changed to normal, to the normal speed and disable their tagging
                            }
                            if (num >= 4)
                            {
                                __instance.ClearInfectionState(); // Get rid of red taggers
                                __instance.lastInfectedPlayer = null; // Set the last player tagged to nothign
                                __instance.SetisCurrentlyTag(true); // Change gamemode to tag

                                // Repeat the same steps we did last time since the gamemode is now tag and there are no infected
                                ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
                                hashtable2.Add("matIndex", 0);
                                if (__instance.currentIt != null)
                                {
                                    __instance.currentIt.SetCustomProperties(hashtable2, null, null);
                                }
                                ExitGames.Client.Photon.Hashtable hashtable3 = new ExitGames.Client.Photon.Hashtable();
                                hashtable3.Add("currentIt", null);
                                __instance.currentRoom.SetCustomProperties(hashtable3, null, null);
                                __instance.lastTag = (double)Time.time;
                                __instance.currentIt = null;
                                __instance.UpdateTagState();
                            }
                        }
                    }
                    if ((primaryDown && __instance.currentIt == null) || (primaryDown && __instance.currentInfected == null)) // Check if any taggers or Infected, and if not call UpdateState function
                    {
                        int num = PhotonNetwork.PlayerList.Length;
                        if (num < 4)
                        {
                            __instance.UpdateState();
                        }
                        else if(num >= 4)
                        {
                            __instance.SetisCurrentlyTag(false);
                            __instance.UpdateState();
                        }
                    }
                }
            }
        }
    }
}