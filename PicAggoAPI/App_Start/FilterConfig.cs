﻿using PicAggoAPI.Models;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PicAggoAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

   
}
