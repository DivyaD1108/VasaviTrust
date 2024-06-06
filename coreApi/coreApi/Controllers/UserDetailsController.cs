using Microsoft.AspNetCore.Mvc;
using coreApi.DataModels;
using coreApi.Services;


namespace coreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        private readonly IUserDetailsService iUserDetailsServices;

        public UserDetailsController(IUserDetailsService _iUserDetailsServices)
        {
            iUserDetailsServices = _iUserDetailsServices;
        }
        [HttpGet]
        [Route("getall")]

        public IActionResult GetUserDetails()
        { 
            UserDetailsResponse userDetailsResponse = iUserDetailsServices.GetUserDetails();
            if(userDetailsResponse.UserDetailsList!=null)
            {
                return (Ok(userDetailsResponse));
            }
            else
            {
                ErrorResponse errorResponse = new ErrorResponse
                {
                    ErrorCode = 500,
                    ErrorMessage = userDetailsResponse.REMARKS
                };
                return StatusCode(errorResponse.ErrorCode, errorResponse);
            }
        }
        [HttpPost]
        [Route("save")]
        public IActionResult SaveUserDetails(UserDetails userDetails)
        {
            SaveResponseModel userdetailsResponse = iUserDetailsServices.SaveuserDetails(userDetails);
            return (Ok(userdetailsResponse));
        }

        
    }
}
