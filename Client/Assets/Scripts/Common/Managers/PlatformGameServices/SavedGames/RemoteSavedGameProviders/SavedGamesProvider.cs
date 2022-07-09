using System.Text;
using Common.Entities;
using Common.Helpers;
using Newtonsoft.Json;

namespace Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders
{
    public abstract class RemoteSavedGameProviderBase : InitBase, IRemoteSavedGameProvider
    {
        #region nonpublic members

        #endregion

        #region inject

        #endregion

        #region api

        public abstract Entity<object> GetSavedGame(string    _FileName);
        public abstract void           SaveGame<T>(T          _Data) where T : FileNameArgs;
        public abstract void           DeleteSavedGame(string _FileName);
        public abstract void           FetchSavedGames();

        #endregion

        #region nonpublic methods
        
        protected static byte[] ToByteArray<T>(T _Obj) where T : class
        {
            if(_Obj.Equals(default(T)))
                return default;
            string s = ToString(_Obj);
            return Encoding.ASCII.GetBytes(s);
        }

        protected static T FromByteArray<T>(byte[] _Data) where T : class
        {
            if(_Data == null)
                return default;
            string s = Encoding.ASCII.GetString(_Data);
            return FromString<T>(s);
        }
        
        protected static string ToString<T>(T _Obj) where T : class
        {
            return _Obj == null ? null : JsonConvert.SerializeObject(_Obj);
        }

        protected static T FromString<T>(string _Data) where T : class
        {
            return JsonConvert.DeserializeObject<T>(_Data);
        }

        #endregion


    }
}