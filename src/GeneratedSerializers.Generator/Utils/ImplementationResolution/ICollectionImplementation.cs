using System;
using System.Linq;

namespace GeneratedSerializers
{
	/// <summary>
	/// Some metadata for the implementation type of a collection 
	/// </summary>
	public interface ICollectionImplementation : IImplementation
	{
		/// <summary>
		/// Gets the code needed to create an instance of <seealso cref="IImplementation.Implementation"/>.
		/// </summary>
		/// <param name="colectionVariable">Name of the variable to initialize</param>
		/// <returns>The generated code</returns>
		string CreateInstance(string colectionVariable);

		/// <summary>
		/// Gets the code needed to add an item to an instance of <seealso cref="IImplementation.Implementation"/>.
		/// </summary>
		/// <param name="colectionVariable">Name of the variable which contains the instance</param>
		/// <param name="itemVariable">Name of the variable which contains the item to add</param>
		/// <returns>The generated code</returns>
		string AddItemToInstance(string colectionVariable, string itemVariable);

		/// <summary>
		/// Gets the code needed to convert back from an instance of <seealso cref="IImplementation.Implementation"/> to the originally requested type.
		/// </summary>
		/// <param name="colectionVariable">Name of the variable which contains the instance</param>
		/// <param name="resultVariable">Name of the variable of the originaly requested type to assign</param>
		/// <returns>The generated code</returns>
		string InstanceToContract(string colectionVariable, string resultVariable);
	}
}
