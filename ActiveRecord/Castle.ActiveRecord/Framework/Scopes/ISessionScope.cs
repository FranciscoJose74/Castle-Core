// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
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

namespace Castle.ActiveRecord
{
	using System;

	using NHibernate;

	public enum SessionScopeType
	{
		Undefined,
		Simple,
		Transactional,
		Custom
	}

	/// <summary>
	/// Contract for implementation of scopes.
	/// </summary>
	/// <remarks>
	/// A scope can implement a logic that affects 
	/// AR for the scope lifetime. Session cache and
	/// transaction are the best examples, but you 
	/// can create new scopes adding new semantics.
	/// </remarks>
	public interface ISessionScope : IDisposable
	{
		SessionScopeType ScopeType { get; }

		/// <summary>
		/// Implementors should return true 
		/// if the key is know. The key might be whatever the
		/// scope implementation wants
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		bool IsKeyKnown(object key);

		/// <summary>
		/// Associates a session with this scope
		/// </summary>
		/// <param name="key"></param>
		/// <param name="session"></param>
		void RegisterSession(object key, ISession session);

		/// <summary>
		/// Returns the session associated with the key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		ISession GetSession(object key);
	}
}
