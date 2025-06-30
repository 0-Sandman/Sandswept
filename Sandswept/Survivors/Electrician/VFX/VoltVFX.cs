using System;
using System.Collections;
using System.Linq;
using EntityStates.Chef;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Sandswept.Survivors.Electrician.Achievements;
using Sandswept.Survivors.Electrician.Skills;
using Sandswept.Survivors.Electrician.States;
using Sandswept.Utils.Components;
using UnityEngine.SceneManagement;
using RoR2.Stats;
using UnityEngine;

namespace Sandswept.Survivors.Electrician.VFX
{
    public class VoltVFX
    {
        public static void Init()
        {
            VFX.GalvanicBolt.Init();
            Main.ran = false; // this is tempest sphere, temporary jank for unlinking prefab to modify it -- can't use static coroutines or workarounds
            VFX.StaticSnare.Init(); // tripwirecontroller also has static snare vfx -- it reuses galvanic bolt vfx
            VFX.SignalOverload.Init(); // this also contains the shield break vfx, because the signal overload entitystate also contains the shield break behavior
        }
    }
}
