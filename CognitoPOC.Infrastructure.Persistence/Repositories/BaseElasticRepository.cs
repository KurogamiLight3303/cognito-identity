using System.Data;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Persistence.Repositories
{
    public abstract class BaseElasticRepository<TIndex>(IOptions<ElasticConfiguration> config, ILogger logger)
        where TIndex : class
    {
        private ElasticClient? _client;
        private readonly ElasticConfiguration _configuration = config.Value ?? throw new ArgumentNullException(nameof(config));
        protected ElasticClient Client
        {
            get
            {
                if (_client != null)
                    return _client;
                var pool = new SingleNodeConnectionPool(new(_configuration.Url!));
                var connectionSettings = new ConnectionSettings(pool, JsonNetSerializer.Default).DefaultIndex(_configuration.DefaultIndex);

                if (!string.IsNullOrEmpty(_configuration.Username) && !string.IsNullOrEmpty(_configuration.Password))
                    connectionSettings.BasicAuthentication(_configuration.Username, _configuration.Password);
                if (_configuration.Debug)
                    connectionSettings.DisableDirectStreaming();
                _client = new(connectionSettings);
            
                EnsureIndexWithMapping<TIndex>(IndexName);
                return _client;
            }
        }
        

        protected abstract string IndexName { get; }
        protected readonly ILogger Logger = logger;

        public async Task AddAsync(TIndex model, CancellationToken cancellationToken = default, bool updateIndex = false)
        {
            try
            {
                var esResponse = await Client.IndexDocumentAsync(model, cancellationToken);
                if (esResponse.Result != Result.Created)
                    throw new($"Document not added {esResponse.Result} {esResponse.ServerError?.Error?.Reason ?? string.Empty}");
                if (updateIndex)
                    await RefreshAsync(cancellationToken);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to add model {Model}", model);
                throw;
            }
        }
        public void Add(TIndex model, bool updateIndex = false)
        {
            var task = Task.Run(async () => await AddAsync(model, updateIndex: updateIndex));
            task.Wait();
        }
        public async Task UpdateAsync(TIndex model, CancellationToken cancellationToken = default, bool updateIndex = false)
        {
            try
            {
                var esResponse = await Client.UpdateAsync<TIndex>(model, u => u
                    .DetectNoop(false)
                    .Doc(model), cancellationToken);
                if (esResponse.Result != Result.Updated)
                    throw new("Document not updated: " + esResponse.DebugInformation);
                if(updateIndex)
                    await RefreshAsync(cancellationToken);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to update model {Model}", model);
                throw;
            }
        }
        public void Update(TIndex model, bool updateIndex = false)
        {
            var task = Task.Run(async () => await UpdateAsync(model, updateIndex: updateIndex));
            task.Wait();
        }

        public async Task DeleteAsync(TIndex model, CancellationToken cancellationToken = default) 
        {
            try
            {
                var esResponse = await Client.DeleteAsync<TIndex>(model, null, cancellationToken);
                if (esResponse.Result != Result.Deleted)
                    throw new("Document not deleted");
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to delete  {Model}", model);
                throw;
            }
        }
        public void Delete(TIndex model) {
            Task.Run(async () => await DeleteAsync(model));
        }

        public async Task<bool> DeleteListAsync(IEnumerable<TIndex> model, CancellationToken cancellationToken = default)
        {
            var result = false;
            try {
                var esResponse = await Client.BulkAsync(b => b
                    .DeleteMany(model)
                    .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);
                result = !esResponse.Errors;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to delete list model {Model}", model);
            }
            return result;
        }
        public bool DeleteList(IEnumerable<TIndex> model)
        {
            var task = Task.Run(async () => await DeleteListAsync(model));
            return task.Result;
        }
        public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default) {
            var result = false;
            try {
                if (Client == null) throw new NoNullAllowedException("The Elastic client has not been initialized");
                var response =await Client.RefreshAsync(IndexName,null,cancellationToken);
                if (response.IsValid) result = true;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to refresh");
            }
            return result;
        }

        public bool Refresh() {
            var task = Task.Run(async () => await RefreshAsync());
            return task.Result;
        }

        public long Count(Func<CountDescriptor<TIndex>, ICountRequest>? selector = null)
        {
            try
            {
                var esResponse = Client.Count(selector);
                if (!esResponse.IsValid)
                    throw new("Invalid count");
                return esResponse.Count;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to count");
                return 0;
            }
        }
        public async Task<long> CountAsync(Func<CountDescriptor<TIndex>, ICountRequest>? selector = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var esResponse = await Client.CountAsync(selector, cancellationToken);
                if (!esResponse.IsValid)
                    throw new("Invalid count");
                return esResponse.Count;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to count");
                return 0;
            }
        }

        public async Task<IReadOnlyCollection<TIndex>> GetAllAsync(Func<QueryContainerDescriptor<TIndex>, QueryContainer> qry, Func<SortDescriptor<TIndex>, IPromise<IList<ISort>>>? selector = null, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<TIndex>? answer = null;
            try
            {
                var esResponse = await Client.SearchAsync<TIndex>(x => x
                    .Query(qry)
                    .Size(100)
                    .Sort(selector)
                , cancellationToken);

                if (esResponse.IsValid) answer =  esResponse.Documents;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Unable to get all");
            }
            finally
            {
                answer ??= new List<TIndex>().AsReadOnly(); 
            }
            return answer;
        }
        public async Task<TIndex?> GetFirstOrDefaultAsync(Func<QueryContainerDescriptor<TIndex>, QueryContainer> qry, Func<SortDescriptor<TIndex>, IPromise<IList<ISort>>>? selector = null, CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync(qry, selector, cancellationToken)).FirstOrDefault();
        }
        public TIndex? GetFirstOrDefault(Func<QueryContainerDescriptor<TIndex>, QueryContainer> qry, Func<SortDescriptor<TIndex>, IPromise<IList<ISort>>>? selector = null)
        {
            var task = GetFirstOrDefaultAsync(qry, selector);
            task.Wait();
            return task.Result;
        }
        public async Task<TIndex?> GetLastOrDefaultAsync(Func<QueryContainerDescriptor<TIndex>, QueryContainer> qry, Func<SortDescriptor<TIndex>, IPromise<IList<ISort>>>? selector = null, CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync(qry, selector, cancellationToken)).LastOrDefault();
        }

        public void RemoveRange(Func<DeleteByQueryDescriptor<TIndex>, IDeleteByQueryRequest> selector)
        {
            var esResponse = Client.DeleteByQuery(selector);
            if (!esResponse.IsValid)
                throw new(esResponse.ServerError?.Error.Reason ?? I18n.UnknownError);
        }

        public async Task RemoveRangeAsync(Func<DeleteByQueryDescriptor<TIndex>, IDeleteByQueryRequest> selector, CancellationToken cancellationToken = default)
        {
            var esResponse = await Client.DeleteByQueryAsync(selector, cancellationToken);
            if (!esResponse.IsValid)
                throw new(esResponse.ServerError?.Error.Reason ?? I18n.UnknownError);
        }

        private void EnsureIndexWithMapping<T>(string? indexName = null, Func<PutMappingDescriptor<TIndex>, PutMappingDescriptor<TIndex>>? customMapping = null)
        {
            {
                try
                {
                    //var methodName = GetActualAsyncMethodName();

                    if (string.IsNullOrEmpty(indexName)) return;

                    Client.ConnectionSettings.DefaultIndices.Add(typeof(T), indexName);

                    //var indexExistsResponse = _client.Indices(new IndexExistsRequest(indexName));
                    var indexExistsResponse = Client.IndexExists(new IndexExistsRequest(indexName));
                    //if (!indexExistsResponse.IsValid)
                    //    BackgroundJob.Enqueue(() => Log(NotificationType.Fatal, methodName, "Verifying index existence",
                    //        indexExistsResponse.DebugInformation));

                    if (indexExistsResponse.Exists) return;

                    Client.CreateIndex(indexName);
                    //var createIndexRes = _client.Indices.Create(indexName);
                    //if (!createIndexRes.IsValid)
                    //BackgroundJob.Enqueue(() => Log(NotificationType.Fatal, methodName, "Creating index",
                    //    createIndexRes.DebugInformation));

                    Client.Map<TIndex>(m =>
                    {
                        m.AutoMap().Index(indexName);
                        if (customMapping != null) m = customMapping(m);
                        return m;
                    });

                    //if (!res.IsValid)
                    //    BackgroundJob.Enqueue(() =>
                    //        Log(NotificationType.Fatal, methodName, "Mapping entity", res.DebugInformation));
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, "Unable to create index");
                }
            }
        }
    }
}
