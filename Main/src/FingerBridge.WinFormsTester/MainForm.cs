using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FingerBridge;

namespace FingerBridge.WinFormsTester
{
    public sealed class MainForm : Form
    {
        private ComboBox _providerComboBox;
        private Label _providerHintLabel;
        private Label _statusLabel;
        private PictureBox _fingerprintPictureBox;
        private TextBox _userTextBox;
        private TextBox _templatePathTextBox;
        private TextBox _base64TextBox;
        private TextBox _logTextBox;
        private Label _qualityLabel;
        private Label _templateSizeLabel;
        private CheckBox _fakeDetectionCheckBox;
        private CheckBox _lfdCheckBox;
        private CheckBox _miotCheckBox;
        private CheckBox _abortFakeCheckBox;
        private NumericUpDown _farnNumericUpDown;
        private NumericUpDown _maxModelsNumericUpDown;
        private Button _enrollButton;
        private Button _verifyButton;
        private Button _saveButton;
        private Button _loadButton;
        private Button _cancelButton;
        private Button _clearButton;
        private Button _copyBase64Button;
        private Button _refreshProvidersButton;

        private IFingerprintService _fingerprintService;
        private CancellationTokenSource _operationCancellation;
        private byte[] _currentTemplate;
        private int _operationSequence;

        public MainForm()
        {
            InitializeComponent();
            LoadProviders();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DisposeCurrentService();
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            Text = "FingerBridge - WinForms Tester";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1040, 720);
            Size = new Size(1180, 780);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            var root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Padding = new Padding(10);
            Controls.Add(root);

            var leftPanel = CreateLeftPanel();
            var rightPanel = CreateRightPanel();

            root.Controls.Add(leftPanel, 0, 0);
            root.Controls.Add(rightPanel, 1, 0);
        }

        private Control CreateLeftPanel()
        {
            var panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 11;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var title = new Label();
            title.Text = "Teste de biometria";
            title.AutoSize = true;
            title.Font = new Font(Font, FontStyle.Bold);
            title.Margin = new Padding(0, 0, 0, 8);
            panel.Controls.Add(title, 0, 0);

            var providerPanel = new TableLayoutPanel();
            providerPanel.Dock = DockStyle.Top;
            providerPanel.ColumnCount = 2;
            providerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            providerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            providerPanel.RowCount = 3;
            providerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            providerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            providerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            providerPanel.Margin = new Padding(0, 0, 0, 10);

            var providerLabel = new Label();
            providerLabel.Text = "Provider";
            providerLabel.AutoSize = true;
            providerPanel.Controls.Add(providerLabel, 0, 0);

            _providerComboBox = new ComboBox();
            _providerComboBox.Dock = DockStyle.Top;
            _providerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _providerComboBox.SelectedIndexChanged += ProviderComboBox_SelectedIndexChanged;
            providerPanel.Controls.Add(_providerComboBox, 0, 1);

            _refreshProvidersButton = new Button();
            _refreshProvidersButton.Text = "Atualizar";
            _refreshProvidersButton.AutoSize = true;
            _refreshProvidersButton.Click += RefreshProvidersButton_Click;
            providerPanel.Controls.Add(_refreshProvidersButton, 1, 1);

            _providerHintLabel = new Label();
            _providerHintLabel.AutoSize = false;
            _providerHintLabel.Dock = DockStyle.Top;
            _providerHintLabel.Height = 45;
            _providerHintLabel.ForeColor = SystemColors.GrayText;
            providerPanel.SetColumnSpan(_providerHintLabel, 2);
            providerPanel.Controls.Add(_providerHintLabel, 0, 2);
            panel.Controls.Add(providerPanel, 0, 1);

            var userLabel = new Label();
            userLabel.Text = "Usuario / referencia para teste";
            userLabel.AutoSize = true;
            panel.Controls.Add(userLabel, 0, 2);

            _userTextBox = new TextBox();
            _userTextBox.Dock = DockStyle.Top;
            _userTextBox.Text = "usuario.teste";
            _userTextBox.Margin = new Padding(0, 0, 0, 10);
            panel.Controls.Add(_userTextBox, 0, 3);

            var optionsGroup = CreateOptionsGroup();
            optionsGroup.Margin = new Padding(0, 0, 0, 10);
            panel.Controls.Add(optionsGroup, 0, 4);

            var actionsGroup = CreateActionsGroup();
            actionsGroup.Margin = new Padding(0, 0, 0, 10);
            panel.Controls.Add(actionsGroup, 0, 5);

            var templateLabel = new Label();
            templateLabel.Text = "Arquivo de template para teste";
            templateLabel.AutoSize = true;
            panel.Controls.Add(templateLabel, 0, 6);

            _templatePathTextBox = new TextBox();
            _templatePathTextBox.Dock = DockStyle.Top;
            _templatePathTextBox.ReadOnly = true;
            _templatePathTextBox.Margin = new Padding(0, 0, 0, 8);
            panel.Controls.Add(_templatePathTextBox, 0, 7);

            var infoGroup = CreateInfoGroup();
            panel.Controls.Add(infoGroup, 0, 8);

            var help = new Label();
            help.Text = "Fluxo recomendado:\r\n1. Cadastrar digital\r\n2. Salvar template ou copiar Base64\r\n3. Carregar template\r\n4. Validar digital\r\n\r\nNo banco, armazene o template como VARBINARY(MAX) associado ao usuario. Evite salvar a imagem da digital.";
            help.Dock = DockStyle.Fill;
            help.ForeColor = SystemColors.GrayText;
            help.Margin = new Padding(0, 10, 0, 0);
            panel.Controls.Add(help, 0, 9);

            return panel;
        }

        private GroupBox CreateOptionsGroup()
        {
            var group = new GroupBox();
            group.Text = "Opcoes do provider";
            group.Dock = DockStyle.Top;
            group.Height = 150;

            var table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = 4;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.Padding = new Padding(8);
            group.Controls.Add(table);

            _fakeDetectionCheckBox = new CheckBox();
            _fakeDetectionCheckBox.Text = "FakeDetection";
            _fakeDetectionCheckBox.Checked = true;
            _fakeDetectionCheckBox.AutoSize = true;
            table.Controls.Add(_fakeDetectionCheckBox, 0, 0);

            _lfdCheckBox = new CheckBox();
            _lfdCheckBox.Text = "LFD / FFD";
            _lfdCheckBox.Checked = true;
            _lfdCheckBox.AutoSize = true;
            table.Controls.Add(_lfdCheckBox, 1, 0);

            _miotCheckBox = new CheckBox();
            _miotCheckBox.Text = "MIOT";
            _miotCheckBox.Checked = true;
            _miotCheckBox.AutoSize = true;
            table.Controls.Add(_miotCheckBox, 0, 1);

            _abortFakeCheckBox = new CheckBox();
            _abortFakeCheckBox.Text = "Abortar se dedo falso";
            _abortFakeCheckBox.Checked = true;
            _abortFakeCheckBox.AutoSize = true;
            table.Controls.Add(_abortFakeCheckBox, 1, 1);

            var farnLabel = new Label();
            farnLabel.Text = "FARN";
            farnLabel.AutoSize = true;
            table.Controls.Add(farnLabel, 0, 2);

            _farnNumericUpDown = new NumericUpDown();
            _farnNumericUpDown.Minimum = 1;
            _farnNumericUpDown.Maximum = 1000;
            _farnNumericUpDown.Value = 166;
            _farnNumericUpDown.Dock = DockStyle.Top;
            table.Controls.Add(_farnNumericUpDown, 1, 2);

            var maxModelsLabel = new Label();
            maxModelsLabel.Text = "MaxModels cadastro";
            maxModelsLabel.AutoSize = true;
            table.Controls.Add(maxModelsLabel, 0, 3);

            _maxModelsNumericUpDown = new NumericUpDown();
            _maxModelsNumericUpDown.Minimum = 1;
            _maxModelsNumericUpDown.Maximum = 10;
            _maxModelsNumericUpDown.Value = 3;
            _maxModelsNumericUpDown.Dock = DockStyle.Top;
            table.Controls.Add(_maxModelsNumericUpDown, 1, 3);

            return group;
        }

        private GroupBox CreateActionsGroup()
        {
            var group = new GroupBox();
            group.Text = "Acoes";
            group.Dock = DockStyle.Top;
            group.Height = 132;

            var grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.RowCount = 3;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.Padding = new Padding(8);
            group.Controls.Add(grid);

            _enrollButton = CreateActionButton("Cadastrar digital", EnrollButton_Click);
            _verifyButton = CreateActionButton("Validar digital", VerifyButton_Click);
            _saveButton = CreateActionButton("Salvar template", SaveButton_Click);
            _loadButton = CreateActionButton("Carregar template", LoadButton_Click);
            _cancelButton = CreateActionButton("Cancelar", CancelButton_Click);
            _clearButton = CreateActionButton("Limpar", ClearButton_Click);

            grid.Controls.Add(_enrollButton, 0, 0);
            grid.Controls.Add(_verifyButton, 1, 0);
            grid.Controls.Add(_saveButton, 0, 1);
            grid.Controls.Add(_loadButton, 1, 1);
            grid.Controls.Add(_cancelButton, 0, 2);
            grid.Controls.Add(_clearButton, 1, 2);

            return group;
        }

        private Button CreateActionButton(string text, EventHandler clickHandler)
        {
            var button = new Button();
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(3);
            button.Click += clickHandler;
            return button;
        }

        private GroupBox CreateInfoGroup()
        {
            var group = new GroupBox();
            group.Text = "Resultado";
            group.Dock = DockStyle.Top;
            group.Height = 88;

            var table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 1;
            table.RowCount = 3;
            table.Padding = new Padding(8);
            group.Controls.Add(table);

            _qualityLabel = new Label();
            _qualityLabel.Text = "Qualidade: -";
            _qualityLabel.AutoSize = true;
            table.Controls.Add(_qualityLabel, 0, 0);

            _templateSizeLabel = new Label();
            _templateSizeLabel.Text = "Template: -";
            _templateSizeLabel.AutoSize = true;
            table.Controls.Add(_templateSizeLabel, 0, 1);

            _copyBase64Button = new Button();
            _copyBase64Button.Text = "Copiar Base64 para banco/log";
            _copyBase64Button.Dock = DockStyle.Top;
            _copyBase64Button.Click += CopyBase64Button_Click;
            table.Controls.Add(_copyBase64Button, 0, 2);

            return group;
        }

        private Control CreateRightPanel()
        {
            var panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 6;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            panel.Padding = new Padding(12, 0, 0, 0);

            _statusLabel = new Label();
            _statusLabel.Text = "Status: aguardando.";
            _statusLabel.AutoSize = true;
            _statusLabel.Font = new Font(Font, FontStyle.Bold);
            _statusLabel.Margin = new Padding(0, 0, 0, 8);
            panel.Controls.Add(_statusLabel, 0, 0);

            _fingerprintPictureBox = new PictureBox();
            _fingerprintPictureBox.Dock = DockStyle.Fill;
            _fingerprintPictureBox.BackColor = Color.White;
            _fingerprintPictureBox.BorderStyle = BorderStyle.FixedSingle;
            _fingerprintPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            panel.Controls.Add(_fingerprintPictureBox, 0, 1);

            var base64Label = new Label();
            base64Label.Text = "Template em Base64";
            base64Label.AutoSize = true;
            base64Label.Margin = new Padding(0, 10, 0, 4);
            panel.Controls.Add(base64Label, 0, 2);

            _base64TextBox = new TextBox();
            _base64TextBox.Multiline = true;
            _base64TextBox.ScrollBars = ScrollBars.Vertical;
            _base64TextBox.Dock = DockStyle.Fill;
            _base64TextBox.ReadOnly = true;
            panel.Controls.Add(_base64TextBox, 0, 3);

            var logLabel = new Label();
            logLabel.Text = "Log";
            logLabel.AutoSize = true;
            logLabel.Margin = new Padding(0, 10, 0, 4);
            panel.Controls.Add(logLabel, 0, 4);

            _logTextBox = new TextBox();
            _logTextBox.Multiline = true;
            _logTextBox.ScrollBars = ScrollBars.Vertical;
            _logTextBox.Dock = DockStyle.Fill;
            _logTextBox.ReadOnly = true;
            panel.Controls.Add(_logTextBox, 0, 5);

            return panel;
        }

        private void LoadProviders()
        {
            string selectedKey = null;
            var selected = _providerComboBox.SelectedItem as ProviderItem;
            if (selected != null && selected.Factory != null)
                selectedKey = selected.Factory.ProviderKey;

            _providerComboBox.SelectedIndexChanged -= ProviderComboBox_SelectedIndexChanged;
            _providerComboBox.Items.Clear();

            var providers = ProviderLoader.LoadProviders();
            foreach (var provider in providers)
                _providerComboBox.Items.Add(provider);

            int indexToSelect = 0;
            if (!string.IsNullOrEmpty(selectedKey))
            {
                for (int i = 0; i < _providerComboBox.Items.Count; i++)
                {
                    var item = _providerComboBox.Items[i] as ProviderItem;
                    if (item != null && item.Factory != null && item.Factory.ProviderKey == selectedKey)
                    {
                        indexToSelect = i;
                        break;
                    }
                }
            }

            if (_providerComboBox.Items.Count > 0)
                _providerComboBox.SelectedIndex = indexToSelect;

            _providerComboBox.SelectedIndexChanged += ProviderComboBox_SelectedIndexChanged;
            CreateServiceFromSelectedProvider();
        }

        private void CreateServiceFromSelectedProvider()
        {
            DisposeCurrentService();

            var selected = _providerComboBox.SelectedItem as ProviderItem;
            if (selected == null || selected.Factory == null || !selected.Available)
            {
                _providerHintLabel.Text = selected == null ? "Nenhum provider disponivel." : selected.Reason;
                SetOperationButtonsEnabled(false);
                return;
            }

            _providerHintLabel.Text = selected.Reason;
            _fingerprintService = selected.Factory.Create(CreateOptions(), SynchronizationContext.Current);
            _fingerprintService.StatusChanged += FingerprintService_StatusChanged;
            _fingerprintService.ImageCaptured += FingerprintService_ImageCaptured;
            SetOperationButtonsEnabled(true);
            Log("Provider selecionado: " + selected.DisplayName + " | Key: " + selected.Factory.ProviderKey + " | Template: " + selected.Factory.DefaultTemplateFormat);
        }

        private FingerprintServiceOptions CreateOptions()
        {
            var selected = _providerComboBox.SelectedItem as ProviderItem;
            return new FingerprintServiceOptions
            {
                ProviderKey = selected != null && selected.Factory != null ? selected.Factory.ProviderKey : BiometricProviderKeys.Mock,
                DeviceModel = selected != null && selected.Factory != null ? selected.Factory.DisplayName : string.Empty,
                TemplateFormat = selected != null && selected.Factory != null ? selected.Factory.DefaultTemplateFormat : FingerprintTemplateFormats.ProviderNative,
                DeviceId = "AUTO",
                FakeDetection = _fakeDetectionCheckBox.Checked,
                LfdControl = _lfdCheckBox.Checked,
                MiotControl = _miotCheckBox.Checked,
                AbortWhenFakeSourceDetected = _abortFakeCheckBox.Checked,
                Farn = Convert.ToInt32(_farnNumericUpDown.Value),
                MaxModels = Convert.ToInt32(_maxModelsNumericUpDown.Value)
            };
        }

        private async void EnrollButton_Click(object sender, EventArgs e)
        {
            if (_fingerprintService == null)
            {
                MessageBox.Show(this, "Nenhum provider disponivel.", "Biometria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RunOperationAsync("cadastro", async delegate(CancellationToken token)
            {
                var result = await _fingerprintService.EnrollAsync(token);
                if (result.Success)
                {
                    SetCurrentTemplate(result.Template, result.Quality);
                    Log("Cadastro concluido. Provider: " + result.ProviderKey + ". Formato: " + result.TemplateFormat + ". Qualidade: " + result.Quality + ". Template: " + result.Template.Length + " bytes.");
                    MessageBox.Show(this, "Digital cadastrada com sucesso. Agora salve o template ou copie o Base64 para testar persistencia.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log("Falha no cadastro. Codigo: " + result.ErrorCode + ". Mensagem: " + result.ErrorMessage);
                    MessageBox.Show(this, result.ErrorMessage, "Falha no cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
        }

        private async void VerifyButton_Click(object sender, EventArgs e)
        {
            if (_fingerprintService == null)
            {
                MessageBox.Show(this, "Nenhum provider disponivel.", "Biometria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentTemplate == null || _currentTemplate.Length == 0)
            {
                MessageBox.Show(this, "Cadastre ou carregue um template antes de validar.", "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RunOperationAsync("validacao", async delegate(CancellationToken token)
            {
                var result = await _fingerprintService.VerifyAsync(_currentTemplate, token);
                if (result.Success)
                {
                    if (result.Matched)
                    {
                        Log("Validacao concluida: DIGITAL CONFERE.");
                        MessageBox.Show(this, "Digital confere com o template carregado.", "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Log("Validacao concluida: digital nao confere.");
                        MessageBox.Show(this, "Digital nao confere com o template carregado.", "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    Log("Falha na validacao. Codigo: " + result.ErrorCode + ". Mensagem: " + result.ErrorMessage);
                    MessageBox.Show(this, result.ErrorMessage, "Falha na validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
        }

        private async Task RunOperationAsync(string operationName, Func<CancellationToken, Task> operation)
        {
            int operationId = ++_operationSequence;
            _operationCancellation = new CancellationTokenSource();
            SetBusy(true);
            Log("Iniciando " + operationName + ".");

            try
            {
                await operation(_operationCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                Log("Operacao cancelada: " + operationName + ".");
            }
            catch (Exception ex)
            {
                Log("Erro inesperado na operacao " + operationName + ": " + ex);
                MessageBox.Show(this, ex.Message, "Erro inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (operationId == _operationSequence)
                {
                    if (_operationCancellation != null)
                    {
                        _operationCancellation.Dispose();
                        _operationCancellation = null;
                    }

                    SetBusy(false);
                    _statusLabel.Text = "Status: aguardando.";
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_currentTemplate == null || _currentTemplate.Length == 0)
            {
                MessageBox.Show(this, "Nao existe template carregado para salvar.", "Salvar template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Salvar template biometrico";
                dialog.Filter = "Template biometrico (*.biotpl)|*.biotpl|Template Futronic (*.ftrtpl)|*.ftrtpl|Template Nitgen (*.nitfir)|*.nitfir|Binario (*.bin)|*.bin|Todos os arquivos (*.*)|*.*";
                dialog.FileName = SafeFileName(_userTextBox.Text) + ".biotpl";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllBytes(dialog.FileName, _currentTemplate);
                _templatePathTextBox.Text = dialog.FileName;
                Log("Template salvo em: " + dialog.FileName);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Carregar template biometrico";
                dialog.Filter = "Template biometrico (*.biotpl;*.ftrtpl;*.nitfir;*.bin)|*.biotpl;*.ftrtpl;*.nitfir;*.bin|Todos os arquivos (*.*)|*.*";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                var bytes = File.ReadAllBytes(dialog.FileName);
                SetCurrentTemplate(bytes, null);
                _templatePathTextBox.Text = dialog.FileName;
                Log("Template carregado de: " + dialog.FileName + ". Bytes: " + bytes.Length + ".");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (_operationCancellation != null)
                _operationCancellation.Cancel();

            if (_fingerprintService != null)
                _fingerprintService.Cancel();

            Log("Cancelamento solicitado.");
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            _currentTemplate = null;
            _base64TextBox.Clear();
            _templatePathTextBox.Clear();
            _qualityLabel.Text = "Qualidade: -";
            _templateSizeLabel.Text = "Template: -";

            if (_fingerprintPictureBox.Image != null)
            {
                var old = _fingerprintPictureBox.Image;
                _fingerprintPictureBox.Image = null;
                old.Dispose();
            }

            Log("Tela limpa.");
        }

        private void CopyBase64Button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_base64TextBox.Text))
            {
                MessageBox.Show(this, "Nao existe Base64 para copiar.", "Copiar Base64", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Clipboard.SetText(_base64TextBox.Text);
            Log("Base64 copiado para a area de transferencia.");
        }

        private void RefreshProvidersButton_Click(object sender, EventArgs e)
        {
            LoadProviders();
        }

        private void ProviderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateServiceFromSelectedProvider();
        }

        private void FingerprintService_StatusChanged(object sender, FingerprintStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<FingerprintStatusEventArgs>(FingerprintService_StatusChanged), sender, e);
                return;
            }

            _statusLabel.Text = "Status: " + e.Message;
            Log(e.Message);
        }

        private void FingerprintService_ImageCaptured(object sender, FingerprintImageEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<FingerprintImageEventArgs>(FingerprintService_ImageCaptured), sender, e);
                return;
            }

            var old = _fingerprintPictureBox.Image;
            _fingerprintPictureBox.Image = e.Image;
            if (old != null)
                old.Dispose();
        }

        private void SetCurrentTemplate(byte[] template, int? quality)
        {
            if (template == null)
            {
                _currentTemplate = null;
                _base64TextBox.Clear();
                _templateSizeLabel.Text = "Template: -";
                return;
            }

            _currentTemplate = new byte[template.Length];
            Buffer.BlockCopy(template, 0, _currentTemplate, 0, template.Length);
            _base64TextBox.Text = Convert.ToBase64String(_currentTemplate);
            _templateSizeLabel.Text = "Template: " + _currentTemplate.Length + " bytes";
            _qualityLabel.Text = quality.HasValue ? "Qualidade: " + quality.Value : "Qualidade: nao informada";
        }

        private void SetBusy(bool busy)
        {
            _enrollButton.Enabled = !busy && _fingerprintService != null;
            _verifyButton.Enabled = !busy && _fingerprintService != null;
            _saveButton.Enabled = !busy;
            _loadButton.Enabled = !busy;
            _refreshProvidersButton.Enabled = !busy;
            _providerComboBox.Enabled = !busy;
            _fakeDetectionCheckBox.Enabled = !busy;
            _lfdCheckBox.Enabled = !busy;
            _miotCheckBox.Enabled = !busy;
            _abortFakeCheckBox.Enabled = !busy;
            _farnNumericUpDown.Enabled = !busy;
            _maxModelsNumericUpDown.Enabled = !busy;
            _cancelButton.Enabled = busy;
        }

        private void SetOperationButtonsEnabled(bool enabled)
        {
            _enrollButton.Enabled = enabled;
            _verifyButton.Enabled = enabled;
            _cancelButton.Enabled = false;
        }

        private void DisposeCurrentService()
        {
            if (_operationCancellation != null)
            {
                _operationCancellation.Cancel();
                _operationCancellation.Dispose();
                _operationCancellation = null;
            }

            if (_fingerprintService != null)
            {
                _fingerprintService.StatusChanged -= FingerprintService_StatusChanged;
                _fingerprintService.ImageCaptured -= FingerprintService_ImageCaptured;
                _fingerprintService.Dispose();
                _fingerprintService = null;
            }
        }

        private void Log(string message)
        {
            if (_logTextBox == null)
                return;

            var line = DateTime.Now.ToString("HH:mm:ss") + " - " + message + Environment.NewLine;
            _logTextBox.AppendText(line);
        }

        private static string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "template";

            var builder = new StringBuilder(value.Length);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var ch in value)
            {
                bool isInvalid = false;
                for (int i = 0; i < invalid.Length; i++)
                {
                    if (invalid[i] == ch)
                    {
                        isInvalid = true;
                        break;
                    }
                }

                builder.Append(isInvalid ? '_' : ch);
            }

            return builder.ToString();
        }
    }
}
