using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAccount.Pri_Dal;
using UserDataMessage;


namespace UserAccount.Pri_Bll
{
    class LUserAccountBLL
    {
        LUserAccountDAL luad = new LUserAccountDAL();
        
        public void refeshConBLL()
        {
            luad.refreshCon();
        }

        //查询用户/密码
        public int CheckUserNamePassword(string UserName, string Password)
        {
            return luad.CheckUserNamePassword( UserName,  Password);
        }

        public UserMessage GetUserType(string UserName, string Password)
        {
            return luad.GetUserType( UserName, Password);
        }

    }
}
