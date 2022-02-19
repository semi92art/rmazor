using Newtonsoft.Json;

namespace Common.Managers.Scores
{
    public static class ScoreManagerUtils
    {
        public static byte[] ToByteArray<T>(T _Obj) where T : class
        {
            if(_Obj.Equals(default(T)))
                return default;
            string s = ToString(_Obj);
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        public static T FromByteArray<T>(byte[] _Data) where T : class
        {
            if(_Data == null)
                return default;
            string s = System.Text.Encoding.ASCII.GetString(_Data);
            return FromString<T>(s);
        }

        private static string ToString<T>(T _Obj) where T : class
        {
            return _Obj == null ? null : JsonConvert.SerializeObject(_Obj);
        }

        private static T FromString<T>(string _Data) where T : class
        {
            return JsonConvert.DeserializeObject<T>(_Data);
        }
    }
}