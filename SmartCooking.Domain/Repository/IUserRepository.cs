using SmartCooking.Infastructure.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCooking.Data.Repository
{
	/// <summary>
	/// Interface υπεύθυνο για το Dependecy Injection για User
	/// </summary>
	public interface IUserRepository
	{
		Task<User> GetUser(int Id);

		Task<IEnumerable<User>> GetUsers();

		Task<bool> InsertUser(User user);

		Task<bool> UpdateUser(User user);

		Task<bool> DeleteUser(User user);
	}
}