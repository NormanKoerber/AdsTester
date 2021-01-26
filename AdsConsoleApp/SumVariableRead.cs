using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;

namespace AdsConsoleApp
{
    public class SumVariableRead : SumRead
    {
        private readonly IList<AdsReadVariable> _variables;

        public SumVariableRead(IAdsConnection connection, IEnumerable<AdsReadVariable> variables)
            : base(connection, SumAccessMode.IndexGroupIndexOffset)
        {
            if (connection is null) throw new ArgumentNullException(nameof(connection));
            _variables = variables?.ToList() ?? throw new ArgumentNullException(nameof(variables));
            if (variables.Any(v => v == null)) throw new ArgumentException($"{nameof(variables)} must not contain null items.");

            var entities = new List<SumDataEntity>();

            foreach (var variable in variables)
            {
                uint handle = CreateVariableHandle(variable);
                entities.Add(new Entity(handle, variable.Size, 0));
            }

            sumEntities = entities;
        }

        private uint CreateVariableHandle(AdsReadVariable variable)
        {
            try
            {
                return Connection.CreateVariableHandle(variable.Name);
            }
            catch (AdsErrorException ex) when (ex.ErrorCode == AdsErrorCode.DeviceSymbolNotFound)
            {
                throw new AdsException($"Could not create handle for '{variable.Name}'.", ex);
            }
        }

        protected SumVariableRead(IAdsConnection connection, SumAccessMode readWriteMode) 
            : base(connection, readWriteMode)
        {
        }

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            Entity handleSumEntity = (Entity)entity;
            return MarshalSumReadHeader((uint)AdsReservedIndexGroup.SymbolValueByHandle, handleSumEntity.Handle, entity.ReadLength, writer);
        }

        public void ReadVariables()
        {
            var rawValues = ReadRaw();
            for (int i = 0; i < _variables.Count; i++)
            {
                _variables[i].UpdateRawData(rawValues[i]);
            }
        }


        private class Entity : SumDataEntity
        {
            public Entity(uint handle, int readLength, int writeLength)
                : base(readLength, writeLength)
            {
                Handle = handle;
            }

            public uint Handle { get; }
        }
    }

    public abstract class AdsVariable
    {
        public string Name { get; set; }

        public ushort Size { get; set; }

        public byte[] RawData { get; protected set; }

        public override string ToString() => $"{Name}: {string.Join(", ", RawData)}";
    }

    public class AdsReadVariable : AdsVariable
    {
        public event EventHandler<RawValueChangedEventArgs> RawValuesChanged;

        internal void UpdateRawData(byte[] rawData)
        {
            RawData = rawData;
            RawValuesChanged?.Invoke(this, new RawValueChangedEventArgs(rawData));
        }
    }

    public class RawValueChangedEventArgs : EventArgs
    {
        public RawValueChangedEventArgs(byte[] rawData)
        {
            RawData = rawData;
        }

        internal byte[] RawData { get; }
    }
}