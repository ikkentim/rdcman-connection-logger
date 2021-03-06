using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Xml;
using RdcMan;

namespace RdcPlgTest
{
    [Export(typeof(IPlugin))]
    public class LoggerPlugin : IPlugin
    {
        private const string XmlElementName = "RdcManConnectionLogger";

        public LoggerPlugin()
        {
            // Fix settings loading
            RdcManFix.ApplyFix();
        }

        private string _configuredLogger = "http://localhost:5001/";
        public void PreLoad(IPluginContext context, XmlNode xmlNode)
        {
            if (xmlNode?.FirstChild is XmlElement el)
            {
                var att = el.GetAttribute("server");

                if (!string.IsNullOrEmpty(att))
                {
                    _configuredLogger = att;
                }
            }

            LoggerClient.Initialize(_configuredLogger);
        }
        
        public async void PostLoad(IPluginContext context)
        {
            Server.ConnectionStateChanged += ServerOnConnectionStateChanged;

            for(;;)
            {
                var avail = await LoggerClient.GetAvailable();

                if(avail)
                {
                    return;
                }

                var cancel = false;
                var form = ((Form)context.MainForm);

                var action = new Action(() =>{
                    var prompt = new LoggerServerEntry
                    {
                        Value = _configuredLogger
                    };

                    var result = prompt.ShowDialog();

                    if(result == DialogResult.Cancel)
                    {
                        cancel = true;
                    }
                    else
                    {
                        _configuredLogger = prompt.Value;
                        LoggerClient.Initialize(_configuredLogger);
                    }
                });

                if(form.InvokeRequired)
                {
                    form.Invoke(action);
                }
                else
                {
                    action();
                }
                
                if (cancel)
                {
                    return;
                }
            }
        }

        private static string GetNodeName(TreeNode node)
        {
            switch (node)
            {
                case ServerBase server:
                    return $"{GetNodeName(server.Parent)}{server.DisplayName}";
                case Group group:
                    return  $"{GetNodeName(group.Parent)}{group.Text}/";
                default:
                    return string.Empty;
            }
        }

        private void ServerOnConnectionStateChanged(ConnectionStateChangedEventArgs obj)
        {
            var entry = new LoggerEntry
            {
                UserName = Environment.UserName,
                Action = obj.State.ToString(),
                RemoteName = GetNodeName(obj.Server),
                RemoteAddress = obj.Server.ServerName
            };
            
            LoggerClient.Send(entry);
        }

        public XmlNode SaveSettings()
        {
            var doc = new XmlDocument();
            var el = doc.CreateElement(XmlElementName);
            el.SetAttribute("server", _configuredLogger);
            return el;
        }

        public void Shutdown()
        {
            var entry = new LoggerEntry
            {
                UserName = Environment.UserName,
                Action = "Shutdown"
            };
            
            LoggerClient.Send(entry);
        }

        public void OnUndockServer(IUndockedServerForm form)
        {
        }

        public void OnDockServer(ServerBase server)
        {
        }

        public void OnContextMenu(ContextMenuStrip contextMenuStrip, RdcTreeNode node)
        {
        }
    }
}