using R2API.Networking;
using R2API.Networking.Interfaces;

namespace Sandswept.Utils
{
    public class CallNetworkedMethod : INetMessage
    {
        private GameObject obj;
        private string method;
        private uint id;
        public CallNetworkedMethod(GameObject obj, string method)
        {
            this.obj = obj;
            this.method = method;
            this.id = obj.GetComponent<NetworkIdentity>().netId.Value;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(id);
            writer.Write(method);
        }

        public void Deserialize(NetworkReader reader)
        {
            id = reader.ReadUInt32();
            method = reader.ReadString();
        }

        public void OnReceived()
        {
            obj = Util.FindNetworkObject(new NetworkInstanceId(id));

            if (obj)
            {
                obj.SendMessage(method, SendMessageOptions.DontRequireReceiver);
            }

        }

        public CallNetworkedMethod()
        {

        }
    }
}