using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraBars.Ribbon;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ImageMagick;

namespace MagickScriptEditor
{
    public partial class MagickScriptForm : RibbonForm
    {
        private static int _counter = 1;
        private readonly TextArea _textArea;
        private String _currentFileName;
        private String defaultMagickScript = "Empty.msl";
        private String extensionImage = "jpg";
        private String extensionMagickScript = "msl";

        private String filterImage =
            "Image Files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif|All Files (*.*)|*.*";

        private String filterMagickScript = "MagickScript Files (*.msl)|*.msl|All Files (*.*)|*.*";

        private String highlighting = "XML";
        private Boolean _isNew = true;
        CodeCompletionWindow _completionWindow;

        public MagickScriptForm(string[] args)
        {
            InitializeComponent();
            _textArea = textEditorControl.ActiveTextAreaControl.TextArea;
            openScriptDialog.Filter = filterMagickScript;
            saveScriptDialog.Filter = filterMagickScript;
            openScriptDialog.DefaultExt = extensionMagickScript;
            saveScriptDialog.DefaultExt = extensionMagickScript;
            openImageDialog.Filter = filterImage;
            saveImageDialog.Filter = filterImage;
            openImageDialog.DefaultExt = extensionImage;
            saveImageDialog.DefaultExt = extensionImage;
            if (args.Length != 0)
            {
                _currentFileName = args[0];
                textEditorControl.LoadFile(_currentFileName);
                _isNew = false;
            }
            else
            {
                _currentFileName = String.Format("Untitled {0}", _counter++);
                textEditorControl.LoadFile(defaultMagickScript);
                _isNew = true;
            }
            textEditorControl.SetHighlighting(highlighting);
            SetWindowTitle(_currentFileName);
            _textArea.KeyEventHandler += ProcessKey;
        }
        bool ProcessKey(char ch)
        {
            if (ch == '<')
            {
                ShowCompletionWindow();
            }
            return false;
        }
        void ShowCompletionWindow()
        {
            CompletionDataProvider completionDataProvider = new CompletionDataProvider();
            _completionWindow = CodeCompletionWindow.ShowCompletionWindow(this, textEditorControl, String.Empty, completionDataProvider, '<');
            if (_completionWindow != null)
            {
                _completionWindow.Closed += CompletionWindowClosed;
            }
        }

        void CompletionWindowClosed(object source, EventArgs e)
        {
            if (_completionWindow != null)
            {
                _completionWindow.Closed -= CompletionWindowClosed;
                _completionWindow.Dispose();
                _completionWindow = null;
            }
        }

        private void SetWindowTitle(String fileName)
        {
            Text = String.Format("{0} - Magick Script Editor", Path.GetFileName(fileName));
        }


        private void OpenItemClick(object sender, ItemClickEventArgs e)
        {
            if (openScriptDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFileName = openScriptDialog.FileName;
                textEditorControl.LoadFile(_currentFileName);
                textEditorControl.SetHighlighting(highlighting);
                _isNew = false;
                SetWindowTitle(_currentFileName);
            }
        }

        private void SaveItemClick(object sender, ItemClickEventArgs e)
        {
            if (_isNew)
            {
                saveScriptDialog.FileName = _currentFileName;
                if (saveScriptDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFileName = saveScriptDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            textEditorControl.SaveFile(_currentFileName);
            _isNew = false;
            SetWindowTitle(_currentFileName);
        }

        private void UndoItemClick(object sender, ItemClickEventArgs e)
        {
            textEditorControl.Undo();
        }

        private void RedoItemClick(object sender, ItemClickEventArgs e)
        {
            textEditorControl.Redo();
        }

        private void CutItemClick(object sender, ItemClickEventArgs e)
        {
            _textArea.ClipboardHandler.Cut(sender, e);
        }

        private void CopyItemClick(object sender, ItemClickEventArgs e)
        {
            _textArea.AutoClearSelection = false;
            _textArea.ClipboardHandler.Copy(sender, e);
        }

        private void PasteItemClick(object sender, ItemClickEventArgs e)
        {
            _textArea.ClipboardHandler.Paste(sender, e);
        }

        private void DeleteItemClick(object sender, ItemClickEventArgs e)
        {
            _textArea.ClipboardHandler.Delete(sender, e);
        }

        private void ExitItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private void ExecuteItemClick(object sender, ItemClickEventArgs e)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(_textArea.Document.TextContent));
            var script = new MagickScript(stream);
            script.Read += OnScriptRead;
            script.Write += OnScriptWrite;
            script.Execute();
        }

        private void OnScriptRead(object sender, ScriptReadEventArgs arguments)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                arguments.Image = new MagickImage(openImageDialog.FileName);
            }
        }

        private void OnScriptWrite(object sender, ScriptWriteEventArgs arguments)
        {
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                arguments.Image.Write(saveImageDialog.FileName);
            }
        }

        private void CheckItemClick(object sender, ItemClickEventArgs e)
        {
            var settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.ValidationEventHandler += ValidationCallBack;

            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(null, "MagickScript.xsd");

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(_textArea.Document.TextContent));
            var textReader = new XmlTextReader(stream);

            using (XmlReader validatingReader = XmlReader.Create(textReader, settings))
            {
                try
                {
                    while (validatingReader.Read())
                    {
                    }
                }
                catch (XmlException exception)
                {
                    Trace(exception.LineNumber, exception.LinePosition, exception.Message);
                }
            }
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            Trace(e.Exception.LineNumber, e.Exception.LinePosition, e.Exception.Message);
        }

        private static void Trace(int lineNumber, int linePosition, string message)
        {
            MessageBox.Show(String.Format("Line: {0}, Position: {1} \"{2}\"", lineNumber, linePosition, message));
        }

        private void MagickScriptForm_Load(object sender, EventArgs e)
        {
            SkinHelper.InitSkinGallery(ribbonGalleryBarItem1, true);
        }

        private void NewItemClick(object sender, ItemClickEventArgs e)
        {
            _currentFileName = String.Format("Untitled {0}", _counter++);
            textEditorControl.LoadFile(defaultMagickScript);
            textEditorControl.SetHighlighting(highlighting);
            _isNew = true;
            SetWindowTitle(_currentFileName);
        }

        private void ImageMagickItemClick(object sender, ItemClickEventArgs e)
        {
            Process.Start("http://www.imagemagick.org");
        }

        private void AboutItemClick(object sender, ItemClickEventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        private void SaveAsItemClick(object sender, ItemClickEventArgs e)
        {
            saveScriptDialog.FileName = _currentFileName;
            if (saveScriptDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFileName = saveScriptDialog.FileName;
            }
            else
            {
                return;
            }
            textEditorControl.SaveFile(_currentFileName);
            _isNew = false;
            SetWindowTitle(_currentFileName);
        }

        private void MagickScriptItemClick(object sender, ItemClickEventArgs e)
        {
            Process.Start("https://magick.codeplex.com/wikipage?title=MagickScript&referringTitle=Documentation");
        }
    }
}