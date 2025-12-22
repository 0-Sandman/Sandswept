using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandswept.Utils
{
    public class Projectile
    {
        public static void BlacklistAttackDirectionFix(GameObject projectile)
        {
            if (!Main.AttackDirectionFixLoaded)
            {
                // Main.ModLogger.LogError("attack direction fix wasnt loaded");
                return;
            }

            BlacklistAttackDirectionFixInternal(projectile);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void BlacklistAttackDirectionFixInternal(GameObject projectile)
        {
            // Main.ModLogger.LogError("trying to blacklist");
            if (!AttackDirectionFix.ProjectileAttributes.IsProjectileBlacklisted(ProjectileCatalog.GetProjectileIndex(projectile)))
            {
                // Main.ModLogger.LogError($"projectile {projectile.name} wasnt already blacklisted so blacklisting it");
                AttackDirectionFix.ProjectileAttributes.BlacklistProjectile(projectile);
            }
        }
    }
}