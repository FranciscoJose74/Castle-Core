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

namespace Castle.DynamicProxy.Builder.CodeGenerators
{
	using System;
	using System.Text;
	using System.Reflection;
	using System.Runtime.Serialization;

	using Castle.DynamicProxy.Invocation;
	using Castle.DynamicProxy.Builder.CodeBuilder;
	using Castle.DynamicProxy.Builder.CodeBuilder.SimpleAST;

	/// <summary>
	/// Summary description for InterfaceProxyGenerator.
	/// </summary>
	public class InterfaceProxyGenerator : BaseCodeGenerator
	{
		protected FieldReference _targetField;

		public InterfaceProxyGenerator(ModuleScope scope) : base(scope)
		{
		}

		public InterfaceProxyGenerator(ModuleScope scope, GeneratorContext context) : base(scope, context)
		{
		}

		protected override Type InvocationType
		{
			get { return Context.InterfaceInvocation; }
		}

		protected override String GenerateTypeName(Type type, Type[] interfaces)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Type inter in interfaces)
			{
				sb.Append('_');
				sb.Append(inter.Name);
			}
			/// Naive implementation
			return String.Format("ProxyInterface{0}{1}", type.Name, sb.ToString());
		}

		protected override void GenerateFields()
		{
			base.GenerateFields ();
			_targetField = MainTypeBuilder.CreateField("__target", typeof (object));
		}

		/// <summary>
		/// Generates one public constructor receiving 
		/// the <see cref="IInterceptor"/> instance and instantiating a HybridCollection
		/// </summary>
		protected override EasyConstructor GenerateConstructor()
		{
			ArgumentReference arg1 = new ArgumentReference( Context.Interceptor );
			ArgumentReference arg2 = new ArgumentReference( typeof(object) );
			ArgumentReference arg3 = new ArgumentReference( typeof(object[]) );

			EasyConstructor constructor;

			if (Context.HasMixins)
			{
				constructor = MainTypeBuilder.CreateConstructor( arg1, arg2, arg3 );
			}
			else
			{
				constructor = MainTypeBuilder.CreateConstructor( arg1, arg2 );
			}

			constructor.CodeBuilder.InvokeBaseConstructor();

			constructor.CodeBuilder.AddStatement( new AssignStatement(
				_targetField, arg2.ToExpression()) );

			GenerateConstructorCode(constructor.CodeBuilder, arg1, arg2, arg3);
			
			return constructor;
		}

		protected Type[] Join(Type[] interfaces, Type[] mixinInterfaces)
		{
			Type[] union = new Type[ interfaces.Length + mixinInterfaces.Length ];
			Array.Copy( interfaces, 0, union, 0, interfaces.Length );
			Array.Copy( mixinInterfaces, 0, union, interfaces.Length, mixinInterfaces.Length );
			return union;
		}

		protected override void CustomizeGetObjectData(AbstractCodeBuilder codebuilder, ArgumentReference arg1, ArgumentReference arg2)
		{
			Type[] key_and_object = new Type[] {typeof (String), typeof (Object)};
			MethodInfo addValueMethod = typeof (SerializationInfo).GetMethod("AddValue", key_and_object);

			codebuilder.AddStatement( new ExpressionStatement(
				new VirtualMethodInvocationExpression(arg1, addValueMethod, 
				new FixedReference("__target").ToExpression(), 
				_targetField.ToExpression() ) ) );
		}

		public virtual Type GenerateCode(Type[] interfaces)
		{
			if (Context.HasMixins)
			{
				_mixins = Context.MixinsAsArray();
				Type[] mixinInterfaces = InspectAndRegisterInterfaces( _mixins );
				interfaces = Join(interfaces, mixinInterfaces);
			}

			interfaces = AddISerializable(interfaces);

			Type cacheType = GetFromCache(typeof(Object), interfaces);
			
			if (cacheType != null)
			{
				return cacheType;
			}

			CreateTypeBuilder( typeof(Object), interfaces );
			GenerateFields();
			ImplementGetObjectData( interfaces );
			ImplementCacheInvocationCache();
			GenerateInterfaceImplementation( interfaces );
			GenerateConstructor();
			return CreateType();
		}
	}
}
