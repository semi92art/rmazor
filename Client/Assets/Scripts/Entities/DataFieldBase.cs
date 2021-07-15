using System;
using Newtonsoft.Json;

namespace Entities
{
    public abstract class DataFieldBase
    {
        [JsonProperty] protected object Value { get; set; }
        [JsonProperty] public ushort FieldId { get; set; }
        [JsonProperty] public DateTime LastUpdateTime => LastUpdate;
        [JsonIgnore] public virtual bool IsSaving { get; protected set; }
        [JsonIgnore] public readonly int AccountId;
        [JsonIgnore] protected DateTime LastUpdate;

        public object             GetValue()    => Value;
        public bool               ToBool()      => Convert.ToBoolean(Value);
        public override string    ToString()    => Convert.ToString(Value);
        public int                ToInt()       => Convert.ToInt32(Value);
        public long               ToLong()      => Convert.ToInt64(Value);
        public float              ToFloat()     => Convert.ToSingle(Value);
        public double             ToDouble()    => Convert.ToDouble(Value);
        public decimal            ToDecimal()   => Convert.ToDecimal(Value);
        public DateTime           ToDateTime()  => Convert.ToDateTime(Value);

        protected DataFieldBase() { }
        
        protected DataFieldBase(int _AccountId, ushort _FieldId, object _Value, DateTime _LastUpdate)
        {
            AccountId = _AccountId;
            FieldId = _FieldId;
            Value = _Value;
            LastUpdate = _LastUpdate;
        }
    }
}