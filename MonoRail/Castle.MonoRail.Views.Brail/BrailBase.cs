// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
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

using System.Diagnostics;
using System.Text;

namespace Castle.MonoRail.Views.Brail
{
	using System;
	using System.Collections;
	using System.IO;
	using Castle.MonoRail.Framework;
	/// <summary>
	///Base class for all the view scripts, this is the class that is responsible for
	/// support all the behind the scenes magic such as variable to PropertyBag trasnlation, 
	/// resources usage, etc. 
	/// </summary>
	public abstract class BrailBase
	{
		private TextWriter outputStream;

		/// <summary>
		/// This is used by layout scripts only, for outputing the child's content
		/// </summary>
		protected TextWriter childOutput;
		private Hashtable properties;
		/// <summary>
		/// used to hold the ComponentParams from the view, so their views/sections could access them
		/// </summary>
		private IList viewComponentsParameters;

		protected IRailsEngineContext context;
		protected Controller __controller;
		protected BooViewEngine viewEngine;

		/// <summary>
		/// usually used by the layout to refer to its view, or a subview to its parent
		/// </summary>
		protected BrailBase parent;

        /// <summary>
        /// Reference to the DSL service
        /// </summary>
        private DslProvider _dsl;

		/// <summary>
		/// Initializes a new instance of the <see cref="BrailBase"/> class.
		/// </summary>
		/// <param name="viewEngine">The view engine.</param>
		/// <param name="output">The output.</param>
		/// <param name="context">The context.</param>
		/// <param name="__controller">The controller.</param>
		public BrailBase(BooViewEngine viewEngine, TextWriter output, IRailsEngineContext context, Controller __controller)
		{
			this.viewEngine = viewEngine;
			this.outputStream = output;
			this.context = context;
			this.__controller = __controller;
			InitProperties(context, __controller);
		}

		/// <summary>
		/// Runs this instance, this is generated by the script
		/// </summary>
		public abstract void Run();


		/// <summary>
		///The path of the script, this is filled by AddBrailBaseClassStep
		/// and is used for sub views 
		/// </summary>
		public virtual string ScriptDirectory
		{
			get { return viewEngine.ViewRootDir; }
		}

		/// <summary>
		/// Gets the view engine.
		/// </summary>
		/// <value>The view engine.</value>
		public BooViewEngine ViewEngine
		{
			get { return viewEngine; }
		}

        /// <summary>
        /// Gets the DSL provider
        /// </summary>
        /// <value>Reference to the current DSL Provider</value>
        public DslProvider Dsl
        {
            get
            {
                BrailBase view = this;
                if (null == view._dsl)
                {
                    view._dsl = new DslProvider(view);
                }

                return view._dsl;
                //while (view.parent != null)
                //{
                //    view = view.parent;
                //}

                //if (view._dsl == null)
                //{
                //    view._dsl = new DslProvider(view);
                //}

                //return view._dsl;
            }
        }

		/// <summary>
		/// Output the subview to the client, this is either a relative path "SubView" which
		/// is relative to the current /script/ or an "absolute" path "/home/menu" which is
		/// actually relative to ViewDirRoot
		/// </summary>
		/// <param name="subviewName"></param>
		public void OutputSubView(string subviewName)
		{
			OutputSubView(subviewName, new Hashtable());
		}

		/// <summary>
		/// Similiar to the OutputSubView(string) function, but with a bunch of parameters that are used
		/// just for this subview. This parameters are /not/ inheritable.
		/// </summary>
		/// <param name="subviewName"></param>
		/// <param name="parameters"></param>
		public void OutputSubView(string subviewName, IDictionary parameters)
		{
			OutputSubView(subviewName, outputStream, parameters);
		}

		/// <summary>
		/// Outputs the sub view to the writer
		/// </summary>
		/// <param name="subviewName">Name of the subview.</param>
		/// <param name="writer">The writer.</param>
		/// <param name="parameters">The parameters.</param>
		public void OutputSubView(string subviewName, TextWriter writer, IDictionary parameters)
		{
			string subViewFileName = GetSubViewFilename(subviewName);
			BrailBase subView = viewEngine.GetCompiledScriptInstance(subViewFileName, writer, context, __controller);
			subView.SetParent(this);
			foreach (DictionaryEntry entry in parameters)
			{
				subView.properties[entry.Key] = entry.Value;
			}
			subView.Run();
		}

		/// <summary>
		/// Get the sub view file name, if the subview starts with a '/' 
		/// then the filename is considered relative to ViewDirRoot
		/// otherwise, it's relative to the current script directory
		/// </summary>
		/// <param name="subviewName"></param>
		/// <returns></returns>
		public string GetSubViewFilename(string subviewName)
		{
			//absolute path from Views directory
			if (subviewName[0] == '/')
				return subviewName.Substring(1) + viewEngine.ViewFileExtension;
			return Path.Combine(ScriptDirectory, subviewName) + viewEngine.ViewFileExtension;
		}

		/// <summary>
		/// this is called by ReplaceUnknownWithParameters step to create a more dynamic experiance
		/// any uknown identifier will be translate into a call for GetParameter('identifier name').
		/// This mean that when an uknonwn identifier is in the script, it will only be found on runtime.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public object GetParameter(string name)
		{
			ParameterSearch search = GetParameterInternal(name);
			if (search.Found == false)
				throw new RailsException("Parameter '" + name + "' was not found!");
			return search.Value;
		}

		/// <summary>
		/// this is called by ReplaceUnknownWithParameters step to create a more dynamic experiance
		/// any uknown identifier with the prefix of ? will be translated into a call for 
		/// TryGetParameter('identifier name without the ? prefix').
		/// This method will return null if the value it not found.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public object TryGetParameter(string name)
		{
			ParameterSearch search = GetParameterInternal(name);
			return new IgnoreNull(search.Value);
		}

		/// <summary>
		/// Gets the parameter - implements the logic for searching parameters.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private ParameterSearch GetParameterInternal(string name)
		{
			//temporary syntax to turn @variable to varaible, imitating :symbol in ruby
			if (name.StartsWith("@"))
				return new ParameterSearch(name.Substring(1), true);
			if (viewComponentsParameters != null)
			{
				foreach (IDictionary viewComponentProperties in viewComponentsParameters)
				{
					if (viewComponentProperties.Contains(name))
						return new ParameterSearch(viewComponentProperties[name], true);
				}
			}
			if (properties.Contains(name))
				return new ParameterSearch(properties[name], true);
			if (parent != null)
				return parent.GetParameterInternal(name);
			return new ParameterSearch(null, false);
		}

		/// <summary>
		/// Sets the parent.
		/// </summary>
		/// <param name="myParent">My parent.</param>
		public void SetParent(BrailBase myParent)
		{
			parent = myParent;
		}

		/// <summary>
		/// Allows to check that a parameter was defined
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsDefined(string name)
		{
			ParameterSearch search = GetParameterInternal(name);
			return search.Found;
		}

		/// <summary>
		/// Gets the flash.
		/// </summary>
		/// <value>The flash.</value>
		public Flash Flash
		{
			get { return context.Flash; }
		}

		/// <summary>
		/// Gets the output stream.
		/// </summary>
		/// <value>The output stream.</value>
		public TextWriter OutputStream
		{
			get { return outputStream; }
		}

		/// <summary>
		/// This is required because we may want to replace the output stream and get the correct
		/// behavior from components call RenderText() or RenderSection()
		/// </summary>
		public IDisposable SetOutputStream(TextWriter newOutputStream)
		{
			ReturnOutputStreamToInitialWriter disposable = new ReturnOutputStreamToInitialWriter(OutputStream, this);
			outputStream = newOutputStream;
			return disposable;
		}

		/// <summary>
		/// Gets or sets the child output.
		/// </summary>
		/// <value>The child output.</value>
		public TextWriter ChildOutput
		{
			get { return childOutput; }
			set { childOutput = value; }
		}

		/// <summary>
		/// Gets the properties.
		/// </summary>
		/// <value>The properties.</value>
		public IDictionary Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Note that this will overwrite any existing property.
		/// </summary>
		public void AddProperty(string name, object item)
		{
			properties[name] = item;
		}

		/// <summary>
		/// Adds the view component newProperties.
		/// This will be included in the parameters searching, note that this override
		/// the current parameters if there are clashing.
		/// The search order is LIFO
		/// </summary>
		/// <param name="newProperties">The newProperties.</param>
		public void AddViewComponentProperties(IDictionary newProperties)
		{
			if (viewComponentsParameters == null)
				viewComponentsParameters = new ArrayList();
			viewComponentsParameters.Insert(0, newProperties);
		}

		/// <summary>
		/// Removes the view component properties, so they will no longer be visible to the views.
		/// </summary>
		/// <param name="propertiesToRemove">The properties to remove.</param>
		public void RemoveViewComponentProperties(IDictionary propertiesToRemove)
		{
			if (viewComponentsParameters == null)
				return;
			viewComponentsParameters.Remove(propertiesToRemove);
		}
        public void RenderComponent(string componentName)
        {
            RenderComponent(componentName, new Hashtable());
        }
        public void RenderComponent(string componentName, IDictionary parameters)
        {
            BrailViewComponentContext componentContext =
                new BrailViewComponentContext(this, null, componentName, OutputStream, 
                new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            this.AddViewComponentProperties(componentContext.ComponentParameters);
            IViewComponentFactory componentFactory = (IViewComponentFactory)this.context.GetService(typeof(IViewComponentFactory));
            ViewComponent component = componentFactory.Create(componentName);
            component.Init(this.context, componentContext);
            component.Render();
            if (componentContext.ViewToRender != null)
            {
                this.OutputSubView("/" + componentContext.ViewToRender, componentContext.ComponentParameters);
            }
            this.RemoveViewComponentProperties(componentContext.ComponentParameters);

        }

		/// <summary>
		/// Initialize all the properties that a script may need
		/// One thing to note here is that resources are wrapped in ResourceToDuck wrapper
		/// to enable easy use by the script
		/// </summary>
		/// <param name="myContext"></param>
		/// <param name="myController"></param>
		private void InitProperties(IRailsEngineContext myContext, Controller myController)
		{
			properties = new Hashtable(
#if DOTNET2
StringComparer.InvariantCultureIgnoreCase
#else
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default
#endif
);
            //properties.Add("dsl", new DslWrapper(this));
			properties.Add("Controller", myController);
			properties.Add("request", myContext.Request);
			properties.Add("response", myContext.Response);
			properties.Add("session", myContext.Session);

			if (myController.Resources != null)
			{
				foreach (object key in myController.Resources.Keys)
				{
					properties.Add(key, new ResourceToDuck(myController.Resources[key]));
				}
			}

			foreach (DictionaryEntry entry in myController.Helpers)
			{
				properties.Add(entry.Key, entry.Value);
			}

			foreach (string key in myController.Params.AllKeys)
			{
				if (key == null)
					continue;
				properties[key] = myContext.Params[key];
			}

			foreach (DictionaryEntry entry in myContext.Flash)
			{
				properties[entry.Key] = entry.Value;
			}

			foreach (DictionaryEntry entry in myController.PropertyBag)
			{
				properties[entry.Key] = entry.Value;
			}

			properties["siteRoot"] = myContext.ApplicationPath;
		}

		private class ParameterSearch
		{
			private bool found;
			private object value;

			public bool Found
			{
				get { return found; }
			}

			public object Value
			{
				get { return value; }
			}

			public ParameterSearch(object value, bool found)
			{
				this.found = found;
				this.value = value;
			}
		}

		private class ReturnOutputStreamToInitialWriter : IDisposable
		{
			private TextWriter initialWriter;
			private BrailBase parent;

			public ReturnOutputStreamToInitialWriter(TextWriter initialWriter, BrailBase parent)
			{
				this.initialWriter = initialWriter;
				this.parent = parent;
			}

			public void Dispose()
			{
				parent.outputStream = initialWriter;
			}
		}
	}
}
