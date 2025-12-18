using EntityFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Common
{
    public  class EFUtilitiesHelp<TContext, TEntity, TItems>
       where TContext : DbContext
       where TEntity : class
       where TItems : IEnumerable<TEntity>
    {

        public virtual void InsertAll(TContext context, IDbSet<TEntity> set, TItems items)
        {
            EFBatchOperation.For(context, set).InsertAll(items);
        }

        public virtual void UpdateAll(TContext context, IDbSet<TEntity> set, TItems items, Action<UpdateSpecification<TEntity>> updateSpecification)
        {
            EFBatchOperation.For(context, set).UpdateAll(items, updateSpecification);
        }
    }
}