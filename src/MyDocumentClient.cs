namespace Azure.DocumentDb.ThrottleTest
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    internal class MyDocumentClient : IDisposable
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];

        private static readonly Uri DatabaseServiceEndpoint =
            new Uri(ConfigurationManager.AppSettings["DatabaseServiceEndpoint"]);

        private static readonly string DatabaseAuthKey = ConfigurationManager.AppSettings["DatabaseAuthKey"];

        private readonly string _collectionId;

        private readonly Uri _collectionUri;

        private readonly Uri _databaseUri;

        private readonly DocumentClient _unreliable;

        public MyDocumentClient(string collectionId)
        {
            _collectionId = collectionId;
            _unreliable = new DocumentClient(DatabaseServiceEndpoint, DatabaseAuthKey,
                new ConnectionPolicy {EnableEndpointDiscovery = false, ConnectionMode = ConnectionMode.Gateway});
            SetPolicy(30, 15, 10);
            _databaseUri = UriFactory.CreateDatabaseUri(DatabaseId);
            _collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionId);
        }

        public void Dispose()
        {
            _unreliable?.Dispose();
        }

        private void SetPolicy(int requestTimeout, int maxRetryWaitTime, int maxRetryCount)
        {
            _unreliable.ConnectionPolicy.RequestTimeout = TimeSpan.FromSeconds(requestTimeout);
            _unreliable.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = maxRetryWaitTime;
            _unreliable.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = maxRetryCount;
        }

        // Ability to use a reliable client and use Exponential Backoff
        //
        //private static IReliableReadWriteDocumentClient MakeReliable(DocumentClient c)
        //{
        //    var exponentialBackoff = new ExponentialBackoff(0, TimeSpan.FromMilliseconds(100),
        //        TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

        //    return c.AsReliable(new DocumentDbRetryStrategy(exponentialBackoff));
        //}

        private async Task EnsureCollection(string collectionId)
        {
            try
            {
                await
                    _unreliable.CreateDocumentCollectionIfNotExistsAsync(
                        _databaseUri,
                        new DocumentCollection {Id = collectionId}).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Exception("exception creating collection", e);
                throw;
            }
        }

        public async Task Insert(object data)
        {
            var sw = new Stopwatch();
            sw.Start();

            // Put this block back to use a reliable client with Exponential Backoff
            //
            //using (var unreliable = new DocumentClient(_databaseServiceEndpoint, _databaseAuthKey, new ConnectionPolicy
            //{
            //    RequestTimeout = TimeSpan.FromMilliseconds(100),
            //    RetryOptions = new RetryOptions { MaxRetryWaitTimeInSeconds=1, MaxRetryAttemptsOnThrottledRequests = 1}
            //}))
            {
                try
                {
                    await EnsureCollection(_collectionId).ConfigureAwait(false);
                    var response = await _unreliable.CreateDocumentAsync(_collectionUri, data, null, false)
                        .ConfigureAwait(false);
                    LogResponse(response);
                    Logger.Log($"insert took {sw.ElapsedMilliseconds}ms");
                }
                catch (Exception e)
                {
                    Logger.Exception($"exception inserting document after {sw.ElapsedMilliseconds}ms", e);
                    throw;
                }
            }
        }

        private static void LogResponse(ResourceResponseBase r)
        {
            var msg =
                "response:" +
                $"{Environment.NewLine}\tStatusCode={r.StatusCode}" +
                $"{Environment.NewLine}\tIsRUPerMinuteUsed={r.IsRUPerMinuteUsed}" +
                $"{Environment.NewLine}\tRequestCharge={r.RequestCharge}" +
                $"{Environment.NewLine}\tCollectionUsage={r.CollectionUsage}" +
                $"{Environment.NewLine}\tMaxResourceQuota={r.MaxResourceQuota}" +
                $"{Environment.NewLine}\tCurrentResourceQuotaUsage={r.CurrentResourceQuotaUsage}" +
                $"{Environment.NewLine}\tCollectionSizeUsage={r.CollectionSizeUsage} of {r.CollectionSizeQuota}" +
                $"{Environment.NewLine}\tDatabaseUsage={r.DatabaseUsage} of {r.DatabaseQuota}" +
                $"{Environment.NewLine}\tDocumentUsage={r.DocumentUsage} of {r.DocumentQuota}" +
                $"{Environment.NewLine}\tPermissionUsage={r.PermissionUsage} of {r.PermissionQuota}" +
                $"{Environment.NewLine}\tStoredProceduresUsage={r.StoredProceduresUsage} of {r.StoredProceduresQuota}" +
                $"{Environment.NewLine}\tTriggersUsage={r.TriggersUsage} of {r.TriggersQuota}" +
                $"{Environment.NewLine}\tUserDefinedFunctionsUsage={r.UserDefinedFunctionsUsage} of {r.UserDefinedFunctionsQuota}" +
                $"{Environment.NewLine}\tUserUsage={r.UserUsage} of {r.UserQuota}";
            Logger.Log(msg);
        }
    }
}