﻿// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Components.DictionaryAdapter
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

	public abstract partial class DictionaryAdapterBase : IDictionaryAdapter
	{
		public DictionaryAdapterBase(DictionaryAdapterInstance instance)
		{
			This = instance;

			CanEdit = typeof(IEditableObject).IsAssignableFrom(Meta.Type);
			CanNotify = typeof(INotifyPropertyChanged).IsAssignableFrom(Meta.Type);
			CanValidate = typeof(IDataErrorInfo).IsAssignableFrom(Meta.Type);

			Initialize();
		}

		public abstract DictionaryAdapterMeta Meta { get; }

		public DictionaryAdapterInstance This { get; private set; }

		public string GetKey(string propertyName)
		{
			PropertyDescriptor descriptor;
			if (Meta.Properties.TryGetValue(propertyName, out descriptor))
			{
				return descriptor.GetKey(this, propertyName, This.Descriptor);
			}
			return null;
		}

		public virtual object GetProperty(string propertyName, bool ifExists)
		{
			PropertyDescriptor descriptor;
			if (Meta.Properties.TryGetValue(propertyName, out descriptor))
			{
				var propertyValue = descriptor.GetPropertyValue(this, propertyName, null, This.Descriptor, ifExists);
				if (propertyValue is IEditableObject)
				{
					AddEditDependency((IEditableObject)propertyValue);
				}
				ComposeChildNotifications(descriptor, null, propertyValue);
				return propertyValue;
			}
			return null;
		}

		public T GetPropertyOfType<T>(string propertyName)
		{
			var propertyValue = GetProperty(propertyName, false);
			return propertyValue != null ? (T)propertyValue : default(T);
		}

		public object ReadProperty(string key)
		{
			object propertyValue = null;
			if (!GetEditedProperty(key, out propertyValue))
			{
				propertyValue = This.Dictionary[key];
			}
			return propertyValue;
		}

		public virtual bool SetProperty(string propertyName, ref object value)
		{
			bool stored = false;

			PropertyDescriptor descriptor;
			if (Meta.Properties.TryGetValue(propertyName, out descriptor))
			{
				if (ShouldNotify == false)
				{
					stored = descriptor.SetPropertyValue(this, propertyName, ref value, This.Descriptor);
					Invalidate();
					return stored;
				}

				var existingValue = GetProperty(propertyName, true);
				if (NotifyPropertyChanging(descriptor, existingValue, value) == false)
				{
					return false;
				}

				var trackPropertyChange = TrackPropertyChange(descriptor, existingValue, value);

				stored = descriptor.SetPropertyValue(this, propertyName, ref value, This.Descriptor);

				if (stored && trackPropertyChange != null)
				{
					trackPropertyChange.Notify();
				}
			}

			return stored;
		}

		public void StoreProperty(PropertyDescriptor property, string key, object value)
		{
			if (property == null || EditProperty(property, key, value) == false)
			{
				This.Dictionary[key] = value;
			}
		}

		public void ClearProperty(PropertyDescriptor property, string key)
		{
			if (property == null || ClearEditProperty(property, key) == false)
			{
				This.Dictionary.Remove(key);
			}	
		}

		public void CopyTo(IDictionaryAdapter other)
		{
			CopyTo(other, null);
		}

		public void CopyTo(IDictionaryAdapter other, Predicate<PropertyDescriptor> selector)
		{
			if (Meta.Type.IsAssignableFrom(other.Meta.Type) == false)
			{
				throw new ArgumentException(string.Format(
					"Unable to copy to {0}.  Type must be assignable from {1}.",
					other.Meta.Type.FullName, Meta.Type.FullName));
			}

			selector = selector ?? (p => true);
	
			foreach (var property in Meta.Properties.Values.Where(p => selector(p)))
			{
				var propertyValue = GetProperty(property.PropertyName, true);
				if (propertyValue != null)
					other.SetProperty(property.PropertyName, ref propertyValue);
			}
		}

		public T Coerce<T>() where T : class
		{
			return (T)This.Factory.GetAdapter(typeof(T), This.Dictionary, This.Descriptor);
		}

		public override bool Equals(object obj)
		{
			var other = obj as IDictionaryAdapter;

			if (other == null)
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}	

			if (Meta.Type != other.Meta.Type)
			{
				return false;
			}

			if (This.EqualityHashCodeStrategy != null)
			{
				return This.EqualityHashCodeStrategy.Equals(this, other);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			if (This.OldHashCode.HasValue)
			{
				return This.OldHashCode.Value;
			}

			int hashCode;
			if (This.EqualityHashCodeStrategy == null ||
				This.EqualityHashCodeStrategy.GetHashCode(this, out hashCode) == false)
			{
				hashCode = base.GetHashCode();
			}

			This.OldHashCode = hashCode;
			return hashCode;
		}

		protected void Initialize()
		{
			IEnumerable<IDictionaryInitializer> initializers = Meta.Initializers;

			if (This.Descriptor is DictionaryDescriptor)
			{
				var dictionaryDescriptor = (DictionaryDescriptor)This.Descriptor;
				if (dictionaryDescriptor.Initializers != null)
				{
					initializers = dictionaryDescriptor.Initializers.Union(initializers.OrderBy(i => i.ExecutionOrder));
				}
			}

			foreach (var initializer in initializers)
			{
				initializer.Initialize(this, Meta.Behaviors);
			}

			foreach (var property in Meta.Properties.Values.Where(p => p.Fetch))
			{
				GetProperty(property.PropertyName, false);
			}
		}
	}
}
