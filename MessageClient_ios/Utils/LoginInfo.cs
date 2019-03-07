//-----------------------------------------------------------------------
// <copyright file="LoginInfo.cs" company="Wistron">
//     Copyright (c) Wistron Technology Co., Ltd. All rights reserved.
// </copyright>
// <author>劉中棟</author>
// <date>2016-10-15</date>
// <summary>登入用戶Model</summary>
//-----------------------------------------------------------------------
using System;
using System.Data;

namespace MessageClient_ios.Util
{
    /// <summary>
    /// Year type.
    /// </summary>
    public enum YearType
    {
        CommonYear,
        TraditionalYear
    }

    /// <summary>
    /// 登入用戶Model
    /// </summary>
    [Serializable]
    public class LoginInfo
    {
        private string _loadfinish = "false";
        private string _userid;
        private string _loginuserid;
        private string _username;
        private string _passWord;
        private string _groupcode;
        private string _groupname;
        private string _agentcode;
        private bool _isPublicAccount;
        private string _netplay;               //上否上網
        private string _winner;                //上否有贏家秘笈權限，Y:有；N:沒有
        private string _agenttype;           //身份類別
        private DataTable _tbroleright;
        //private LoginUserInfo _frontRole;
        private string _isloginout = "N";    //是否登出        
        /// <summary>
        /// 登錄帳號
        /// </summary>
        public string UserId
        {
            get { return _userid; }
            set { _userid = value; }
        }

        /// <summary>
        /// 用戶登錄所使用的賬號，可以是郵件 可以是賬號ID
        /// </summary>
        public string LoginUserId
        {
            get { return _loginuserid; }
            set { _loginuserid = value; }
        }

        /// <summary>
        /// 線程加載完
        /// </summary>
        public string loadfinish
        {
            get { return _loadfinish; }
            set { _loadfinish = value; }
        }

        /// <summary>
        /// 用護名
        /// </summary>
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// 登錄密碼
        /// </summary>
        public string PassWord
        {
            get { return _passWord; }
            set { _passWord = value; }
        }

        /// <summary>
        /// 通路群
        /// </summary>
        public string GroupCode
        {
            get { return _groupcode; }
            set { _groupcode = value; }
        }

        /// <summary>
        /// 通路名稱
        /// </summary>
        public string GroupName
        {
            get { return _groupname; }
            set { _groupname = value; }
        }

        /// <summary>
        /// 前端-通路商代碼。
        /// </summary>
        public string AgentCode
        {
            get { return _agentcode; }
            set { _agentcode = value; }
        }
        /// <summary>
        /// 前端-是否為公號。
        /// </summary>
        public bool IsPublicAccount
        {
            get { return this._isPublicAccount; }
            set { this._isPublicAccount = value; }
        }

        /// <summary>
        /// 是否上網
        /// </summary>
        public string NetPlay
        {
            get { return _netplay; }
            set { _netplay = value; }
        }

        /// <summary>
        /// 是否上網
        /// </summary>
        public string IsLoginOut
        {
            get { return _isloginout; }
            set { _isloginout = value; }
        }

        /// <summary>
        /// 是否有贏家秘笈權限
        /// </summary>
        public string Winner
        {
            get { return _winner; }
            set { _winner = value; }
        }

        /// <summary>
        /// 身份類別
        /// </summary>
        public string AgentType
        {
            get { return _agenttype; }
            set { _agenttype = value; }
        }

        public DataTable TbRoleRight
        {
            get { return _tbroleright; }
            set { _tbroleright = value; }
        }

        /// <summary>
        /// 前端頁面之登陸信息
        /// </summary>
        //public LoginUserInfo TbFrontRole
        //{
        //    get { return this._frontRole; }
        //    set { this._frontRole = value; }
        //}

        //西元 or 民国
        private YearType _yearType = YearType.TraditionalYear;
        public YearType PYearType
        {
            get
            {
                return _yearType;
            }
            set
            {
                _yearType = value;
            }
        }

        private static LoginInfo _CurrentUser = null;

        //應用單件模式，保存用戶登錄狀態  
        public static LoginInfo CurrentUser
        {
            get
            {
                if (_CurrentUser == null)
                    _CurrentUser = new LoginInfo();

                return _CurrentUser;
            }
        }

    }
}
