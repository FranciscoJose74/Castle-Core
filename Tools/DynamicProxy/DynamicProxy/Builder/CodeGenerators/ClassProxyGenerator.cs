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
	/// Summary description for ClassProxyGenerator.
	/// </summary>
	public class ClassProxyGenerator : BaseCodeGenerator
	{
		private static readonly Type INVOCATION_TYPE = typeof(SameClassInvocation);

		private bool m_delegateToBaseGetObjectData;

		public ClassProxyGenerator(ModuleScope scope) : base(scope)
		{
		}

		public ClassProxyGenerator(ModuleScope scope, GeneratorContext context) : base(scope, context)
		{
		}

		protected override Type InvocationType
		{
			get { return INVOCATION_TYPE; }
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
			return String.Format("CProxyType{0}{1}{2}", type.Name, sb.ToString(), interfaces.Length);
		}

		/// <summary>
		/// Generates one public constructor receiving 
		/// the <see cref="IInterceptor"/> instance and instantiating a hashtable
		/// </summary>
		protected override EasyConstructor GenerateConstructor()
		{
			ArgumentReference arg1 = new ArgumentReference( typeof(IInterceptor) );
			ArgumentReference arg2 = new ArgumentReference( typeof(object[]) );

			EasyConstructor constructor;

			if (Context.HasMixins)
			{
				constructor = MainTypeBuilder.CreateConstructor( arg1, arg2 );
			}
			else
			{
				constructor = MainTypeBuilder.CreateConstructor( arg1 );
			}

			constructor.CodeBuilder.InvokeBaseConstructor();

			GenerateConstructorCode(constructor.CodeBuilder, arg1, SelfReference.Self, arg2);
		
			return constructor;
		}

		protected void GenerateSerializationConstructor( EasyConstructor defConstructor )
		{
			// TODO: Adjust to mixin constructors too

			ArgumentReference arg1 = new ArgumentReference( typeof(SerializationInfo) );
			ArgumentReference arg2 = new ArgumentReference( typeof(StreamingContext) );

			EasyConstructor constr = MainTypeBuilder.CreateConstructor( arg1, arg2 );

			Type[] object_arg = new Type[] { typeof (String), typeof(Type) };
			MethodInfo getValueMethod = typeof (SerializationInfo).GetMethod("GetValue", object_arg);

			VirtualMethodInvocationExpression getValueInvocation =
				new VirtualMethodInvocationExpression(arg1, getValueMethod, 
				new FixedReference("__interceptor").ToExpression(), 
				new TypeTokenExpression( typeof(IInterceptor) ) );

			constr.CodeBuilder.AddStatement( new ExpressionStatement(
				new ConstructorInvocationExpression( defConstructor.Builder, 
				new ConvertExpression( typeof(IInterceptor), getValueInvocation ) )) );
			
			
			
			constr.CodeBuilder.AddStatement( new ReturnStatement() );
		}

		protected override void CustomizeGetObjectData(AbstractCodeBuilder codebuilder, 
			ArgumentReference arg1, ArgumentReference arg2)
		{
			Type[] key_and_object = new Type[] {typeof (String), typeof (Object)};
			Type[] key_and_bool = new Type[] {typeof (String), typeof (bool)};
			MethodInfo addValueMethod = typeof (SerializationInfo).GetMethod("AddValue", key_and_object);
			MethodInfo addValueBoolMethod = typeof (SerializationInfo).GetMethod("AddValue", key_and_bool);

			codebuilder.AddStatement( new ExpressionStatement(
				new VirtualMethodInvocationExpression(arg1, addValueBoolMethod, 
				new FixedReference("__delegateToBase").ToExpression(), 
				new FixedReference( m_delegateToBaseGetObjectData ? 1 : 0 ).ToExpression() ) ) );

			if (m_delegateToBaseGetObjectData)
			{
				MethodInfo baseGetObjectData = m_baseType.GetMethod("GetObjectData", 
					new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

				codebuilder.AddStatement( new ExpressionStatement(
					new MethodInvocationExpression( baseGetObjectData, 
						arg1.ToExpression(), arg2.ToExpression() )) );
			}
			else
			{
				LocalReference members_ref = codebuilder.DeclareLocal( typeof(MemberInfo[]) );
				LocalReference data_ref = codebuilder.DeclareLocal( typeof(object[]) );

				MethodInfo getSerMembers = typeof(FormatterServices).GetMethod("GetSerializableMembers", 
					new Type[] { typeof(Type) });
				MethodInfo getObjData = typeof(FormatterServices).GetMethod("GetObjectData", 
					new Type[] { typeof(object), typeof(MemberInfo[]) });
				
				codebuilder.AddStatement( new AssignStatement( members_ref,
					new MethodInvocationExpression( null, getSerMembers, 
					new TypeTokenExpression( m_baseType ) )) );
				
				codebuilder.AddStatement( new AssignStatement( data_ref, 
					new MethodInvocationExpression( null, getObjData, 
					SelfReference.Self.ToExpression(), members_ref.ToExpression() )) );

				codebuilder.AddStatement( new ExpressionStatement(
					new VirtualMethodInvocationExpression(arg1, addValueMethod, 
					new FixedReference("__data").ToExpression(), 
					data_ref.ToExpression() ) ) );
			}
		}

		public virtual Type GenerateCode(Type baseClass)
		{
			return GenerateCode(baseClass, new Type[0]);
		}	
		
		public virtual Type GenerateCode(Type baseClass, Type[] interfaces)
		{
			if (baseClass.IsSerializable)
			{
				m_delegateToBaseGetObjectData = VerifyIfBaseImplementsGetObjectData(baseClass);
				interfaces = AddISerializable( interfaces );
			}

			Type cacheType = GetFromCache(baseClass, interfaces);
			
			if (cacheType != null)
			{
				return cacheType;
			}

			CreateTypeBuilder( baseClass, interfaces );
			GenerateFields();

			if (baseClass.IsSerializable)
			{
				ImplementGetObjectData( interfaces );
			}

			ImplementCacheInvocationCache();
			GenerateTypeImplementation( baseClass, true );
			GenerateInterfaceImplementation(interfaces);
			GenerateConstructor();
			return CreateType();
		}

		public virtual Type GenerateCustomCode(Type baseClass)
		{
			if (!Context.HasMixins)
			{
				return GenerateCode(baseClass);
			}

			m_mixins = Context.MixinsAsArray();
			Type[] mixinInterfaces = InspectAndRegisterInterfaces( m_mixins );

			return GenerateCode(baseClass, mixinInterfaces);
		}

		protected bool VerifyIfBaseImplementsGetObjectData(Type baseType)
		{
			// If base type implements ISerializable, we have to make sure
			// the GetObjectData is marked as virtual
			if (typeof(ISerializable).IsAssignableFrom(baseType))
			{
				MethodInfo getObjectDataMethod = baseType.GetMethod("GetObjectData", 
					new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

				if (!getObjectDataMethod.IsVirtual || getObjectDataMethod.IsFinal)
				{
					String message = String.Format("The type {0} implements ISerializable, but GetObjectData is not marked as virtual", 
						baseType.FullName);
					throw new ProxyGenerationException(message);
				}

				ConstructorInfo serializationConstructor = baseType.GetConstructor(
					new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

				if (serializationConstructor == null)
				{
					String message = String.Format("The type {0} implements ISerializable, but failed to provide a deserialization constructor", 
						baseType.FullName);
					throw new ProxyGenerationException(message);
				}

				return true;
			}
			return false;
		}

		protected void SkipDefaultInterfaceImplementation(Type[] interfaces)
		{
			foreach( Type inter in interfaces )
			{
				Context.AddInterfaceToSkip(inter);
			}
		}

		protected Type[] Join(Type[] interfaces, Type[] mixinInterfaces)
		{
			Type[] union = new Type[ interfaces.Length + mixinInterfaces.Length ];
			Array.Copy( interfaces, 0, union, 0, interfaces.Length );
			Array.Copy( mixinInterfaces, 0, union, interfaces.Length, mixinInterfaces.Length );
			return union;
		}

		protected override MethodInfo GenerateCallbackMethodIfNecessary(MethodInfo method)
		{
			if (Context.HasMixins && m_interface2mixinIndex.Contains(method.DeclaringType))
			{
				return method;
			}

			String name = String.Format("callback__{0}", method.Name);

			ParameterInfo[] parameters = method.GetParameters();

			ArgumentReference[] args = new ArgumentReference[ parameters.Length ];
			
			for(int i=0; i < args.Length; i++)
			{
				args[i] = new ArgumentReference( parameters[i].ParameterType );
			}

			EasyMethod easymethod = MainTypeBuilder.CreateMethod(name, 
				new ReturnReferenceExpression(method.ReturnType), args);

			Expression[] exps = new Expression[ parameters.Length ];
			
			for(int i=0; i < args.Length; i++)
			{
				exps[i] = args[i].ToExpression();
			}

			easymethod.CodeBuilder.AddStatement(
				new ReturnStatement( 
					new MethodInvocationExpression(method, exps) ) );

			return easymethod.MethodBuilder;
		}
	}
}
