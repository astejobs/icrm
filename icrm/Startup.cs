﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(icrm.Startup))]
namespace icrm
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
