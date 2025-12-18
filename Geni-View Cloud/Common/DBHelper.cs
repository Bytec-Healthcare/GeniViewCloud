using EntityFramework.Utilities;
using GeniView.Cloud.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Common
{
    public class DBHelper : IDisposable
    {
        private GeniViewCloudDataRepository _db;

        public GeniViewCloudDataRepository DB
        {
            get
            {
                return _db;
            }
            set
            {
                _db = value;
            }
        }

        public DBHelper()
        {
            GeniViewCloudDataRepository _db = new GeniViewCloudDataRepository();
        }


        public virtual void BatchInsert<TContext, T>(TContext db, DbSet<T> dbSet, List<T> dataList)
            where TContext : DbContext
            where T : class
        {
            if (dataList.Any() == true)
            {
                EFBatchOperation.For<TContext, T>(db, dbSet).InsertAll<T>(dataList);
            }
        }

        public virtual void UpdateAll<TContext, T>(TContext db, DbSet<T> dbSet, List<T> dataList, Action<UpdateSpecification<T>> updateSpecification)
            where TContext : DbContext
            where T : class
        {
            if (dataList.Any() == true)
            {
                EFBatchOperation.For<TContext, T>(db, dbSet).UpdateAll<T>(dataList, updateSpecification);
            }
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.Collect();
        }
    }
}