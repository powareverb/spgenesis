using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SPGenesis.Entities;
using System.Collections;

namespace SPGenesis.Entities.Linq
{
    public class SPGENLinqQueryableList<TEntity> : IQueryable<TEntity>, IOrderedQueryable<TEntity>
        where TEntity : class
    {
        private SPGENLinqExpressionTreeVisitor<TEntity> LastExecutedTreeVisitor;
        private SPListItemCollection LastQueryListItemCollection;

        private SPGENLinqQueryProvider<TEntity> _provider;
        private SPGENEntityManagerFoundationBase<TEntity> _managerInstance;
        private SPGENEntityOperationContext<TEntity> _context;

        public IQueryProvider Provider { get { return _provider; } }
        public Expression Expression { get; internal set; }
        public SPQuery QueryTemplate { get { return _context.Parameters != null ? _context.Parameters.SPQueryTemplate : null; } }

        [Obsolete("Not longer supported. Use QueryTemplate instead.", true)]
        public SPQuery QueryOptions { get; private set; }

        [Obsolete("Not longer supported. Set parameters when creating the queryable list instead.", true)]
        public SPGENEntityOperationParameters Parameters { get; set; }

        internal SPGENLinqQueryableList(SPGENEntityOperationContext<TEntity> context)
        {
            this.Expression = Expression.Constant(this);

            _context = context;
            _provider = new SPGENLinqQueryProvider<TEntity>(this, context);
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            var provider = _provider;

            this.LastExecutedTreeVisitor = null;
            this.LastQueryListItemCollection = null;
            
            var result = Provider.Execute<IEnumerator<TEntity>>(this.Expression);

            this.LastExecutedTreeVisitor = provider.ExpressionTreeVisitor;
            this.LastQueryListItemCollection = provider.ListItemCollection;

            return result;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(TEntity); }
        }

        public System.Xml.XmlNode TranslateQueryToCAML()
        {
            var visitedExpressionResult = SPGENLinqExpressionTreeVisitor<TEntity>.Execute(this.Expression, typeof(TEntity), _context);

            return visitedExpressionResult.CAMLQuery;
        }

        public string TranslateQueryToCAMLAsString(bool formatted)
        {
            var visitedExpressionResult = SPGENLinqExpressionTreeVisitor<TEntity>.Execute(this.Expression, typeof(TEntity), _context);

            return visitedExpressionResult.GetCAMLAsString(true);
        }

        public System.Xml.XmlNode TranslateExpressionToCAML(Expression<Func<TEntity, bool>> expression)
        {
            var visitedExpressionResult = SPGENLinqExpressionTreeVisitor<TEntity>.Execute(expression, typeof(TEntity), _context);

            return visitedExpressionResult.CAMLQuery;
        }

        public string TranslateExpressionToCAMLAsString(Expression<Func<TEntity, bool>> expression, bool formatted)
        {
            var visitedExpressionResult = SPGENLinqExpressionTreeVisitor<TEntity>.Execute(expression, typeof(TEntity), _context);

            return visitedExpressionResult.GetCAMLAsString(formatted);
        }

        public string GetLastQueryCAMLAsString(bool formatted)
        {
            return this.LastExecutedTreeVisitor != null ? this.LastExecutedTreeVisitor.GetCAMLAsString(formatted) : string.Empty;
        }

        public System.Xml.XmlNode GetLastQueryCAML()
        {
            return this.LastExecutedTreeVisitor != null ? this.LastExecutedTreeVisitor.CAMLQuery : null;
        }

        public SPListItemCollection GetLastQueryListItemCollection()
        {
            return this.LastQueryListItemCollection != null ? this.LastQueryListItemCollection : null;
        }

        public bool EnableTimers
        {
            get { return _provider.EnableTimers; }
            set { _provider.EnableTimers = true; }
        }

        public SPGENLinqTimingValues ElapsedExecutionTime
        {
            get { return _provider.ElapsedExecutionTime; }
        }

        public SPGENLinqTimingValues ElapsedExpressionEvaluationTime
        {
            get { return _provider.ElapsedExpressionEvaluationTime; }
        }

        public SPGENLinqTimingValues ElapsedDBQueryTime
        {
            get { return _provider.ElapsedDBQueryTime; }
        }

    }
}
