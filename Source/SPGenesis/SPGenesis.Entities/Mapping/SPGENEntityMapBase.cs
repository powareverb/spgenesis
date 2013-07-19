using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SPGenesis.Entities.Adapters;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities
{
    public abstract class SPGENEntityMapBase<TEntity> : ISPGENEntityValueObjectMapBase
        where TEntity : class
    {
        private static SPGENEntityPropertyAccessor<TEntity> _identifierPropertyAccessor;
        private static string _identifierFieldName;

        private static SPGENEntityMapInitializationState _mapperInitState;
        private static readonly object _mapperInitLock = new object();

        private static Dictionary<string, List<SPGENEntityPropertyAccessor<TEntity>>> _propertyAccessorMaps = new Dictionary<string, List<SPGENEntityPropertyAccessor<TEntity>>>();
        private static Dictionary<string, bool> _dependentFields = new Dictionary<string, bool>();
        private static HashSet<string> _notUpdatableFields = new HashSet<string>();

        private static Dictionary<PropertyInfo, Type> _valueObjectPropertyMaps = new Dictionary<PropertyInfo, Type>();
        private Dictionary<PropertyInfo, object> _valueObjectMapInstances;

        private static readonly object _propertyToAccessorIdMapLock = new object();
        private static IDictionary<PropertyInfo, Guid> _propertyToAccessorIdMap;

        private static readonly object _requiredFieldNamesForReadLock = new object();
        private static string[] _requiredFieldNamesForRead;
        private static readonly object _requiredFieldNamesForWriteLock = new object();
        private static string[] _requiredFieldNamesForWrite;

        private Dictionary<Guid, SPGENEntityPropertyAccessorArguments> _propertyAccessorArgMap;

        public SPGENEntityMapBase()
        {
            EnsureStaticsInitialization();

            InitMapperInstance();
        }

        private void EnsureStaticsInitialization()
        {
            if (_mapperInitState == SPGENEntityMapInitializationState.Ready)
                return;

            lock (_mapperInitLock)
            {
                if (_mapperInitState == SPGENEntityMapInitializationState.Ready)
                    return;

                if (_mapperInitState == SPGENEntityMapInitializationState.Initializing)
                    throw new SPGENEntityMapInitializationException("The mapper for entity '" + typeof(TEntity).FullName + "' can not be accessed while it is being initialized.");

                try
                {
                    _mapperInitState = SPGENEntityMapInitializationState.Initializing;

                    InitializeMapper();
                }
                catch (Exception ex)
                {
                    _mapperInitState = SPGENEntityMapInitializationState.NotInitialized;
                    throw new SPGENEntityMapInitializationException("Failed to initialize the entity map '" + typeof(TEntity).FullName + "'. " + ex.Message, ex);
                }

                _mapperInitState = SPGENEntityMapInitializationState.Ready;
            }
        }

        private void InitMapperInstance()
        {
            _propertyAccessorArgMap = new Dictionary<Guid, SPGENEntityPropertyAccessorArguments>();

            foreach (var kvp in _propertyAccessorMaps)
            {
                foreach(var accessor in kvp.Value)
                {
                    if (_propertyAccessorArgMap.ContainsKey(accessor.Id))
                        continue;

                    _propertyAccessorArgMap.Add(accessor.Id, CreatePropertyAccessorArguments(accessor));
                }
            }

            if (_identifierPropertyAccessor != null)
            {
                _propertyAccessorArgMap.Add(_identifierPropertyAccessor.Id, CreatePropertyAccessorArguments(_identifierPropertyAccessor));
            }

            AddPropertyAccessorArguments(_propertyAccessorArgMap);
        }

        internal SPGENEntityPropertyAccessorArguments CreatePropertyAccessorArguments(SPGENEntityPropertyAccessor<TEntity> accessor)
        {
            var args = new SPGENEntityPropertyAccessorArguments();

            args.FieldName = accessor.MappedFieldName;
            args.AdapterInstance = accessor.GetAdapterInstance();

            if (args.AdapterInstance != null)
            {
                args.GetConverterInstance = accessor.CreateGetPropertyConvArgs();
                args.SetConverterInstance = accessor.CreateSetPropertyConvArgs();
            }

            return args;
        }

        protected void AddValueObjectMapProperty<TValueObject, TValueObjectMapper>(PropertyInfo pInfo)
            where TValueObject : class
            where TValueObjectMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            if (_valueObjectPropertyMaps.ContainsKey(pInfo))
                throw new ArgumentException("The property is already mapped.");

            var valueMapper = CreateValueObjectMapInstance<TValueObject, TValueObjectMapper>(pInfo);

            _valueObjectPropertyMaps.Add(pInfo, valueMapper.GetType());
        }


        #region Virtual methods

        /// <summary>
        /// Initializes the mapper with all mappings and defaults.
        /// </summary>
        protected abstract void InitializeMapper();

        /// <summary>
        /// Creates the entity instance. Can be used when more complex entity creation patterns needs to be done (factories).
        /// </summary>
        /// <param name="context">Operation context</param>
        /// <returns>The created entity instance</returns>
        protected internal virtual TEntity CreateEntityInstance(SPGENEntityOperationContext<TEntity> context)
        {
            return Activator.CreateInstance<TEntity>();
        }

        /// <summary>
        /// Fires when an entity is about to be populated (read operations).
        /// </summary>
        /// <param name="context">Operation context</param>
        /// <returns>Return true to continue and false to terminate the population.</returns>
        protected virtual bool OnPopulatingEntity(SPGENEntityOperationContext<TEntity> context)
        {
            return true;
        }

        /// <summary>
        /// Fires when an entity is about to populate the repository data item (update operations).
        /// </summary>
        /// <param name="context">Operation context</param>
        /// <returns>Return true to continue and false to terminate the population.</returns>
        protected virtual bool OnPopulatingRepositoryItem(SPGENEntityOperationContext<TEntity> context)
        {
            return true;
        }

        /// <summary>
        /// Fires when an entity is populated (read operations).
        /// </summary>
        /// <param name="context">Operation context</param>
        protected virtual void OnPopulatedEntity(SPGENEntityOperationContext<TEntity> context)
        {
        }

        /// <summary>
        /// Fires when an entity has populated the repository data item (update operations).
        /// </summary>
        /// <param name="context">Operation context</param>
        protected virtual void OnPopulatedRepositoryItem(SPGENEntityOperationContext<TEntity> context)
        {
        }

        /// <summary>
        /// Fires when state for an entity is requested.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The state object.</returns>
        protected virtual SPGENEntityRepositoryState OnStateReadRequest(TEntity entity)
        {
            return null;
        }

        /// <summary>
        /// Fires when state needs to be written.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="state">The state object.</param>
        protected virtual void OnStateWriteRequest(TEntity entity, SPGENEntityRepositoryState state)
        {
        }

        #endregion


        #region Internal members

        internal virtual void SetIdentifierValue(SPGENEntityOperationContext<TEntity> context)
        {
        }

        internal void SetIdentifierValueIntID(SPGENEntityOperationContext<TEntity> context, int id)
        {
            EnsureIdentifierRegistered();

            _identifierPropertyAccessor.InvokeSetPropertyItemID(context, id);
        }

        internal void SetIdentifierValueCustomId(SPGENEntityOperationContext<TEntity> context, object id)
        {
            EnsureIdentifierRegistered();

            InvokeSetPropertyAccessor(_identifierPropertyAccessor, context, id);
        }

        internal int GetIdentifierValueIntId(SPGENEntityOperationContext<TEntity> context)
        {
            EnsureIdentifierRegistered();

            return _identifierPropertyAccessor.PropertyValueGetMethodItemID(context.Entity);
        }

        internal T GetIdentifierValue<T>(SPGENEntityOperationContext<TEntity> context)
        {
            EnsureIdentifierRegistered();

            return (T)_identifierPropertyAccessor.InvokeGetProperty(context.Entity);
        }

        internal Type GetIdentifierValueType()
        {
            EnsureIdentifierRegistered();

            return _identifierPropertyAccessor.Property.PropertyType;
        }

        internal string GetIdentifierFieldName()
        {
            EnsureIdentifierRegistered();

            return _identifierPropertyAccessor.MappedFieldName;
        }

        internal bool ShouldExcludeProperty(string fieldName, SPGENEntityPropertyAccessor<TEntity> accessor, SPGENEntityOperationParameters parameters, bool forUpdate)
        {
            if (forUpdate && !accessor.SupportsUpdate)
                return true;

            if (parameters == null)
                return false;

            if (accessor.Property != null)
            {
                if (forUpdate && parameters.IsPropertyExcludedForWrite(accessor.Property))
                {
                    return true;
                }
                else if (parameters.IsPropertyExcludedForRead(accessor.Property))
                {
                    return true;
                }
            }

            return false;
        }

        internal void InvokeSetPropertyAccessor(SPGENEntityPropertyAccessor<TEntity> accessor, SPGENEntityOperationContext<TEntity> context, object value)
        {
            if (accessor.Adapter == null)
            {
                accessor.InvokeSetProperty(context.Entity, value);
                return;
            }

            var propAccArgs = _propertyAccessorArgMap[accessor.Id];
            var args = propAccArgs.SetConverterInstance as ISPGENEntityAdapterConvArgs<TEntity>;

            args.OperationContext = context;
            args.OperationParameters = context.Parameters;
            args.Entity = context.Entity;
            args.TargetProperty = accessor.Property;
            args.FieldName = context.FieldName;

            args.SetValue(value);

            accessor.InvokeSetPropertyWithAdapter(context.Entity, args, propAccArgs.AdapterInstance);
        }

        internal object InvokeGetPropertyAccessor(SPGENEntityPropertyAccessor<TEntity> accessor, SPGENEntityOperationContext<TEntity> context)
        {
            if (accessor.Adapter == null)
            {
                return accessor.InvokeGetProperty(context.Entity);
            }

            var propAccArgs = _propertyAccessorArgMap[accessor.Id];
            var args = propAccArgs.GetConverterInstance as ISPGENEntityAdapterConvArgs<TEntity>;

            args.OperationContext = context;
            args.OperationParameters = context.Parameters;
            args.Entity = context.Entity;
            args.TargetProperty = accessor.Property;
            args.FieldName = context.FieldName;

            return accessor.InvokeGetPropertyWithAdapter(context.Entity, args, propAccArgs.AdapterInstance);
        }

        internal void ResetIdentifierValue(SPGENEntityOperationContext<TEntity> context)
        {
            Type t = GetIdentifierValueType();

            if (t == typeof(int))
            {
                _identifierPropertyAccessor.InvokeSetPropertyItemID(context, 0);
            }
            else
            {
                _identifierPropertyAccessor.InvokeSetProperty(context.Entity, (t.IsValueType) ? Activator.CreateInstance(t) : null);
            }
        }

        internal string IdentifierFieldName 
        { 
            get 
            { 
                return _identifierFieldName; 
            }
            set
            {
                _identifierFieldName = value;
            }
        }

        internal SPGENEntityPropertyAccessor<TEntity> IdentifierPropertyAccessor
        {
            get
            {
                return _identifierPropertyAccessor;
            }
            set
            {
                _identifierPropertyAccessor = value;
            }
        }

        internal SPGENEntityRepositoryState GetRepositoryStateFromEntity(TEntity entity)
        {
            var ret = OnStateReadRequest(entity);
            if (ret == null && entity is SPGENEntityBase)
            {
                ret = (entity as SPGENEntityBase).RepositoryState;
            }

            return ret;
        }

        internal IDictionary<string, bool> DepententFields { get { return _dependentFields; } }

        internal HashSet<string> NotUpdatableFields { get { return _notUpdatableFields; } }

        internal void PopulateEntity(SPGENEntityOperationContext<TEntity> context)
        {
            if (!OnPopulatingEntity(context))
                return;

            if (context.CancelOperation == true)
                return;

            if (this.HasIdentifierProperty && !ShouldExcludeProperty(null, _identifierPropertyAccessor, context.Parameters, false))
            {
                SetIdentifierValue(context);
            }

            foreach (var fieldName in context.DataItem.FieldNames)
            {
                if (!_propertyAccessorMaps.ContainsKey(fieldName))
                    continue;

                context.FieldName = fieldName;
                object value = context.DataItem.FieldValues[fieldName];

                foreach (var accessor in _propertyAccessorMaps[fieldName])
                {
                    if (ShouldExcludeProperty(context.FieldName, accessor, context.Parameters, false))
                        continue;

                    //If the source is event properties, then set default value if the value is null.
                    if (value == null && context.EventProperties != null)
                    {
                        if (accessor.Property.PropertyType.IsValueType)
                        {
                            value = Activator.CreateInstance(accessor.Property.PropertyType);
                        }
                    }

                    InvokeSetPropertyAccessor(accessor, context, value);
                }
            }

            if (context.UseEntityState)
            {
                //Save state if state base is inherited.
                var state = new SPGENEntityRepositoryState(context.DataItem);
                if (context.Entity is SPGENEntityBase)
                {
                    (context.Entity as SPGENEntityBase).RepositoryState = state;
                }

                OnStateWriteRequest(context.Entity, state);
            }


            OnPopulatedEntity(context);
        }

        internal void PopulateRepositoryDataItem(SPGENEntityOperationContext<TEntity> context)
        {
            if (!OnPopulatingRepositoryItem(context))
                return;

            if (context.CancelOperation == true)
                return;

            foreach (var fieldName in context.DataItem.FieldNames)
            {
                if (!_propertyAccessorMaps.ContainsKey(fieldName))
                    continue;

                context.FieldName = fieldName;

                foreach (var accessor in _propertyAccessorMaps[fieldName])
                {
                    if (ShouldExcludeProperty(fieldName, accessor, context.Parameters, true))
                        continue;

                    context.DataItem.FieldValues[fieldName] = InvokeGetPropertyAccessor(accessor, context);
                }
            }

            if (this.HasIdentifierProperty && !ShouldExcludeProperty(null, _identifierPropertyAccessor, context.Parameters, true))
            {
                SetIdentifierValue(context);
            }

            if (context.UseEntityState)
            {
                var state = new SPGENEntityRepositoryState(context.DataItem);
                if (context.Entity is SPGENEntityBase)
                {
                    (context.Entity as SPGENEntityBase).RepositoryState = state;
                }

                OnStateWriteRequest(context.Entity, state);
            }


            OnPopulatedRepositoryItem(context);
        }

        internal SPGENEntityPropertyAccessorArguments? GetPropertyAccessorArguments(PropertyInfo pInfo)
        {
            EnsurePropertyAccessorIdMap();

            if (!_propertyToAccessorIdMap.ContainsKey(pInfo))
                return null;

            Guid id = _propertyToAccessorIdMap[pInfo];

            if (!_propertyAccessorArgMap.ContainsKey(id))
                return null;

            return _propertyAccessorArgMap[id];
        }

        internal HashSet<PropertyInfo> GetQueryableValueObjectProperties(PropertyInfo pInfo)
        {
            EnsureValueObjectMapInstances();

            var instance = _valueObjectMapInstances[pInfo] as SPGENEntityValueObjectMapBase;

            return instance.GetQueryableProperties();
        }

        protected SPGENEntityValueObjectMapBase<TValueObject> GetValueObjectMapInstance<TValueObject>(PropertyInfo pInfo) where TValueObject : class
        {
            EnsureValueObjectMapInstances();

            return _valueObjectMapInstances[pInfo] as SPGENEntityValueObjectMapBase<TValueObject>;
        }

        private void EnsureValueObjectMapInstances()
        {
            if (_valueObjectMapInstances == null)
            {
                _valueObjectMapInstances = new Dictionary<PropertyInfo, object>();

                foreach (var kvp in _valueObjectPropertyMaps)
                {
                    Type t = kvp.Value;
                    var instance = Activator.CreateInstance(t);
                    _valueObjectMapInstances.Add(kvp.Key, instance);
                }
            }
        }

        private void EnsurePropertyAccessorIdMap()
        {
            if (_propertyToAccessorIdMap != null)
                return;

            lock (_propertyToAccessorIdMapLock)
            {
                if (_propertyToAccessorIdMap != null)
                    return;

                var result = new Dictionary<PropertyInfo, Guid>();

                if (this.HasIdentifierProperty)
                {
                    result.Add(_identifierPropertyAccessor.Property, _identifierPropertyAccessor.Id);
                }

                foreach (var kvp in _propertyAccessorMaps)
                {
                    foreach (var accessor in kvp.Value)
                    {
                        if (result.ContainsKey(accessor.Property))
                            continue;

                        result.Add(accessor.Property, accessor.Id);
                    }
                }

                _propertyToAccessorIdMap = result;
            }
        }

        internal bool HasIdentifierProperty
        {
            get
            {
                if (_identifierPropertyAccessor == null)
                    return false;

                return true;
            }
        }

        internal SPGENEntityPropertyAccessor<TEntity> FindPropertyAccessor(PropertyInfo property)
        {
            foreach (var kvp in _propertyAccessorMaps)
            {
                int i = kvp.Value.FindIndex(p => p.Property == property);
                if (i != -1)
                    return kvp.Value[i];
            }

            return null;
        }

        protected void AddPropertyAccessor<TPropertyValue>(PropertyInfo property, string fieldInternalName)
        {
            AddToPropertyAccessorMap<TPropertyValue>(property, fieldInternalName);
        }

        protected void AddPropertyAccessor<TPropertyValue, TAdapter>(PropertyInfo property, string fieldInternalName, Func<TAdapter> adapter)
            where TAdapter : SPGENEntityAdapter<TEntity, TPropertyValue>
        {
            AddToPropertyAccessorMap<TPropertyValue, TAdapter>(property, fieldInternalName, adapter);
        }

        internal SPGENEntityPropertyAccessor<TEntity> AddToPropertyAccessorMap<TPropertyValue>(PropertyInfo property, string fieldInternalName)
        {
            return AddToPropertyAccessorMap<TPropertyValue, SPGENEntityAdapter<TEntity, TPropertyValue>>(property, fieldInternalName, null);
        }

        internal SPGENEntityPropertyAccessor<TEntity> AddToPropertyAccessorMap<TPropertyValue, TAdapter>(PropertyInfo property, string fieldInternalName, Func<TAdapter> adapter)
            where TAdapter : SPGENEntityAdapter<TEntity, TPropertyValue>
        {
            List<SPGENEntityPropertyAccessor<TEntity>> list;
            if (_propertyAccessorMaps.ContainsKey(fieldInternalName))
            {
                list = _propertyAccessorMaps[fieldInternalName];
            }
            else
            {
                list = new List<SPGENEntityPropertyAccessor<TEntity>>();

                _propertyAccessorMaps.Add(fieldInternalName, list);
            }

            if (adapter == null)
                adapter = new Func<TAdapter>(() => new Adapters.SPGENEntityAdapterDefault<TEntity, TPropertyValue>() as TAdapter);

            var accessor = SPGENEntityPropertyAccessor<TEntity>.CreateAccessor<TPropertyValue, TAdapter>(property, adapter, false);

            if (fieldInternalName != null)
            {
                accessor.MappedFieldName = fieldInternalName;
            }

            list.Add(accessor);

            return accessor;
        }

        internal virtual void AddPropertyAccessorArguments(IDictionary<Guid, SPGENEntityPropertyAccessorArguments> instances)
        {
        }

        #endregion


        #region Public members

        public virtual string[] GetRequiredFieldsForRead()
        {
            if (_requiredFieldNamesForRead != null)
                return _requiredFieldNamesForRead;

            lock (_requiredFieldNamesForReadLock)
            {
                if (_requiredFieldNamesForRead != null)
                    return _requiredFieldNamesForRead;

                var list = new List<string>();

                foreach (var kvp in _propertyAccessorMaps)
                {
                    if (!list.Contains(kvp.Key))
                        list.Add(kvp.Key);
                }

                foreach (var kvp in _dependentFields)
                {
                    if (!list.Contains(kvp.Key))
                        list.Add(kvp.Key);
                }


                _requiredFieldNamesForRead = list.ToArray();
            }

            return _requiredFieldNamesForRead;
        }

        public virtual string[] GetRequiredFieldsForWrite()
        {
            if (_requiredFieldNamesForWrite != null)
                return _requiredFieldNamesForWrite;

            lock (_requiredFieldNamesForWriteLock)
            {
                if (_requiredFieldNamesForWrite != null)
                    return _requiredFieldNamesForWrite;

                var list = new List<string>();

                foreach (var kvp in _propertyAccessorMaps)
                {
                    if (_notUpdatableFields.Contains(kvp.Key))
                        continue;

                    if ((from p in kvp.Value where !p.SupportsUpdate select p).Count() == kvp.Value.Count())
                        continue;

                    if (!list.Contains(kvp.Key))
                        list.Add(kvp.Key);
                }

                foreach (var kvp in _dependentFields)
                {
                    if (kvp.Value && !list.Contains(kvp.Key))
                        list.Add(kvp.Key);
                }


                _requiredFieldNamesForWrite = list.ToArray();
            }

            return _requiredFieldNamesForWrite;
        }

        #endregion


        #region Private members

        private static SPGENEntityValueObjectMapBase<TValueObject> CreateValueObjectMapInstance<TValueObject, TMapper>(PropertyInfo pInfo)
            where TValueObject : class
            where TMapper : SPGENEntityValueObjectMapBase<TValueObject>
        {
            if (typeof(TMapper) == typeof(SPGENEntityValueObjectMapBase<TValueObject>))
            {
                Type t = SPGENEntityMapResolver.FindMapper(typeof(TValueObject));
                var ret = Activator.CreateInstance(t) as SPGENEntityValueObjectMapBase<TValueObject>;
                if (ret == null)
                    throw new SPGENEntityMapNotFoundException(typeof(TValueObject));

                return ret;
            }
            else
            {
                return Activator.CreateInstance<TMapper>();
            }
        }

        private static void EnsureIdentifierRegistered()
        {
            if (_identifierPropertyAccessor == null)
                throw new SPGENEntityGeneralException(string.Format("No identifier has been registered for the entity '{0}'.", typeof(TEntity).FullName));
        }

        private static Func<SPGENEntityAdapter<TEntity, TPropertyValue>> GetDefaultAdapter<TPropertyValue>()
        {
            return new Func<SPGENEntityAdapter<TEntity, TPropertyValue>>(() => new SPGENEntityAdapterDefault<TEntity, TPropertyValue>());
        }

        #endregion

    }

    interface ISPGENEntityMapBase
    {
    }
}
