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

        public async Task<List<T>> GetAllAsync()
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var snapshot = await collection.GetSnapshotAsync();
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

        public async Task<T?> GetByIdAsync(string id)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            var snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<T>();
            }
            return null;
        }

        public async Task<List<T>> GetByFieldAsync(string fieldName, object value)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var query = collection.WhereEqualTo(fieldName, value);
            var snapshot = await query.GetSnapshotAsync();
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

        public async Task<string> AddAsync(T entity)
        {
            var collection = _firestoreDb.Collection(_collectionName);
            var docRef = await collection.AddAsync(entity);
            return docRef.Id;
        }

        public async Task UpdateAsync(string id, T entity)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.SetAsync(entity, SetOptions.Overwrite);
        }

        public async Task DeleteAsync(string id)
        {
            var document = _firestoreDb.Collection(_collectionName).Document(id);
            await document.DeleteAsync();
        }
    }
}
