﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using Common;
using System.Transactions;

namespace BLL
{
    public class UserBLL
    {
        IDAL.IUserDAL iUser = DALFactory.DataAccess.CreateUser();
        IDAL.IUserDataDAL iUserData = DALFactory.DataAccess.CreateUserData();
        //IDAL.IUserDAL iUser = new SQlDAL.UserDALImp();

        private static UserBLL instance;

        private UserBLL()
        {
        }

        public static UserBLL GetUserBLL()
        {
            if (instance == null)
            {
                instance = new UserBLL();
            }
            return instance;
        }

        public bool Login(string userName, string userPassword, out string msg)
        {
            msg = "";
            if (userName == "" || userPassword == "")
            {
                msg = "用户名或密码不能为空！";
                return false;
            }
            bool ok = false;
            try
            {
                ok = iUser.Login(userName, MD5Provider.Hash(userPassword));
            }
            catch (Exception exp)
            {
                msg = exp.Message;
            }
            return ok;
        }

        public int GetIdByName(string userName)
        {
            return iUser.FindIdByName(userName);
        }

        public bool Register(User user, UserData data, out string msg)
        {
            msg = "";
            bool isok = false;
            if (!CheckUser(user, out msg))
            {
                return isok;
            }

            using (TransactionScope tsCope = new TransactionScope())
            {
                try
                {
                    user.Password = MD5Provider.Hash(user.Password);
                    int id = iUser.AddUserAndRetId(user);
                    msg = Convert.ToString(id);
                    if (id != -1)
                    {
                        data.Uid = id;
                        if (iUserData.addUserData(data))
                        {
                            isok = true;
                        }
                    }
                }
                catch (Exception exp)
                {
                    msg = exp.Message;
                    return false;
                }

                tsCope.Complete();

            }
            return isok;
        }

        //先核对用户信息才允许改变
        public bool FindPassWord(UserData data, string passw, out string msg)
        {
            msg = "";
            if ("".Equals(data.Idcard))
            {
                msg = "未输入身份证号码！";
            }
            else if ("".Equals(data.Username))
            {
                msg = "未输入用户名！";
            }
            try
            {
                if (data.Idcard.Equals(iUserData.findIdCardByName(data.Username)))
                {
                    User user = new User();
                    user.UserName = data.Username;
                    user.Password = MD5Provider.Hash(passw);
                    if (iUser.Update(user))
                    {
                        msg = "更新成功！！";
                        return true;
                    }
                }
                else
                {
                    msg = "用户名与身份证不匹配！";
                }
            }
            catch (Exception exp)
            {
                msg = exp.Message;
            }
            return false;
        }

        public bool CheckUser(Model.User user, out string msg)
        {
            msg = "";
            if (user == null)
            {
                msg = "未填入用户";
                return false;
            }
            else if ("".Equals(user.UserName) || user.UserName == null)
            {
                msg = "用户为空";
                return false;
            }
            else if ("".Equals(user.Password) || user.Password == null)
            {
                msg = "密码为空";
                return false;
            }
            return true;
        }

        public UserData GetUserData(int uid)
        {
            return iUserData.GetDataById(uid);
        }
    }
}
