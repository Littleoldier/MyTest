using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserDataMessage
{
    class UserMessage
    {
        //Name
        public string Name { get; set; }

        //Password
        public string Password { get; set; }
        
        //UserType
        public string UserType { get; set; }


        //UserDes
        public string UserDes { get; set; }

        public void Clear()
        {
            Name = "";
            
            Password = "";
            
            UserType = "";
            
            UserDes = "";
        }
    }
}
