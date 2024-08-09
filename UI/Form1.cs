using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace UI
{
    public partial class Form1 : Form
    {
        private Process consoleProcess;
        private DockerClient dockerClient;

        public Form1()
        {
            InitializeComponent();
            dockerClient = new DockerClientConfiguration(new Uri("tcp://127.0.0.1:23750")).CreateClient();
        }

        private async void BtnStartConsoleApp_Click(object sender, EventArgs e)
        {
            // Start RabbitMQ container manually
            var rabbitContainerId = await StartRabbitMQContainerAsync();

            // Start Consumer container
            var consumerContainerId = await StartConsumerContainerAsync();

            // Output Consumer container logs
            await OutputContainerLogsAsync(consumerContainerId);
        }
        private async Task<string> StartRabbitMQContainerAsync()
        {
            // RabbitMQ container configuration
            var rabbitContainerBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("rabbitmq:3.8.9")
                .WithPortBinding(5672)
                .WithPortBinding(15672);

            // Start RabbitMQ container
            var rabbitContainer = rabbitContainerBuilder.Build();
            await rabbitContainer.StartAsync();

            return rabbitContainer.Id;
        }

        private async Task<string> StartConsumerContainerAsync()
        {
            // Define Consumer container configuration
            var consumerDockerfilePath = Path.Combine("..", "Consumer", "Dockerfile");
            var consumerContainerBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(consumerDockerfilePath)
                .WithPortBinding(8089) // Adjust port binding for the Consumer
                .WithEntrypoint("/bin/bash")
                .WithCommand("-c", "dotnet Consumer.dll"); // Adjust the command to start the Consumer

            // Start Consumer container
            var consumerContainer = consumerContainerBuilder.Build();
            await consumerContainer.StartAsync();

            return consumerContainer.Id;
        }

        [Obsolete]
        private async Task OutputContainerLogsAsync(string containerId)
        {
            var logs = await dockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = true
            });

            using (var reader = new StreamReader(logs))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    AppendLog(line);
                }
            }
        }

        private void AppendLog(string log)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => txtLogs.AppendText(log + Environment.NewLine)));
            }
            else
            {
                txtLogs.AppendText(log + Environment.NewLine);
            }
        }
    }
}
