using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace RMA.Server.Services
{
    public class FirestoreRepository<T> where T : class
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _collectionName;

        public FirestoreRepository(FirestoreDb firestoreDb, string collectionName)
        {
            _firestoreDb = firestoreDb;
            _collectionName = collectionName;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var snapshot = await collection.GetSnapshotAsync();
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Read", Math.Max(1, snapshot.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }
            return result;
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            var snapshot = await document.GetSnapshotAsync();
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Read", 1);

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<T>();
            }
            return null;
        }

        public virtual async Task<List<T>> GetByFieldAsync(string fieldName, object value)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var query = collection.WhereEqualTo(fieldName, value);
            var snapshot = await query.GetSnapshotAsync();
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Read", Math.Max(1, snapshot.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }
            return result;
        }

        public virtual async Task<List<T>> GetPagedAsync(int limit, int offset)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var query = collection.Offset(offset).Limit(limit);
            var snapshot = await query.GetSnapshotAsync();
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Read", Math.Max(1, snapshot.Count));

            var result = new List<T>();
            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    result.Add(document.ConvertTo<T>());
                }
            }
            return result;
        }

        public virtual async Task<string> AddAsync(T entity)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var docRef = await collection.AddAsync(entity);
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Write", 1);

            return docRef.Id;
        }

        public virtual async Task UpdateAsync(string id, T entity)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.SetAsync(entity, SetOptions.Overwrite);
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Write", 1);
        }

        public virtual async Task DeleteAsync(string id)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.DeleteAsync();
            
            MetricsCollectorService.Instance?.IncrementFirestoreOp(_collectionName, "Delete", 1);
        }
    }
}
