using coreApi.DataModels;
namespace coreApi.Services
{
    public interface IUserDetailsService
    {
        public UserDetailsResponse GetUserDetails();
        public SaveResponseModel SaveuserDetails(UserDetails userDetails);
         
    }
}
