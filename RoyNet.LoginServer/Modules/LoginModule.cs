﻿using System.Linq;
using Dapper;
using Nancy;

namespace RoyNet.LoginServer
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            //call in browse  http://127.0.0.1:8080/login/u1/p1
            Get["/login/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    string uid = conn.Query<string>("select uid from Account where username=@username and password=@password", new
                    {
                        username = p.uname,
                        password = p.pwd
                    }).FirstOrDefault();
                    if (uid != null)
                    {
                        string token = TokenManager.CreateToken(uid);
                        return Response.AsJson(new { Result = "OK", Token = token, ServerList = Program.Config.GameServers });
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };

            //in table account, there are username, password, uid (should be auto increment)

            //http://127.0.0.1:8080/reg/u6/p6
            Get["/reg/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    var account = new
                    {
                        username = p.uname,
                        password = p.pwd
                    };
                    int ret = conn.Execute("insert into Account(`username`,`password`) values(@username,@password)", account);
                    if (ret == 1)
                    {
                        string uid = conn.Query<string>("select uid from Account where username=@username", account).First();
                        string token = TokenManager.CreateToken(uid);
                        return Response.AsJson(new { Result = "OK", Token = token, ServerList = Program.Config.GameServers });
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };

            Get["/chk/{token}"] = p =>
            {
                string uid;
                if (TokenManager.Check(p.token, out uid))
                {
                    return Response.AsJson(new { Result = "OK", UID = uid });
                }
                else
                {
                    return Response.AsJson(new { Result = "Failed" });
                }
            };
        }
    }
}
