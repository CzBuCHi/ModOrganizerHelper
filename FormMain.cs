﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModOrganizerHelper.Properties;

namespace ModOrganizerHelper
{
    public partial class FormMain : Form
    {
        private readonly List<string> _log = new List<string>();
        private Worker _worker;

        public FormMain() {
            InitializeComponent();
            Height = 210;
            CreateWorker();
        }

        private void CreateWorker() {
            string iniPath = Path.Combine(Settings.Default.IniPath, "ModOrganizer.ini");
            if (!File.Exists(iniPath)) {
                return;
            }

            _worker = new Worker(iniPath);
            labelProfile.Text = _worker.ProfileName;

            Action updateLog = Debounce(() => {
                Invoke(new Action(() => {
                    textBoxLog.Text = string.Join(Environment.NewLine, _log);
                    textBoxLog.SelectionStart = textBoxLog.Text.Length;
                    textBoxLog.ScrollToCaret();
                    Application.DoEvents();
                }));
            }, 16);

            _worker.Log += (message, replace) => {
                if (replace) {
                    _log.RemoveAt(_log.Count - 1);
                }
                _log.Add(message);
                updateLog();
            };

            buttonUpdate.Enabled = true;
            buttonDelete.Enabled = true;
        }

        private static Action Debounce(Action func, int milliseconds = 300) {
            int last = 0;
            return () => {
                int current = Interlocked.Increment(ref last);
                Task.Delay(milliseconds).ContinueWith(task => {
                    if (current == last) {
                        func();
                    }
                    task.Dispose();
                });
            };
        }

        private void buttonDelete_Click(object sender, EventArgs e) {
            Height = 415;
            _log.Clear();

            EnableButtons(false);
            Task.Run(() => _worker.DeleteAllLinks()).ContinueWith(o => Invoke(new Action(() => { EnableButtons(true); })));
        }

        private void buttonUpdate_Click(object sender, EventArgs e) {
            Height = 415;
            _log.Clear();
            EnableButtons(false);
            Task.Run(() => _worker.UpdateLinks()).ContinueWith(o => Invoke(new Action(() => { EnableButtons(true); })));
        }

        private void EnableButtons(bool enable) {
            buttonDelete.Enabled = enable;
            buttonUpdate.Enabled = enable;
        }

        private void buttonFindIni_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                Filter = @"ModOrganizer.ini|ModOrganizer.ini",
                InitialDirectory = Settings.Default.IniPath
            };

            if (dialog.ShowDialog(this) == DialogResult.OK) {
                Settings.Default.IniPath = Path.GetDirectoryName(dialog.FileName);
                Settings.Default.Save();
                CreateWorker();
            }
        }

        private void buttonSelectSavePath_Click(object sender, EventArgs e) {
            FolderBrowserDialog dialog = new FolderBrowserDialog {
                SelectedPath = Settings.Default.SavesPath
            };

            if (dialog.ShowDialog(this) == DialogResult.OK) {
                Settings.Default.SavesPath = dialog.SelectedPath;
                Settings.Default.Save();
            }
        }

        private void labelIniPath_DoubleClick(object sender, EventArgs e) {
            Settings.Default.IniPath = Clipboard.GetText();
            Settings.Default.Save();
        }

        private void labelSavesDirectoryPath_DoubleClick(object sender, EventArgs e) {
            Settings.Default.SavesPath = Clipboard.GetText();
            Settings.Default.Save();
        }
    }
}
