using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraBars.Ribbon;
using ImageMagick;
using MagickScriptEditor;

namespace MagickCentral
{
    public partial class MagickForm : RibbonForm
    {
        private String extensionImage = "jpg";
        private String extensionMagickScript = "msl";

        private String filterImage =
            "Image Files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif|All Files (*.*)|*.*";

        private String filterMagickScript = "MagickScript Files (*.msl)|*.msl|All Files (*.*)|*.*";
        public MagickForm()
        {
            InitializeComponent();
            openScriptDialog.Filter = filterMagickScript;
            openScriptDialog.DefaultExt = extensionMagickScript;
            openImageDialog.Filter = filterImage;
            saveImageDialog.Filter = filterImage;
            openImageDialog.DefaultExt = extensionImage;
            saveImageDialog.DefaultExt = extensionImage;
        }

        private void OpenImageItemClick(object sender, ItemClickEventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                var imageForm = new ImageForm();
                imageForm.pictureEdit1.Image = Image.FromFile(openImageDialog.FileName);
                imageForm.MdiParent = this;
                imageForm.Text = openImageDialog.FileName;
                imageForm.Show();
                ActivateMdiChild(imageForm);
            }
        }

        private void NewScriptItemClick(object sender, ItemClickEventArgs e)
        {
            String[] args = { };
            var scriptForm = new MagickScriptForm(args);
            scriptForm.ShowDialog();
        }

        private void ExecuteScriptItemClick(object sender, ItemClickEventArgs e)
        {
            if (openScriptDialog.ShowDialog() == DialogResult.OK)
            {
                var script = new MagickScript(openScriptDialog.FileName);
                script.Read += OnScriptRead;
                script.Write += OnScriptWrite;
                script.Execute();
            }
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

        private void EditScriptItemClick(object sender, ItemClickEventArgs e)
        {
            if (openScriptDialog.ShowDialog() == DialogResult.OK)
            {
                String[] args = { openScriptDialog.FileName };
                var scriptForm = new MagickScriptForm(args);
                scriptForm.ShowDialog();
            }
        }

        private void AboutItemClick(object sender, ItemClickEventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        private void SaveImageItemClick(object sender, ItemClickEventArgs e)
        {
            ImageForm imageForm = xtraTabbedMdiManager1.ActiveFloatForm as ImageForm;
            if (imageForm != null)
            {
                saveImageDialog.FileName = imageForm.Text;
                if (saveImageDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap currentImage = imageForm.pictureEdit1.EditValue as Bitmap;
                    Debug.Assert(currentImage != null, "currentImage != null");
                    currentImage.Save(saveImageDialog.FileName);
                }
            }
        }

        private void MagickForm_Load(object sender, EventArgs e)
        {
            SkinHelper.InitSkinGallery(ribbonGalleryBarItem1, true);
        }
    }
}