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
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        [HarmonyPatch(typeof(GorillaTagManager))]
        [HarmonyPatch("Update", 0)]
        class ClearTag : MonoBehaviour
        {
            static void Prefix(GorillaTagManager __instance)
            {
                bool secondaryDown = false;
                bool primaryDown = false;
                bool flag = !PhotonNetwork.CurrentRoom.IsVisible || !PhotonNetwork.InRoom;
                if (flag)
                {
                    List<InputDevice> list = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                    list[0].TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryDown);
                    list[0].TryGetFeatureValue(CommonUsages.primaryButton, out primaryDown);

                    if (secondaryDown)
                    {
                        if (__instance.isCurrentlyTag)
                        {
                            __instance.ClearInfectionState();
                            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                            {
                               if (__instance.FindVRRigForPlayer(player) != null)
                                {
                                    __instance.isCurrentlyTag = false;
                                    __instance.currentIt = null;
                                    __instance.currentInfected.Clear();
                                }
                                __instance.UpdateTagState();
                                __instance.UpdateInfectionState();
                            }
                        }
                    }
                }
            }
        }
    }
}