using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using RdcMan;

namespace RdcPlgTest
{
    [Export(typeof(IPlugin))]
    public class LoggerPlugin : IPlugin
    {
        private const string XmlElementName = "RdcManConnectionLogger";
        private IPluginContext _context;
        private int _iconIdx = -1;

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
            _context = context;

            SetupIcons();

            Server.ConnectionStateChanged += ServerOnConnectionStateChanged;

            await CheckServerAvailableOrPrompt(context);

            Poller.StartPoller();
            Poller.ServerStateChanged += PollerOnServerStateChanged;
        }

        private void SetupIcons()
        {
            var treeView = _context.Tree as TreeView;

            if (treeView == null)
                return;

            _iconIdx = treeView.ImageList.Images.Count;
            treeView.ImageList.Images.Add(SystemIcons.Warning);
        }

        private async Task CheckServerAvailableOrPrompt(IPluginContext context)
        {
            var form = ((Form)context.MainForm);
            for (;;)
            {
                var avail = await LoggerClient.GetAvailable();

                if (avail)
                {
                    return;
                }


                if (!PromptServerConnection(form))
                {
                    break;
                }
            }
        }

        private bool PromptServerConnection(Form form)
        {
            var cancel = false;

            form.InvokeIfRequired(() =>
            {
                var prompt = new LoggerServerEntry { Value = _configuredLogger };

                var result = prompt.ShowDialog();

                if (result == DialogResult.Cancel)
                {
                    cancel = true;
                }
                else
                {
                    _configuredLogger = prompt.Value;
                    LoggerClient.Initialize(_configuredLogger);
                }
            });
        
            return !cancel;
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
        
        private void PollerOnServerStateChanged(object sender, ServerState e)
        {
            if (_iconIdx < 0)
            {
                return;
            }
            
            void Traverse(TreeNode node)
            {
                if (node is ServerBase server &&
                    string.Equals(server.ServerName, e.RemoteAddress, StringComparison.OrdinalIgnoreCase))
                {
                    if (e.ConnectedUser != null) // for testing on 1 machine
                    // if (e.ConnectedUser != null && e.ConnectedUser != Environment.UserName)
                    {
                        server.TreeView.InvokeIfRequired(() =>
                        {
                            server.ImageIndex = _iconIdx;
                            server.SelectedImageIndex = _iconIdx;
                            server.StateImageIndex = _iconIdx;
                        });
                    }
                }

                foreach (var child in node.Nodes.OfType<TreeNode>())
                {
                    Traverse(child);
                }
            }

            // _context.Tree.RootNode is garbage. Use the TreeView instead.
            if (_context.Tree is TreeView treeView)
            {
                foreach (var node in treeView.Nodes.OfType<TreeNode>())
                {
                    Traverse(node);
                }
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
            ToolStripMenuItem InactiveItem(string text)
            {
                return new ToolStripMenuItem(text) { Enabled = false };
            }

            if (node is ServerBase server)
            {
                var log = new ToolStripMenuItem();
                log.Text = "Activity log...";

                var any = false;
                
                var state = Poller.GetServerState(server.ServerName);
                foreach (var activity in state.Activity.OrderByDescending(x => x.Date))
                {
                    log.DropDownItems.Add(InactiveItem($"{activity.UserName} {activity.Action} @ {activity.Date:yyyy-MM-dd HH:mm:ss}"));
                    any = true;
                }
                if (!any)
                {
                    log.DropDownItems.Add(InactiveItem("(none)"));
                }

                contextMenuStrip.Items.Add(log);
            }
        }
    }
}