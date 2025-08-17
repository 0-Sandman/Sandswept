namespace Sandswept.Items.Greens
{
    public class PlatingManager : NetworkBehaviour
    {
        [SyncVar]
        public float CurrentPlating = 0;
        [SyncVar]
        public float MaxPlating = 0;
    }
}
