﻿using NuClear.Model.Common.Entities;

namespace NuClear.Replication.OperationsProcessing.Metadata.Model.EntityTypes
{
    public class EntityTypePhonecall : EntityTypeBase<EntityTypePhonecall>
    {
        public override int Id
        {
            get { return EntityTypeIds.Phonecall; }
        }

        public override string Description
        {
            get { return "Phonecall"; }
        }
    }
}