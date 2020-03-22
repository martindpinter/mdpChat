using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mdpChat.Server.EntityFrameworkCore;
using mdpChat.Server.EntityFrameworkCore.Repositories;
using mdpChat.Server.EntityFrameworkCore.Interfaces;

namespace mdpChat.Server
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<ChatDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("ChatDBConnection")));

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddRazorPages();
            services.AddSignalR();

            services.AddScoped<IMessageRepository, SqlMessageRepository>();
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IGroupRepository, SqlGroupRepository>();
            services.AddScoped<IMembershipRepository, SqlMembershipRepository>();
            services.AddScoped<IClientRepository, SqlClientRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMvc();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/ChatHub");
                // endpoints.MapGet("/", async context =>
                // {
                //     await context.Response.WriteAsync("Hello World!");
                // });
            });
        }
    }
}
