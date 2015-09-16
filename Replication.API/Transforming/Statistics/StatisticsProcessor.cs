using System.Collections.Generic;

using NuClear.AdvancedSearch.Replication.API.Specifications;
using NuClear.Storage.Readings;

namespace NuClear.AdvancedSearch.Replication.API.Transforming.Statistics
{
    public class StatisticsProcessor<T> : IStatisticsProcessor 
        where T : class
    {
        private readonly IQuery _query;
        private readonly IBulkRepository<T> _repository;
        private readonly StatisticsInfo<T> _metadata;
        private readonly DataChangesDetector<T, T> _changesDetector;

        public StatisticsProcessor(StatisticsInfo<T> metadata, IQuery query, IBulkRepository<T> repository)
        {
            _query = query;
            _repository = repository;
            _metadata = metadata;
            _changesDetector = new DataChangesDetector<T, T>(_metadata.MapSpecificationProviderForSource, _metadata.MapSpecificationProviderForTarget, _query);
        }

        public void RecalculateStatistics(long projectId, IReadOnlyCollection<long?> categoryIds)
        {
            var filter = _metadata.FindSpecificationProvider.Invoke(projectId, categoryIds);

            // ������� ����������� �������� ������������� ������,
            // ����� �������� �� �� �������������, ������� ��������� �� ��������������.
            var intermediateResult = _changesDetector.DetectChanges(Specs.Map.ZeroMapping<T>(), filter, _metadata.FieldComparer);
            var changes = MergeTool.Merge(intermediateResult.Difference, intermediateResult.Complement);

            // ������� ��� ���������� ���������� - �� ����� ��������� ��� ������� ������� � ����.
            // ������� ������ ����������.
            _repository.Update(changes.Intersection);
        }
    }
}