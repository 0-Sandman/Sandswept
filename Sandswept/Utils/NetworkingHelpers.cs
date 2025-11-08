using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace Sandswept.Utils
{
    public static class NetworkingUtils
    {
        /// <summary>
        /// Attempts to reconstruct a network object reference from just its network id value.
        /// </summary>
        /// <typeparam name="T">Any type of object.</typeparam>
        /// <param name="netIdValue">The netId value of the object to retrieve. This can usually be found on its network identity.</param>
        /// <returns>The object if it can find it, else a default value for type T.</returns>
        public static T GetObjectFromNetIdValue<T>(uint netIdValue)
        {
            NetworkInstanceId netInstanceId = new NetworkInstanceId(netIdValue);
            NetworkIdentity foundNetworkIdentity = null;
            if (NetworkServer.active)
            {
                NetworkServer.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
            }
            else
            {
                ClientScene.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
            }

            if (foundNetworkIdentity)
            {
                T foundObject = foundNetworkIdentity.GetComponent<T>();
                if (foundObject != null)
                {
                    return foundObject;
                }
            }

            return default(T);
        }

        public static void SetBuffCountSynced(this CharacterBody body, BuffIndex index, int count) {
            if (NetworkServer.active) {
                body.SetBuffCount(index, count);
            }
            else {
                new SetBuffCountMessage(body, index, count).Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public static void AddBuffSynced(this CharacterBody body, BuffIndex index) {
            if (NetworkServer.active) {
                body.AddBuff(index);
            }
            else {
                new AddBuffMessage(body, index).Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public static void RemoveBuffSynced(this CharacterBody body, BuffIndex index) {
            if (NetworkServer.active) {
                body.RemoveBuff(index);
            }
            else {
                new RemoveBuffMessage(body, index).Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public static void AddTimedBuffSynced(this CharacterBody body, BuffIndex index, int duration) {
            if (NetworkServer.active) {
                body.AddTimedBuff(index, duration);
            }
            else {
                new AddTimedBuffMessage(body, index, duration).Send(R2API.Networking.NetworkDestination.Server);
            }
        }

        public static void RegisterMessages() {
            NetworkingAPI.RegisterMessageType<SetBuffCountMessage>();
            NetworkingAPI.RegisterMessageType<AddTimedBuffMessage>();
            NetworkingAPI.RegisterMessageType<AddBuffMessage>();
            NetworkingAPI.RegisterMessageType<RemoveBuffMessage>();
        }

        private class AddTimedBuffMessage : INetMessage
        {
            public CharacterBody body;
            public BuffIndex index;
            public float duration;
            void INetMessage.OnReceived()
            {
                if (body) {
                    body.AddTimedBuff(index, duration);
                }
            }
            void ISerializableObject.Deserialize(NetworkReader reader)
            {
                body = reader.ReadGameObject()?.GetComponent<CharacterBody>() ?? null;
                index = reader.ReadBuffIndex();
                duration = reader.ReadSingle();
            }
            void ISerializableObject.Serialize(NetworkWriter writer)
            {
                writer.Write(body.gameObject);
                writer.WriteBuffIndex(index);
                writer.Write(duration);
            }
            
            public AddTimedBuffMessage(CharacterBody body, BuffIndex index, float duration) {
                this.body = body;
                this.index = index;
                this.duration = duration;
            }
            public AddTimedBuffMessage() {}
        }

        private class RemoveBuffMessage : INetMessage
        {
            public CharacterBody body;
            public BuffIndex index;
            void INetMessage.OnReceived()
            {
                if (body) {
                    body.AddBuff(index);
                }
            }
            void ISerializableObject.Deserialize(NetworkReader reader)
            {
                body = reader.ReadGameObject()?.GetComponent<CharacterBody>() ?? null;
                index = reader.ReadBuffIndex();
            }
            void ISerializableObject.Serialize(NetworkWriter writer)
            {
                writer.Write(body.gameObject);
                writer.WriteBuffIndex(index);
            }
            
            public RemoveBuffMessage(CharacterBody body, BuffIndex index) {
                this.body = body;
                this.index = index;
            }
            public RemoveBuffMessage() {}
        }

        private class AddBuffMessage : INetMessage
        {
            public CharacterBody body;
            public BuffIndex index;
            void INetMessage.OnReceived()
            {
                if (body) {
                    body.AddBuff(index);
                }
            }
            void ISerializableObject.Deserialize(NetworkReader reader)
            {
                body = reader.ReadGameObject()?.GetComponent<CharacterBody>() ?? null;
                index = reader.ReadBuffIndex();
            }
            void ISerializableObject.Serialize(NetworkWriter writer)
            {
                writer.Write(body.gameObject);
                writer.WriteBuffIndex(index);
            }
            
            public AddBuffMessage(CharacterBody body, BuffIndex index) {
                this.body = body;
                this.index = index;
            }
            public AddBuffMessage() {}
        }

        private class SetBuffCountMessage : INetMessage
        {
            public CharacterBody body;
            public BuffIndex index;
            public int count;
            void INetMessage.OnReceived()
            {
                if (body) {
                    body.SetBuffCount(index, count);
                }
            }
            void ISerializableObject.Deserialize(NetworkReader reader)
            {
                body = reader.ReadGameObject()?.GetComponent<CharacterBody>() ?? null;
                index = reader.ReadBuffIndex();
                count = reader.ReadInt32();
            }
            void ISerializableObject.Serialize(NetworkWriter writer)
            {
                writer.Write(body.gameObject);
                writer.WriteBuffIndex(index);
                writer.Write(count);
            }
            
            public SetBuffCountMessage(CharacterBody body, BuffIndex index, int count) {
                this.body = body;
                this.index = index;
                this.count = count;
            }
            public SetBuffCountMessage() {}
        }
    }
}