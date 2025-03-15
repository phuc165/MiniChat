using System;
using System.Windows.Forms;

namespace Client
{
    public class ClientForm : Form
    {
        private TextBox inputTextBox;
        private TextBox displayTextBox;
        private Button sendButton;
        private ChatClient chatClient = new ChatClient();

        public ClientForm()
        {
            // UI setup
            Text = "Chat Client";
            Width = 400;
            Height = 300;

            displayTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            inputTextBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 25
            };

            sendButton = new Button
            {
                Text = "Send",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            sendButton.Click += async (s, e) => await SendMessage();

            Controls.Add(displayTextBox);
            Controls.Add(inputTextBox);
            Controls.Add(sendButton);

            // Connect and start receiving when the form loads
            Load += async (s, e) =>
            {
                try
                {
                    await chatClient.Connect("127.0.0.1", 12345);
                    StartReceiving();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to connect: {ex.Message}");
                    Close();
                }
            };

            FormClosed += (s, e) => chatClient.Disconnect();
        }

        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                try
                {
                    await chatClient.SendMessage(inputTextBox.Text);
                    displayTextBox.AppendText($"You: {inputTextBox.Text}{Environment.NewLine}");
                    inputTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending message: {ex.Message}");
                }
            }
        }

        private async void StartReceiving()
        {
            while (true)
            {
                try
                {
                    string message = await chatClient.ReceiveMessage();
                    if (displayTextBox.InvokeRequired)
                    {
                        displayTextBox.Invoke(new Action(() =>
                            displayTextBox.AppendText($"Other: {message}{Environment.NewLine}")));
                    }
                    else
                    {
                        displayTextBox.AppendText($"Other: {message}{Environment.NewLine}");
                    }
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => MessageBox.Show($"Disconnected: {ex.Message}")));
                    }
                    break;
                }
            }
        }
    }
}
