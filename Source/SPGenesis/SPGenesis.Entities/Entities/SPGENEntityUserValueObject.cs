using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Reflection;

namespace SPGenesis.Entities
{
    public class SPGENEntityUserValueObject
    {
        public virtual int Id { get; set; }
        public virtual string DisplayName { get; set; }

        [Obsolete("Not supported. Use the GetSPUser method to get the SPUser object containing these properties.", true)]
        public virtual string Email { get; internal set; }
        [Obsolete("Not supported. Use the GetSPUser method to get the SPUser object containing these properties.", true)]
        public virtual string Login { get; set; }

        public SPGENEntityUserValueObject()
        {
        }

        public SPGENEntityUserValueObject(SPUser user)
        {
            this.Id = user.ID;
            this.DisplayName = user.Name;
        }

        public SPUser GetSPUser(SPUserCollection users)
        {
            return users.GetByID(this.Id);
        }
    }

    public class SPGENEntityUserValueMap : SPGENEntityUserValueMap<SPGENEntityUserValueObject>
    {
        protected override void GetValues(SPGENEntityUserValueObject lookup, out int lookupId, out string lookupValue)
        {
            lookupId = lookup.Id;
            lookupValue = lookup.DisplayName;
        }

        protected override void SetValues(SPGENEntityUserValueObject lookup, int lookupId, string lookupValue)
        {
            lookup.Id = lookupId;
            lookup.DisplayName = lookupValue;
        }

        protected override System.Linq.Expressions.Expression<Func<SPGENEntityUserValueObject, object>> IdentifierProperty
        {
            get { return e => e.Id; }
        }

        protected override System.Linq.Expressions.Expression<Func<SPGENEntityUserValueObject, object>> ValueProperty
        {
            get { return e => e.DisplayName; }
        }

        public override Linq.SPGENEntityEvalLinqExprResult EvalComparison(Linq.SPGENEntityEvalLinqExprArgs args)
        {
            string propName = args.SourceProperty.Name;
            if (propName == "Login" || propName == "Email")
                throw new NotSupportedException();

            return base.EvalComparison(args);
        }

        public override Linq.SPGENEntityEvalLinqExprResult EvalMethodCall(System.Linq.Expressions.MethodCallExpression mce, Linq.SPGENEntityEvalLinqExprArgs args)
        {
            string propName = args.SourceProperty.Name;
            if (propName == "Login" || propName == "Email")
                throw new NotSupportedException();

            return base.EvalMethodCall(mce, args);
        }
    }

    public abstract class SPGENEntityUserValueMap<TValueObject> : SPGENEntityUserValueMap<TValueObject, SPFieldUserValue>
        where TValueObject : class
    {
    }

    public abstract class SPGENEntityUserValueMap<TValueObject, TFieldValue> : SPGENEntityLookupValueMap<TValueObject, TFieldValue>
        where TValueObject : class
        where TFieldValue : SPFieldUserValue
    {
    }
}
