using System;
using System.Collections.Generic;
using System.Text;

namespace Sandswept.Survivors.Ranger.Achievements
{
    [RegisterAchievement("RangerClearGameMonsoon", "Skins.Ranger.Sandswept", null, 10, null)]
    public class Mastery : BaseMasteryAchievement
    {
        [SystemInitializer(typeof(HG.Reflection.SearchableAttribute.OptInAttribute))]
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("RangerBody");
        }
    }
}