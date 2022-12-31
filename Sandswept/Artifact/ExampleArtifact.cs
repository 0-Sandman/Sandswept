/*using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Sandswept.Main;

namespace Sandswept.Artifact
{
    internal class ExampleArtifact : ArtifactBase<ExampleArtifact>
    {
        public static ConfigEntry<int> TimesToPrintMessageOnStart;

        public override string ArtifactName => "Artifact of Example";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_EXAMPLE";

        public override string ArtifactDescription => "When enabled, print a message to the chat at the start of the run.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("ExampleArtifactEnabledIcon.png");

        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("ExampleArtifactDisabledIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            TimesToPrintMessageOnStart = config.Bind<int>("Artifact: " + ArtifactName, "Times to Print Message in Chat", 5, "How many times should a message be printed to the chat on run start?");
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += PrintMessageToChat;
        }

        private void PrintMessageToChat(Run run)
        {
            if (NetworkServer.active && ArtifactEnabled)
            {
                for (int i = 0; i < TimesToPrintMessageOnStart.Value; i++)
                {
                    Chat.AddMessage("Example Artifact has been Enabled.");
                }
            }
        }
    }
}*/
