﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xaml;
using Alba.CsConsoleFormat.Markup;

namespace Alba.CsConsoleFormat
{
    public class BindableObject : ISupportInitialize, IAttachedPropertyStore
    {
        private object _dataContext;
        private IDictionary<PropertyInfo, GetExpressionBase> _getters;
        private IDictionary<AttachableMemberIdentifier, object> _attachedProperties;

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (_dataContext == value)
                    return;
                _dataContext = value;
                UpdateDataContext();
            }
        }

        private void UpdateDataContext ()
        {
            if (_getters == null)
                return;
            foreach (KeyValuePair<PropertyInfo, GetExpressionBase> getter in _getters)
                getter.Key.SetValue(this, getter.Value.GetValue(this));
        }

        public void Bind (PropertyInfo prop, GetExpressionBase getter)
        {
            if (_getters == null)
                _getters = new SortedList<PropertyInfo, GetExpressionBase>();
            _getters[prop] = getter;
        }

        public BindableObject Clone ()
        {
            BindableObject clone = CreateInstance();
            clone.CloneOverride(this);
            return clone;
        }

        protected virtual void CloneOverride (BindableObject source)
        {}

        protected virtual BindableObject CreateInstance ()
        {
            return (BindableObject)MemberwiseClone();
        }

        void ISupportInitialize.BeginInit ()
        {
            BeginInit();
        }

        void ISupportInitialize.EndInit ()
        {
            EndInit();
        }

        protected virtual void BeginInit ()
        {}

        protected virtual void EndInit ()
        {}

        int IAttachedPropertyStore.PropertyCount
        {
            get { return _attachedProperties != null ? _attachedProperties.Count : 0; }
        }

        bool IAttachedPropertyStore.TryGetProperty (AttachableMemberIdentifier identifier, out object value)
        {
            if (_attachedProperties == null || !_attachedProperties.TryGetValue(identifier, out value)) {
                value = AttachedProperty.Get(identifier).DefaultValueUntyped;
                return false;
            }
            return true;
        }

        void IAttachedPropertyStore.SetProperty (AttachableMemberIdentifier identifier, object value)
        {
            if (_attachedProperties == null)
                _attachedProperties = new ConcurrentDictionary<AttachableMemberIdentifier, object>();
            _attachedProperties[identifier] = value;
        }

        bool IAttachedPropertyStore.RemoveProperty (AttachableMemberIdentifier identifier)
        {
            return _attachedProperties != null && _attachedProperties.Remove(identifier);
        }

        void IAttachedPropertyStore.CopyPropertiesTo (KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            if (_attachedProperties == null)
                return;
            _attachedProperties.CopyTo(array, index);
        }

        public T GetValue<T> (AttachedProperty<T> property)
        {
            object value;
            return _attachedProperties == null || !_attachedProperties.TryGetValue(property.Identifier, out value)
                ? property.DefaultValue : (T)value;
        }

        public void SetValue<T> (AttachedProperty<T> property, T value)
        {
            if (_attachedProperties == null)
                _attachedProperties = new ConcurrentDictionary<AttachableMemberIdentifier, object>();
            _attachedProperties[property.Identifier] = value;
        }

        public void ResetValue<T> (AttachedProperty<T> property)
        {
            if (_attachedProperties == null)
                return;
            _attachedProperties.Remove(property.Identifier);
        }
    }
}