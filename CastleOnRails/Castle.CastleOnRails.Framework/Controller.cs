// Copyright 2004 DigitalCraftsmen - http://www.digitalcraftsmen.com.br/
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

namespace Castle.CastleOnRails.Framework
{
	using System;
	using System.IO;
	using System.Web;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;

	using Castle.CastleOnRails.Framework.Internal;

	/// <summary>
	/// Implements the core functionality and expose the
	/// common methods the concrete controller will usually
	/// use.
	/// </summary>
	public abstract class Controller
	{
		private IViewEngine _viewEngine;
		private IRailsEngineContext _context;
		private String _areaName;
		private String _controllerName;
		private String _selectedViewName;
		private IDictionary _bag;
		private FilterDescriptor[] _filters;
		private IFilterFactory _filterFactory;
		private String _layoutName;

		public Controller()
		{
			_bag = new HybridDictionary();
		}

		#region Usefull Properties

		public String Name
		{
			get { return _controllerName; }
		}

		public String AreaName
		{
			get { return _areaName; }
		}

		public String LayoutName
		{
			get { return _layoutName; }
			set { _layoutName = value; }
		}

		public IDictionary PropertyBag
		{
			get { return _bag; }
		}

		protected IRailsEngineContext Context
		{
			get { return _context; }
		}

		protected HttpContext HttpContext
		{
			get { return _context.UnderlyingContext as HttpContext; }
		}

		#endregion

		#region Usefull operations

		protected void RenderView( String name )
		{
			String basePath = _controllerName;

			if (_areaName != null)
			{
				basePath = Path.Combine( _areaName, _controllerName );
			}

			_selectedViewName = Path.Combine( basePath, name );
		}

		protected void RenderView( String controller, String name )
		{
			_selectedViewName = Path.Combine( controller, name );
		}

		protected void Redirect( String controller, String action )
		{
			// Cancel the view processing
			_selectedViewName = null;

			_context.Response.Redirect( 
				String.Format("../{0}/{1}.rails", controller, action), true );
		}

		#endregion

		#region Core methods

		public void Process( IRailsEngineContext context, IFilterFactory filterFactory,
			String areaName, String controllerName, String actionName, IViewEngine viewEngine )
		{
			_areaName = areaName;
			_controllerName = controllerName;
			_viewEngine = viewEngine;
			_context = context;
			_filterFactory = filterFactory;

			if (GetType().IsDefined( typeof(FilterAttribute), true ))
			{
				_filters = CollectFilterDescriptor();
			}

			if (GetType().IsDefined( typeof(LayoutAttribute), true ))
			{
				LayoutName = ObtainDefaultLayoutName();
			}

			Send( actionName );
		}

		/// <summary>
		/// Performs the Action, which means:
		/// <br/>
		/// 1. Define the default view name<br/>
		/// 2. Runs the Before Filters<br/>
		/// 3. Select the method related to the action name and invoke it<br/>
		/// 4. On error, executes the rescues if available<br/>
		/// 5. Runs the After Filters<br/>
		/// 6. Invoke the view engine<br/>
		/// </summary>
		/// <param name="action">Action name</param>
		public virtual void Send( String action )
		{
			// Specifies the default view for this area/controller/action
			RenderView( action );

			MethodInfo method = SelectMethod(action, _context.Request);

			bool skipFilters = _filters == null || method.IsDefined( typeof(SkipFilter), true );

			if (!skipFilters)
			{
				ProcessFilters( ExecuteEnum.Before );
			}

			bool hasError = false;

			try
			{
				InvokeMethod(method);
			}
			catch(Exception ex)
			{
				hasError = true;

				if (!PerformRescue(method, GetType(), ex))
				{
					throw ex;
				}
			}
			finally
			{
				if (!skipFilters)
				{
					ProcessFilters( ExecuteEnum.After );
				}
				DisposeFilter();
			}

			if (!hasError) ProcessView();
		}

		#endregion

		#region Action Invocation

		protected virtual MethodInfo SelectMethod(String action, IRequest request)
		{
			Type type = this.GetType();

			MethodInfo method = type.GetMethod( action, 
				BindingFlags.IgnoreCase|BindingFlags.Public|BindingFlags.Instance,
				null, CallingConventions.Standard, new Type[0], new ParameterModifier[0]);
	
			if (method == null)
			{
				throw new ControllerException( String.Format("No action for '{0}' found", action) );
			}

			return method;
		}

		private void InvokeMethod(MethodInfo method)
		{
			InvokeMethod(method, _context.Request);
		}

		protected virtual void InvokeMethod(MethodInfo method, IRequest request)
		{
			method.Invoke( this, new object[0] );
		}

		#endregion

		#region Filters

		private FilterDescriptor[] CollectFilterDescriptor()
		{
			object[] attrs = GetType().GetCustomAttributes( typeof(FilterAttribute), true );
			FilterDescriptor[] desc = new FilterDescriptor[attrs.Length];

			for(int i=0; i < attrs.Length; i++)
			{
				desc[i] = new FilterDescriptor(attrs[i] as FilterAttribute);
			}

			return desc;
		}

		private void ProcessFilters(ExecuteEnum when)
		{
			foreach(FilterDescriptor desc in _filters)
			{
				if ((desc.When & when) != 0)
				{
					ProcessFilter(when, desc);
				}
			}
		}

		private void ProcessFilter(ExecuteEnum when, FilterDescriptor desc)
		{
			if (desc.FilterInstance == null)
			{
				desc.FilterInstance = _filterFactory.Create( desc.FilterType );
			}

			desc.FilterInstance.Perform( when, _context,  this );
		}

		private void DisposeFilter()
		{
			if (_filters == null) return;

			foreach(FilterDescriptor desc in _filters)
			{
				if (desc.FilterInstance != null)
				{
					_filterFactory.Release(desc.FilterInstance);
				}
			}
		}

		#endregion

		#region Views and Layout

		protected virtual String ObtainDefaultLayoutName()
		{
			object[] attrs = GetType().GetCustomAttributes( typeof(LayoutAttribute), true );

			if (attrs.Length == 1)
			{
				LayoutAttribute layoutDef = (LayoutAttribute) attrs[0];
				return layoutDef.LayoutName;
			}

			return null;
		}

		private void ProcessView()
		{
			if (_selectedViewName != null)
			{
				_viewEngine.Process( _context, this, _selectedViewName );
			}
		}

		#endregion

		#region Rescue

		protected virtual bool PerformRescue(MethodInfo method, Type controllerType, Exception ex)
		{
			_context.LastException = ex;

			RescueAttribute att = null;

			if (method.IsDefined( typeof(RescueAttribute), true ))
			{
				att = method.GetCustomAttributes( 
					typeof(RescueAttribute), true )[0] as RescueAttribute;
			}
			else if (controllerType.IsDefined( typeof(RescueAttribute), true ))
			{
				att = controllerType.GetCustomAttributes( 
					typeof(RescueAttribute), true )[0] as RescueAttribute;
			}

			if (att != null)
			{
				try
				{
					_selectedViewName = String.Format( "rescues\\{0}", att.ViewName );
					ProcessView();
					return true;
				}
				catch(Exception)
				{
					// In this situation, the view could not be found
					// So we're back to the default error exibition
				}
			}

			return false;
		}

		#endregion

		#region Pre And Post view processing (overridables)

		public virtual void PreSendView(object view)
		{
			if ( view is IControllerAware )
			{
				(view as IControllerAware).SetController(this);
			}
		}

		public virtual void PostSendView(object view)
		{
		}

		#endregion
	}
}
