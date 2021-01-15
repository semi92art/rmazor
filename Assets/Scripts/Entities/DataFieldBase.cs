using System;

namespace Entities
{
    public abstract class DataFieldBase
    {
        public bool IsSaving { get; protected set; }
        public readonly ushort FieldId;
        protected readonly int AccountId;
        protected object Value;
        protected DateTime LastUpdate;
        
        public bool      GetBool()      => Convert.ToBoolean(Value);
        public string    GetString()    => Convert.ToString(Value);
        public int       GetInt()       => Convert.ToInt32(Value);
        public long      GetLong()      => Convert.ToInt64(Value);
        public float     GetFloat()     => Convert.ToSingle(Value);
        public double    GetDouble()    => Convert.ToDouble(Value);
        public decimal   GetDecimal()   => Convert.ToDecimal(Value);
        public DateTime  GetDateTime()  => Convert.ToDateTime(Value);

        public DateTime LastUpdateTime => LastUpdate;

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