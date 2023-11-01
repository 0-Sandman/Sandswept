using UnityEngine.Networking;

namespace Sandswept2.Utils
{
    public static class NetworkingHelpers
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
    }
}