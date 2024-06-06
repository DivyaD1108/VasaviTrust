using Microsoft.Extensions.Options;
using coreApi.DataAccess;
using coreApi.DataModels;
using coreApi.Services;
using coreApi.Utilities.Extensions;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace coreApi.Services
{
    public class UserDetailsServices: IUserDetailsService
    {
        private readonly DatabaseSettings databaseSettings;

        public UserDetailsServices(IOptions<DatabaseSettings>options)
        {
            databaseSettings = options.Value;
        }
        public UserDetailsResponse GetUserDetails()
        {
            UserDetailsResponse userdetailsResponse = new UserDetailsResponse();

            List<UserDetails> userdetailsList = new List<UserDetails>();
            try
            {
                DataSet dsUserdetails = new DataSet();
                dsUserdetails = SqlHelper.ExecuteDataset(databaseSettings.ConnectionString, CommandType.StoredProcedure, "usp_solis_UserDetails_GetAll");
                if (!dsUserdetails.IsEmpty())
                {
                    foreach (DataRow drItem in dsUserdetails.Tables[0].Rows)
                    {
                        UserDetails userdetails = new UserDetails
                        {
                            ID = drItem.GetValue<string>("ID"),
                            USERNAME = drItem.GetValue<string>("USERNAME"),
                            USERPASSWORD = drItem.GetValue<string>("USERPASSWORD"),
                            EMPLOYEENAME = drItem.GetValue<string>("EMPLOYEENAME")
                        };
                        userdetailsList.Add(userdetails);
                    }
                    userdetailsResponse.UserDetailsList = userdetailsList;
                    userdetailsResponse.RESPONSESTATUS = GlobalConstants.Success_Result;
                }
                else
                {
                    userdetailsResponse.UserDetailsList = null;
                    userdetailsResponse.RESPONSESTATUS = GlobalConstants.Failure_Result;
                    userdetailsResponse.REMARKS = "Data not available";
                }
            }
            catch (Exception ex)
            {
                userdetailsResponse.UserDetailsList = null;
                userdetailsResponse.RESPONSESTATUS = GlobalConstants.Failure_Result;
                userdetailsResponse.REMARKS = ex.Message;
            }
            return userdetailsResponse;
        }
        public SaveResponseModel SaveuserDetails(UserDetails userDetails)
        {
            SaveResponseModel userdetailsResponse = new SaveResponseModel();
            string returnValue = "";
            try
            {
                bool isExists = false;
                UserDetailsResponse userdetailsList = GetUserDetails();

                if(userdetailsList.UserDetailsList!=null)
                {
                    foreach(UserDetails item in userdetailsList.UserDetailsList)
                    {
                        if(item.ID.ToUpper().Equals(userDetails.ID.ToUpper()))
                        {
                            isExists = true;
                            break;
                        }
                    }
                }
                string generatedPassword = string.Empty;
                if (!isExists && userDetails.USERNAME != "")
                {
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    sqlParameters.Add(Custom.AddParam(SqlDbType.VarChar, "P_ID", userDetails.ID,25));
                    sqlParameters.Add(Custom.AddParam(SqlDbType.VarChar, "IP_USERNAME", userDetails.USERNAME, 75));
                    sqlParameters.Add(Custom.AddParam(SqlDbType.VarChar, "IP_USERPASSWORD", userDetails.USERPASSWORD, 50));
                    sqlParameters.Add(Custom.AddParam(SqlDbType.VarChar, "IP_EMPLOYEENAME", userDetails.EMPLOYEENAME, 75));
                    sqlParameters.Add(Custom.AddParam(SqlDbType.VarChar, "OP_DATA", String.Empty, 25, true));
                    returnValue = SqlHelper.ExecuteNonQueryString(databaseSettings.ConnectionString, CommandType.StoredProcedure, "usp_solis_UserDetails_SAVE", sqlParameters.ToArray());
                    if(returnValue!=null)
                    {
                        userdetailsResponse.UniqueId = returnValue;
                        userdetailsResponse.RESPONSESTATUS = GlobalConstants.Success_Result;
                        userdetailsResponse.REMARKS = "Data inserted";
                    }
                    else
                    {
                        userdetailsResponse.UniqueId = returnValue;
                        userdetailsResponse.RESPONSESTATUS = GlobalConstants.Failure_Result;
                        userdetailsResponse.REMARKS = "Data not inserted";
                    }
                }
                else
                {
                    userdetailsResponse.UniqueId = null;
                    userdetailsResponse.RESPONSESTATUS = GlobalConstants.Failure_Result;
                    userdetailsResponse.REMARKS = "Data already exists or enter a valid data";
                }
            }

            catch(Exception ex)
            {
                userdetailsResponse.UniqueId = null;
                userdetailsResponse.RESPONSESTATUS = GlobalConstants.Failure_Result;
                userdetailsResponse.REMARKS = ex.Message;
            }
            return userdetailsResponse;
        }
    }
}
