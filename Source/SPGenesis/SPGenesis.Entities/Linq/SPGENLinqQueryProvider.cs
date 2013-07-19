using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Linq.Expressions;
using SPGenesis.Entities.Repository;
using System.Diagnostics;
using System.Xml;

namespace SPGenesis.Entities.Linq
{
    internal class SPGENLinqQueryProvider<TEntity> : IQueryProvider
        where TEntity : class
    {
        private SPGENLinqQueryableList<TEntity> _parentQueryableList;
        private SPGENEntityOperationContext<TEntity> _context;
        private string[] _fieldNames;

        internal bool EnableTimers { get; set; }
        internal SPGENLinqTimingValues ElapsedExecutionTime { get; private set; }
        internal SPGENLinqTimingValues ElapsedExpressionEvaluationTime { get; private set; }
        internal SPGENLinqTimingValues ElapsedDBQueryTime { get; private set; }

        internal SPGENLinqExpressionTreeVisitor<TEntity> ExpressionTreeVisitor { get; private set; }
        internal SPListItemCollection ListItemCollection { get; private set; }

        internal SPGENLinqQueryProvider(SPGENLinqQueryableList<TEntity> parent, SPGENEntityOperationContext<TEntity> context)
        {
            if (context.List == null)
                throw new SPGENEntityGeneralException("There is no list instance in the operation context.");

            _context = context;
            _parentQueryableList = parent;
            _fieldNames = _context.EntityMap.GetRequiredFieldsForRead();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            _parentQueryableList.Expression = expression;

            return (IQueryable)_parentQueryableList;
        }

        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            _parentQueryableList.Expression = expression;

            return (IQueryable<TResult>)_parentQueryableList;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            Stopwatch sw = null;
            if (this.EnableTimers)
                sw = new Stopwatch();

            var result = ExecuteWithTimedScope(() => ExecuteQuery(expression), sw);

            if (this.EnableTimers)
                this.ElapsedExecutionTime = new SPGENLinqTimingValues(sw.ElapsedMilliseconds, sw.ElapsedTicks);

            return result;
        }

        private object ExecuteQuery(Expression expression)
        {
            Stopwatch sw1 = null;
            Stopwatch sw2 = null;
            if (this.EnableTimers)
            {
                sw1 = new Stopwatch();
                sw2 = new Stopwatch();
            }

            SPQuery query = null;

            if (_context.Parameters != null)
            {
                if (_context.Parameters.SPQueryTemplate != null)
                    query = _context.Parameters.SPQueryTemplate;
            }

            if (query == null)
                query = new SPQuery();


            XmlNode completeCamlNode = null;

            var visitedExpressionResult = ExecuteWithTimedScope<SPGENLinqExpressionTreeVisitor<TEntity>>(() =>
                {
                    var expResult = SPGENLinqExpressionTreeVisitor<TEntity>.Execute(expression, typeof(TEntity), _context);

                    completeCamlNode = expResult.CAMLQuery;

                    return expResult;
                }, 
                sw1);

            query.Query = completeCamlNode.InnerXml;

            var result = _context.ManagerInstance.ExecuteListItemsFetchOperation(_context, query.Query, null);

            this.ListItemCollection = result.ListItemCollection;
            this.ExpressionTreeVisitor = visitedExpressionResult;

            if (this.EnableTimers)
            {
                this.ElapsedDBQueryTime = new SPGENLinqTimingValues(sw2.ElapsedMilliseconds, sw2.ElapsedTicks);
                this.ElapsedExpressionEvaluationTime = new SPGENLinqTimingValues(sw1.ElapsedMilliseconds, sw1.ElapsedTicks);
            }

            return result;
        }

        private TResult ExecuteWithTimedScope<TResult>(Func<TResult> method, Stopwatch sw)
        {
            TResult result;

            if (sw != null)
            {
                sw.Reset();
                sw.Start();
            }

            result = method();

            if (sw != null)
                sw.Stop();

            return result;
        }
    }

    public struct SPGENLinqTimingValues
    {
        public long Milliseconds;
        public long Ticks;
        public long Microseconds { get { return Convert.ToInt64(Convert.ToDouble(this.Ticks) / 10); } }

        public SPGENLinqTimingValues(long ms, long ticks)
        {
            this.Milliseconds = ms;
            this.Ticks = ticks;
        }
    }
}
