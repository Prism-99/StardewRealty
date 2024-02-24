using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Services
{
    internal abstract class IService : IConvertible
    {
        public delegate void RegisterEventHandlerArgs(string eventName, Action<object[]> callback);
        public event RegisterEventHandlerArgs OnRegisterEvent;
        public delegate void TriggerEventHandler(string eventName, object[] eventParameters);
        public event TriggerEventHandler OnTriggerEvent;

        internal ILoggerService logger;
        public virtual List<string> CustomServiceEventSubscripitions { get; }
        public virtual List<string> CustomServiceEventTriggers { get; }
        public abstract Type ServiceType { get; }
        public abstract Type[] InitArgs { get; } 
        public virtual Type[] Dependants { get; } = null;
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }
        protected void TriggerEvent(string eventName, object[] eventParameters)
        {
            OnTriggerEvent(eventName, eventParameters);
        }
        protected void RegisterEventHandler(string eventName, Action<object[]> callback)
        {
            OnRegisterEvent(eventName, callback);
        }
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public abstract object ToType(Type conversionType, IFormatProvider provider);

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        internal abstract void Initialize(ILoggerService logger, object[] args);
    }
}
