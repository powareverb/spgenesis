using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.SharePoint;
using SPGenesis.Core;
using SPGenesis.Entities.Adapters;
using SPGenesis.Entities.Repository;
using SPGenesis.Entities.Linq.Adapters;

namespace SPGenesis.Entities
{
    internal class SPGENEntityPropertyAccessor<TEntity> where TEntity : class
    {
        public delegate int PropertyValueGetDelegateID(TEntity target);
        public delegate void PropertyValueSetDelegateID(TEntity target, int value);

        public delegate object PropertyValueGetDelegate(TEntity target);
        public delegate void PropertyValueSetDelegate(TEntity target, object value);

        public delegate object PropertyValueGetWithConverterDelegate(TEntity target, object arguments, object adapter);
        public delegate void PropertyValueSetWithConverterDelegate(TEntity target, object arguments, object adapter);

        public bool CanRead = true;
        public bool CanWrite = true;

        public PropertyValueGetDelegate PropertyValueGetMethod { get; set; }
        public PropertyValueSetDelegate PropertyValueSetMethod { get; set; }
        public PropertyValueGetWithConverterDelegate PropertyValueGetWithConverterMethod { get; set; }
        public PropertyValueSetWithConverterDelegate PropertyValueSetWithConverterMethod { get; set; }
        public PropertyValueGetDelegateID PropertyValueGetMethodItemID { get; set; }
        public PropertyValueSetDelegateID PropertyValueSetMethodItemID { get; set; }
        public Delegate Adapter { get; private set; }
        public Func<object> CreateSetPropertyConvArgs { get; set; }
        public Func<object> CreateGetPropertyConvArgs { get; set; }
        public PropertyInfo Property { get; set; }
        public string MappedFieldName { get; set; }
        public bool SupportsUpdate { get; set; }
        public Guid Id { get; private set; }

        public static SPGENEntityPropertyAccessor<TEntity> CreateAccessor<TPropertyValue>(PropertyInfo property, bool isItemId)
        {
            return new SPGENEntityPropertyAccessor<TEntity>(property, null, isItemId);
        }

        public static SPGENEntityPropertyAccessor<TEntity> CreateAccessor<TPropertyValue, TAdapter>(PropertyInfo property, Func<TAdapter> adapter, bool isItemId)
            where TAdapter : Adapters.SPGENEntityAdapter<TEntity, TPropertyValue>
        {
            return new SPGENEntityPropertyAccessor<TEntity>(property, adapter, isItemId);
        }

        private SPGENEntityPropertyAccessor(PropertyInfo property, Delegate adapter, bool isItemId)
        {
            this.Id = Guid.NewGuid();
            this.Adapter = adapter;
            this.SupportsUpdate = true;
            this.Property = property;

            if (property.GetGetMethod(true) == null)
                CanRead = false;

            if (property.GetSetMethod(true) == null)
                CanWrite = false;

            if (!isItemId)
            {
                if (CanRead)
                    CreateGetPropertyMethod(property);

                if (CanWrite)
                    CreateSetPropertyMethod(property);
            }
            else
            {
                if (CanRead)
                    CreateGetPropertyMethodForItemID(property);

                if (CanWrite)
                    CreateSetPropertyMethodForItemID(property);
            }
        }

        public object GetAdapterInstance()
        {
            if (this.Adapter == null)
                return null;

            return this.Adapter.DynamicInvoke();
        }

        public int InvokeGetPropertyItemID(SPGENEntityOperationContext<TEntity> context)
        {
            if (!CanRead)
                throw new SPGENEntityGeneralException("Unable to get id property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'.");

            try
            {
                return this.PropertyValueGetMethodItemID(context.Entity);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not get id property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        public void InvokeSetPropertyItemID(SPGENEntityOperationContext<TEntity> context, int id)
        {
            if (!CanWrite)
                throw new SPGENEntityGeneralException("Unable to set id property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'.");

            try
            {
                this.PropertyValueSetMethodItemID(context.Entity, id);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not set id property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        public object InvokeGetProperty(TEntity instance)
        {
            if (!CanRead)
                throw new SPGENEntityGeneralException("Unable to get property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'.");

            try
            {
                return this.PropertyValueGetMethod(instance);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not get property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        public object InvokeGetPropertyWithAdapter(TEntity entity, Adapters.ISPGENEntityAdapterConvArgs<TEntity> arguments, object adapter)
        {
            if (!CanRead)
                throw new SPGENEntityGeneralException("Unable to get property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'.");

            try
            {
                return this.PropertyValueGetWithConverterMethod(entity, arguments, adapter);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not get property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        public void InvokeSetProperty(TEntity instance, object value)
        {
            if (!CanWrite)
                throw new SPGENEntityGeneralException("Unable to set property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'.");

            try
            {
                this.PropertyValueSetMethod(instance, value);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not set property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        public void InvokeSetPropertyWithAdapter(TEntity entity, Adapters.ISPGENEntityAdapterConvArgs<TEntity> arguments, object adapter)
        {
            if (!CanWrite)
                throw new SPGENEntityGeneralException("Unable to set property '" + this.Property.Name + "' from entity '" + this.Property.DeclaringType + "'.");

            try
            {
                this.PropertyValueSetWithConverterMethod(entity, arguments, adapter);
            }
            catch (Exception ex)
            {
                throw new SPGENEntityGeneralException("Could not set property '" + this.Property.Name + "' on entity '" + this.Property.DeclaringType + "'. Message: " + ex.Message, ex);
            }
        }

        private void CreateGetPropertyMethodForItemID(PropertyInfo property)
        {
            string dmName = "GetProp__" + property.DeclaringType.FullName.Replace(".", "_") + "__" + property.Name;

            var dynamicMethod = new DynamicMethod(dmName, typeof(int), new[] { typeof(TEntity) }, true);
            var generator = dynamicMethod.GetILGenerator(128);

            generator.DeclareLocal(typeof(int));

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, property.GetGetMethod(true));
            generator.Emit(OpCodes.Stloc_0);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);

            this.PropertyValueGetMethodItemID = (PropertyValueGetDelegateID)dynamicMethod.CreateDelegate(typeof(PropertyValueGetDelegateID));
        }

        private void CreateSetPropertyMethodForItemID(PropertyInfo property)
        {
            string dmName = "SetProp__" + property.DeclaringType.FullName.Replace(".", "_") + "__" + property.Name;

            var dynamicMethod = new DynamicMethod(dmName, null, new[] { typeof(TEntity), typeof(int) }, true);
            var generator = dynamicMethod.GetILGenerator(128);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, property.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);

            this.PropertyValueSetMethodItemID = (PropertyValueSetDelegateID)dynamicMethod.CreateDelegate(typeof(PropertyValueSetDelegateID));
        }

        private void CreateGetPropertyMethod(PropertyInfo property)
        {
            string dmName = "GetProp__" + property.DeclaringType.FullName.Replace(".", "_") + "__" + property.Name;

            if (this.Adapter != null)
            {
                var dynamicMethod = new DynamicMethod(dmName, typeof(object), new[] { typeof(TEntity), typeof(object), typeof(object) }, true);
                var generator = dynamicMethod.GetILGenerator(128);
                var propertyGetMethod = property.GetGetMethod(true);

                var adapter = this.GetAdapterInstance();
                var convertMethod = adapter.GetType().GetMethod("InvokeConvertToListItemValue", BindingFlags.Instance | BindingFlags.NonPublic);
                var paramInfo = convertMethod.GetParameters()[0];
                var propertyArgumentsSetValueMethod = paramInfo.ParameterType.GetProperty("Value").GetSetMethod(true);
                var valueType = paramInfo.ParameterType.GetProperty("Value").PropertyType;

                generator.DeclareLocal(typeof(object));

                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Castclass, paramInfo.ParameterType);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, propertyGetMethod);

                if (property.PropertyType != valueType)
                {
                    generator.Emit(OpCodes.Castclass, valueType);
                }

                generator.Emit(OpCodes.Callvirt, propertyArgumentsSetValueMethod);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Castclass, adapter.GetType());
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Castclass, paramInfo.ParameterType);
                generator.Emit(OpCodes.Callvirt, convertMethod);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);

                this.CreateGetPropertyConvArgs = new Func<object>(() => Activator.CreateInstance(paramInfo.ParameterType));
                this.PropertyValueGetWithConverterMethod = (PropertyValueGetWithConverterDelegate)dynamicMethod.CreateDelegate(typeof(PropertyValueGetWithConverterDelegate));
            }
            else
            {
                var dynamicMethod = new DynamicMethod(dmName, typeof(object), new[] { typeof(TEntity) }, true);
                var generator = dynamicMethod.GetILGenerator(128);

                generator.DeclareLocal(typeof(object));

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, property.GetGetMethod(true));

                if (property.PropertyType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, property.PropertyType);
                }

                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);

                this.PropertyValueGetMethod = (PropertyValueGetDelegate)dynamicMethod.CreateDelegate(typeof(PropertyValueGetDelegate));
            }
        }

        private void CreateSetPropertyMethod(PropertyInfo property)
        {
            string dmName = "SetProp__" + property.DeclaringType.FullName.Replace(".", "_") + "__" + property.Name;

            if (this.Adapter != null)
            {
                var dynamicMethod = new DynamicMethod(dmName, null, new[] { typeof(TEntity), typeof(object), typeof(object) }, true);
                var generator = dynamicMethod.GetILGenerator(128);

                var adapter = this.GetAdapterInstance();
                var convertMethod = adapter.GetType().GetMethod("InvokeConvertToPropertyValue", BindingFlags.Instance | BindingFlags.NonPublic);
                var paramInfo = convertMethod.GetParameters()[0];

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Castclass, adapter.GetType());
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Castclass, paramInfo.ParameterType);
                generator.Emit(OpCodes.Callvirt, convertMethod);

                if (property.PropertyType != convertMethod.ReturnType)
                {
                    generator.Emit(OpCodes.Castclass, property.PropertyType);
                }

                generator.Emit(OpCodes.Callvirt, property.GetSetMethod(true));
                generator.Emit(OpCodes.Ret);

                this.CreateSetPropertyConvArgs = new Func<object>(() => Activator.CreateInstance(paramInfo.ParameterType));
                this.PropertyValueSetWithConverterMethod = (PropertyValueSetWithConverterDelegate)dynamicMethod.CreateDelegate(typeof(PropertyValueSetWithConverterDelegate));
            }
            else
            {
                var dynamicMethod = new DynamicMethod(dmName, null, new[] { typeof(TEntity), typeof(object) }, true);
                var generator = dynamicMethod.GetILGenerator(128);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);

                if (property.PropertyType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, property.PropertyType);
                }
                else if (property.PropertyType != typeof(object))
                {
                    generator.Emit(OpCodes.Castclass, property.PropertyType);
                }

                generator.Emit(OpCodes.Callvirt, property.GetSetMethod(true));
                generator.Emit(OpCodes.Ret);

                this.PropertyValueSetMethod = (PropertyValueSetDelegate)dynamicMethod.CreateDelegate(typeof(PropertyValueSetDelegate));
            }
        }
    }
}
