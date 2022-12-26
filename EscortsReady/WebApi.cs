namespace EscortsReady
{
    internal class WebApi
    {
        private WebApplicationBuilder builder;
        private WebApplication app;        

        public WebApi(params string[] args)
        {
            Initialize(args);
            var t = new Thread(new ThreadStart(async () => await new DiscordService().ExecuteAsync(Program.cancelToken)));
            t.Start();
            app.Run();
            Program.cancelToken = new CancellationToken(true);
        }

        private void Initialize(params string[] args)
        {
            builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            Program.logger = app.Logger;
            Program.config = app.Configuration;
        }
    }
}