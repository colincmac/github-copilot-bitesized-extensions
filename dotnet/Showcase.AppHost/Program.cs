using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using Showcase.AppHost.OpenTelemetryCollector;

var builder = DistributedApplication.CreateBuilder(args);

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
       .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
       .WithHttpEndpoint(targetPort: 9090, name: "http");

var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
                     .WithHttpEndpoint(targetPort: 3000, name: "http");

//var cache = builder.AddRedis("cache");

builder.AddOpenTelemetryCollector("otelcollector", "../otelcollector/config.yaml")
       .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp");


var openai = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
    : builder.AddConnectionString("openai");

#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//var pythonPlugins = builder.AddPythonApp(
//    name: "python-plugins",
//    projectDirectory: Path.Combine("..", "Python.Plugins"),
//    scriptPath: "-m",
//    virtualEnvironmentPath: "env",
//    scriptArgs: ["uvicorn", "main:app"])
//       .WithEndpoint(targetPort: 62394, scheme: "http", env: "UVICORN_PORT");

//if (builder.ExecutionContext.IsRunMode && builder.Environment.IsDevelopment())
//{
//    pythonPlugins.WithEnvironment("DEBUG", "True");
//}

#pragma warning restore ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var gitHubAgent = builder.AddProject<Projects.Showcase_GitHubCopilot_Agent>("GitHubAgent")
    .WithReference(openai)
    //.WithReference(cache)
    .WithEnvironment("OPENAI_EXPERIMENTAL_ENABLE_OPEN_TELEMETRY", "true");

builder.Build().Run();
