using Coven;

using Spectre.Console;

if (args.Length == 0)
{
    System.Console.WriteLine("A file's path is required");
    return 1;
}

var path = Path.Combine(Directory.GetCurrentDirectory(), args[0]);
if (!File.Exists(path))
{
    System.Console.WriteLine($"File {path} does not exists");
    return 1;
}

await AnsiConsole.Live(new Markup("[blue] Starting up... [/]\n"))
    .StartAsync(async ctx =>
    {
        ctx.Refresh();

        using var app = await Application.CreateAsync(path);
        ctx.UpdateTarget(app);

        do
        {
            ctx.Refresh();
        }
        while (await app.CaptureInput());
    });

return 0;