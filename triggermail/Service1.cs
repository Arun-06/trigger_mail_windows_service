using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Security.Policy;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace triggermail
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = Convert.ToDouble(GetConfig("TimeInterval"));
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
        }
        private void OnElapsedTime(object obj, ElapsedEventArgs e)
        {
            SendActivationMail();
        }
        public void SendActivationMail()
        {
            DataSet ds_users = new DataSet();
            SmtpClient smtpClient = new SmtpClient(GetConfig("host"), Convert.ToInt32(GetConfig("port"))) { 
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(GetConfig("mailUser"),GetConfig("mailPass")),
            EnableSsl = true
            };
            

            try
            {
                ds_users = GetNewUsers();
                if (ds_users != null && ds_users.Tables.Count > 0 && ds_users.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds_users.Tables[0].Rows)
                    {
                        if (dr["user_mail"] != null && !string.IsNullOrEmpty(dr["user_mail"].ToString()) && dr["user_id"] != null && !string.IsNullOrEmpty(dr["user_id"].ToString()) && dr["user_name"] != null && !string.IsNullOrEmpty(dr["user_name"].ToString()))
                        {
                            var body = new StringBuilder();
                            body.AppendFormat("Hello, {0}\n", dr["user_name"].ToString());
                            body.AppendLine(@"Click Activate link to active you account");
                            body.AppendLine("<a href=\""+GetConfig("apiurl")+ dr["user_id"].ToString()+"\">Activate</a>");
                            MailMessage msg = new MailMessage(GetConfig("mailUser"), dr["user_mail"].ToString())
                            {
                                Subject = "Account Activation",
                                IsBodyHtml = true,
                                Body = body.ToString()
                            };
                            smtpClient.Send(msg);
                            updateStatus(dr["user_id"].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //error handling code
            }
        }
        private DataSet GetNewUsers()
        {
            DataSet ds_users = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["accountactivation"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from usertable where isactive = @active and mailtriggerd_on is null or mailtriggerd_on > DATEADD(DAY,-3,GETDATE());", conn);
                    cmd.Parameters.AddWithValue("@active",0);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(ds_users);
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                //error handling code
                
            }
            return ds_users;
        }
        private  void updateStatus(string userid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["accountactivation"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("update usertable set mailtriggerd_on = @mailtri where user_id = @userid;", conn);
                    SqlParameter spm = new SqlParameter("@mailtri", SqlDbType.DateTime);
                    spm.Value = DateTime.UtcNow;
                    cmd.Parameters.Add(spm);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                //error handling code

            }
        }
        private string GetConfig(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();
        }
    }
}
