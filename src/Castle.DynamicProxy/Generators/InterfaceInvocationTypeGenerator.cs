// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.DynamicProxy.Generators
{
	using System;
	using System.Reflection;
	using Emitters;
	using Emitters.SimpleAST;
	using Tokens;

	public class InterfaceInvocationTypeGenerator : InvocationTypeGenerator
	{
		public InterfaceInvocationTypeGenerator(Type target, IProxyMethod method, MethodInfo callback, bool canChangeTarget)
			: base(target, method, callback, canChangeTarget)
		{
		}

		protected override void ImplementInvokeMethodOnTarget(NestedClassEmitter nested, ParameterInfo[] parameters, MethodEmitter method, MethodInfo callbackMethod, Reference targetField)
		{
			method.CodeBuilder.AddStatement(
				new ExpressionStatement(
					new MethodInvocationExpression(SelfReference.Self, InvocationMethods.EnsureValidTarget)));
			base.ImplementInvokeMethodOnTarget(nested, parameters, method, callbackMethod, targetField);
		}

	}
}