using System.Collections.Generic;

namespace Network.PacketArgs
{
    public class GetProfileResponseArgs
    {
        public int AccountId { get; set; }
        public int Gold { get; set; }
        public int Diamonds { get; set; }
        public List<ProfileOption> Options { get; set; }
    }

    public class ProfileOption
    {
        public int Option { get; set; }
        public bool Available { get; set; }

        public static List<ProfileOption> CreateList()
        {
            var result = new List<ProfileOption>();
            for (int i = 0; i < 10; i++)
                result.Add(new ProfileOption());
            return result;
        }
    }
}