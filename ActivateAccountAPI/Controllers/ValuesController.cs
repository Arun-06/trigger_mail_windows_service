using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ActivateAccountAPI.Controllers
{
    public class ValuesController : ApiController
    {
        
        //api/values/ActivateAccount/101
        [HttpGet]
        public string ActivateAccount(int id)
        {
            string strRet = "not a valid user id";
            try
            {
                if (IsValidUser(id))
                {
                    strRet = ActivateUser(id);
                }
            }
            catch (Exception e)
            {

            }
            return strRet;
        }
        private bool IsValidUser(int id)
        {
            bool blnret = true;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["accountactivation"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from usertable where isactive = @active and user_id = @userid;", conn);
                    cmd.Parameters.AddWithValue("@active", 0);
                    cmd.Parameters.AddWithValue("@userid", id);
                    SqlDataReader sdr = cmd.ExecuteReader();
                    if (!sdr.HasRows)
                        blnret = false;
                    sdr.Close();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                blnret = false;
            }
            return blnret;
        }
        private string ActivateUser(int id)
        {
            string ret = "WELLCOME";
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["accountactivation"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("update usertable set isactive = @active where user_id = @userid;", conn);
                    cmd.Parameters.AddWithValue("@active", 1);
                    cmd.Parameters.AddWithValue("@userid", id);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                
            }
            catch (Exception e)
            {
                ret = "Try after sometime...";
            }
            return ret;
        }
    }
}
