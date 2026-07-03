using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;

namespace RMA.Server.Services
{
    public static class FirestoreMetrics
    {
        private static long _totalReads = 0;
        private static long _totalWrites = 0;

        public static long TotalReads => _totalReads;
        public static long TotalWrites => _totalWrites;

        public static void IncrementReads(long count)
        {
            System.Threading.Interlocked.Add(ref _totalReads, count);
        }

        public static void IncrementWrites(long count)
        {
            System.Threading.Interlocked.Add(ref _totalWrites, count);
        }

        public static void Reset()
        {
            System.Threading.Interlocked.Exchange(ref _totalReads, 0);
            System.Threading.Interlocked.Exchange(ref _totalWrites, 0);
        }
    }

    public class FirestoreRepository<T> where T : class
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _collectionName;
        private readonly IMemoryCache? _cache;

        public FirestoreRepository(FirestoreDb firestoreDb, string collectionName, IMemoryCache? cache = null)
        {
            _firestoreDb = firestoreDb;
            _collectionName = collectionName;
            _cache = cache;
        }

        private int GetCollectionVersion()
        {
            if (_cache == null) return 1;
            string key = $"col_{_collectionName}_version";
            if (!_cache.TryGetValue(key, out int version))
            {
                version = 1;
                _cache.Set(key, version, TimeSpan.FromHours(24));
            }
            return version;
        }

        private void InvalidateCache()
        {
            if (_cache == null) return;
            string key = $"col_{_collectionName}_version";
            if (_cache.TryGetValue(key, out int version))
            {
                _cache.Set(key, version + 1, TimeSpan.FromHours(24));
            }
            else
            {
                _cache.Set(key, 2, TimeSpan.FromHours(24));
            }
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_all";
                if (_cache.TryGetValue(cacheKey, out List<T>? cachedList) && cachedList != null)
                {
                    return cachedList;
                }
            }

            var collection = _firestoreDb.Collection(_collectionName);
            var snapshot = await collection.GetSnapshotAsync();
            FirestoreMetrics.IncrementReads(Math.Max(1, snapshot.Documents.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }

            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_all";
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            }

            return result;
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_id_{id}";
                if (_cache.TryGetValue(cacheKey, out T? cachedEntity))
                {
                    return cachedEntity;
                }
            }

            var document = _firestoreDb.Collection(_collectionName).Document(id);
            var snapshot = await document.GetSnapshotAsync();
            FirestoreMetrics.IncrementReads(1);

            T? result = null;
            if (snapshot.Exists)
            {
                result = snapshot.ConvertTo<T>();
            }

            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_id_{id}";
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            }

            return result;
        }

        public virtual async Task<List<T>> GetByFieldAsync(string fieldName, object value)
        {
            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_field_{fieldName}_{value}";
                if (_cache.TryGetValue(cacheKey, out List<T>? cachedList) && cachedList != null)
                {
                    return cachedList;
                }
            }

            var collection = _firestoreDb.Collection(_collectionName);
            var query = collection.WhereEqualTo(fieldName, value);
            var snapshot = await query.GetSnapshotAsync();
            FirestoreMetrics.IncrementReads(Math.Max(1, snapshot.Documents.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }

            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_field_{fieldName}_{value}";
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            }

            return result;
        }

        public virtual async Task<List<T>> GetPagedAsync(int limit, int offset)
        {
            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_paged_{limit}_{offset}";
                if (_cache.TryGetValue(cacheKey, out List<T>? cachedList) && cachedList != null)
                {
                    return cachedList;
                }
            }

            var collection = _firestoreDb.Collection(_collectionName);
            var query = collection.Offset(offset).Limit(limit);
            var snapshot = await query.GetSnapshotAsync();
            FirestoreMetrics.IncrementReads(Math.Max(1, snapshot.Documents.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }

            if (_cache != null)
            {
                int version = GetCollectionVersion();
                string cacheKey = $"col_{_collectionName}_v{version}_paged_{limit}_{offset}";
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            }

            return result;
        }

        public virtual async Task<string> AddAsync(T entity)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var docRef = await collection.AddAsync(entity);
            FirestoreMetrics.IncrementWrites(1);
            InvalidateCache();
            return docRef.Id;
        }

        public virtual async Task UpdateAsync(string id, T entity)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.SetAsync(entity, SetOptions.Overwrite);
            FirestoreMetrics.IncrementWrites(1);
            InvalidateCache();
        }

        public virtual async Task DeleteAsync(string id)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.DeleteAsync();
            FirestoreMetrics.IncrementWrites(1);
            InvalidateCache();
        }
    }
}
