﻿using System;

using NuClear.Replication.Core;
using NuClear.StateInitialization.Core.Storage;

namespace NuClear.StateInitialization.Core.Commands
{
    public enum ExecutionMode
    {
        Sequential = 1,
        Parallel,
        ShadowParallel
    }

    [Flags]
    public enum DbManagementMode
    {
        None = 0,
        DropAndRecreateViews = 1,
        DropAndRecreateConstraints = 2,
        EnableIndexManagment = 4,
        UpdateTableStatistics = 8,
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ReplicateInBulkCommand : ICommand
    {
        private static readonly TimeSpan DefaultBulkCopyTimeout = TimeSpan.FromMinutes(30);

        public ReplicateInBulkCommand(
            StorageDescriptor sourceStorageDescriptor,
            StorageDescriptor targetStorageDescriptor,
            DbManagementMode databaseManagementMode = DbManagementMode.DropAndRecreateConstraints | DbManagementMode.EnableIndexManagment | DbManagementMode.UpdateTableStatistics,
            ExecutionMode executionMode = ExecutionMode.Parallel,
            TimeSpan? bulkCopyTimeout = null)
        {
            SourceStorageDescriptor = sourceStorageDescriptor;
            TargetStorageDescriptor = targetStorageDescriptor;
            DbManagementMode = databaseManagementMode;
            ExecutionMode = executionMode;
            BulkCopyTimeout = bulkCopyTimeout ?? DefaultBulkCopyTimeout;
        }

        public StorageDescriptor SourceStorageDescriptor { get; }
        public StorageDescriptor TargetStorageDescriptor { get; }
        public DbManagementMode DbManagementMode { get; }
        public ExecutionMode ExecutionMode { get; }
        public TimeSpan BulkCopyTimeout { get; }
    }
}
