using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;

namespace AdsConsoleApp
{
    internal class SumHandleRawRead : SumRead
    {
        protected SumHandleRawRead(IAdsConnection connection, SumAccessMode readWriteMode) : base(connection, readWriteMode)
        {
        }

        public SumHandleRawRead(IAdsConnection connection, uint[] serverHandles, uint[] length)
            : base(connection, SumAccessMode.IndexGroupIndexOffset)
        {
            List<SumDataEntity> entities = new List<SumDataEntity>();

            for (int i = 0; i < serverHandles.Length; i++)
            {
                entities.Add(new Entity(serverHandles[i], (int)length[i], 0));
			}

            sumEntities = entities;
		}

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            Entity handleSumEntity = (Entity)entity;
            return MarshalSumReadHeader(61445u, handleSumEntity.Handle, entity.ReadLength, writer);
        }
    }

    internal class Entity : SumDataEntity
    {
        public uint Handle { get; }

        public Entity(uint handle, int readLength, int writeLength) 
            : base(readLength,writeLength)
        {
            Handle = handle;
        }
    }
}
