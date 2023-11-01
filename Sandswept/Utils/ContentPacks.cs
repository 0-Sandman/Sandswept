using RoR2.ContentManagement;
using RoR2;
using System.Collections;
using UnityEngine;

namespace Sandswept.Utils
{
    public class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();

        public string identifier => "Sandswept";

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            //contentPack.buffDefs.Add(Main.Buffs.ToArray());
            contentPack.unlockableDefs.Add(Main.Unlocks.ToArray());
            contentPack.effectDefs.Add(Main.EffectPrefabs.ConvertAll((GameObject x) => new EffectDef(x)).ToArray());
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}