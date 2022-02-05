using System;
using Common.Network;
using Newtonsoft.Json;

namespace Common.Entities
{
    public abstract class DataFieldBase
    {
        [JsonProperty] protected          object      Value          { get; set; }
        [JsonProperty] public             ushort      FieldId        { get; set; }
        [JsonProperty] public             DateTime    LastUpdateTime => LastUpdate;
        [JsonIgnore]   public             int         AccountId      { get; set; }
        [JsonIgnore]   protected          DateTime    LastUpdate;
        [JsonIgnore]   protected readonly IGameClient GameClient;

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
        
        protected DataFieldBase(
            IGameClient _GameClient, 
            int _AccountId, 
            ushort _FieldId,
            object _Value, 
            DateTime _LastUpdate)
        {
            GameClient = _GameClient;
            AccountId = _AccountId;
            FieldId = _FieldId;
            Value = _Value;
            LastUpdate = _LastUpdate;
        }
    }
}