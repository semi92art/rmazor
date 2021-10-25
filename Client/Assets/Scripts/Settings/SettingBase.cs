using System.Collections.Generic;
using Entities;
using Utils;

namespace Settings
{
    public abstract class SettingBase<T> : ISetting<T>
    {
        public abstract SaveKey Key { get; }
        public abstract string TitleKey { get; }
        public abstract ESettingLocation Location { get; }
        public abstract ESettingType Type { get; }
        public virtual List<T> Values => null;
        public virtual object Min => null;
        public virtual object Max => null;
        public virtual string SpriteOnKey => null;
        public virtual string SpriteOffKey => null;
        public virtual T Get() => SaveUtils.GetValue<T>(Key);
        public virtual void Put(T _VolumeOn) => SaveUtils.PutValue(Key, _VolumeOn);
    }
}